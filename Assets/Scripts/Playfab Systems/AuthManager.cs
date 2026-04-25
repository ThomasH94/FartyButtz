using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;

/// <summary>
/// Handles all authentication flows:
///   - Silent re-login (saved session GUID via SessionManager)
///   - Guest login via device ID
///   - Email + password login
///   - Email + password registration
///   - Google Play Games (Android)
///   - Apple Sign-In (iOS)
///   - Account linking: guest -> Google/Apple
///   - Display name updates
///
/// Every successful login saves a session GUID via SessionManager so the next
/// launch can silently re-authenticate without prompting the player.
///
/// Publishes: PlayFabLoginSuccessPayload, PlayFabLoginFailedPayload,
///            PlayFabSilentLoginFailedPayload,
///            PlayFabRegisterSuccessPayload, PlayFabRegisterFailedPayload,
///            PlayFabAccountLinkedPayload, PlayFabAccountLinkFailedPayload,
///            PlayFabDisplayNameUpdatedPayload
/// </summary>
public class AuthManager : SingletonMonoBehaviour<AuthManager>
{
    protected override void Awake() => base.Awake();

    // -------------------------------------------------------------------------
    // SILENT RE-LOGIN
    // Called by GameManager on launch when SessionManager.HasSavedSession() is true.
    // Uses the stored GUID — no UI, no user input required.
    // On failure: clears the stale session and publishes PlayFabSilentLoginFailedPayload
    // so GameManager can fall back to showing LoginMenu.
    // -------------------------------------------------------------------------
    public void LoginWithSavedSession()
    {
        _pendingLoginType = PendingLoginType.Silent;
        string guid = SessionManager.Instance.GetSessionGuid();
        Debug.Log("[Auth] Attempting silent re-login...");

        PlayFabClientAPI.LoginWithCustomID(
            new LoginWithCustomIDRequest
            {
                CustomId = guid,
                CreateAccount = false,      // Never create an account silently
                InfoRequestParameters = BuildInfoParams()
            },
            OnLoginSuccess,
            error => {
                Debug.LogWarning($"[Auth] Silent re-login failed: {error.ErrorMessage}");
                SessionManager.Instance.ClearSession();
                EventBus.Publish(new PlayFabSilentLoginFailedPayload(error.ErrorMessage));
            }
        );
    }

    // -------------------------------------------------------------------------
    // GUEST LOGIN
    // Uses the device's unique ID. Safe first-launch default.
    // -------------------------------------------------------------------------
    public void LoginAsGuest()
    {
        _pendingLoginType = PendingLoginType.Guest;
        Debug.Log("[Auth] Attempting guest login...");
        PlayFabClientAPI.LoginWithCustomID(
            new LoginWithCustomIDRequest
            {
                CustomId = SystemInfo.deviceUniqueIdentifier,
                CreateAccount = true,
                InfoRequestParameters = BuildInfoParams()
            },
            OnLoginSuccess, OnLoginFailed
        );
    }

    // -------------------------------------------------------------------------
    // EMAIL LOGIN
    // -------------------------------------------------------------------------
    public void LoginWithEmail(string email, string password)
    {
        _pendingLoginType = PendingLoginType.Email;
        Debug.Log("[Auth] Attempting email login...");
        PlayFabClientAPI.LoginWithEmailAddress(
            new LoginWithEmailAddressRequest
            {
                Email = email,
                Password = password,
                InfoRequestParameters = BuildInfoParams()
            },
            OnLoginSuccess, OnLoginFailed
        );
    }

    // -------------------------------------------------------------------------
    // EMAIL REGISTRATION
    // Creates a new PlayFab account tied to an email + password.
    // On success, immediately logs in so the player enters the game.
    // -------------------------------------------------------------------------
    public void RegisterWithEmail(string email, string password, string displayName)
    {
        Debug.Log("[Auth] Attempting email registration...");
        PlayFabClientAPI.RegisterPlayFabUser(
            new RegisterPlayFabUserRequest
            {
                Email = email,
                Password = password,
                DisplayName = displayName,
                RequireBothUsernameAndEmail = false
            },
            result => {
                Debug.Log($"[Auth] Registration success: {result.PlayFabId}");
                EventBus.Publish(new PlayFabRegisterSuccessPayload(result.PlayFabId));
                LoginWithEmail(email, password);
            },
            error => {
                Debug.LogError($"[Auth] Registration failed: {error.ErrorMessage}");
                EventBus.Publish(new PlayFabRegisterFailedPayload(error.ErrorMessage));
            }
        );
    }

    // -------------------------------------------------------------------------
    // GOOGLE PLAY GAMES LOGIN (Android)
    // Requires Google Play Games Unity plugin.
    // Pass the token from: PlayGamesPlatform.Instance.GetServerAuthCode()
    // -------------------------------------------------------------------------
    public void LoginWithGoogle(string serverAuthCode)
    {
        _pendingLoginType = PendingLoginType.Platform;
        Debug.Log("[Auth] Attempting Google login...");
        PlayFabClientAPI.LoginWithGoogleAccount(
            new LoginWithGoogleAccountRequest
            {
                ServerAuthCode = serverAuthCode,
                CreateAccount = true,
                InfoRequestParameters = BuildInfoParams()
            },
            OnLoginSuccess, OnLoginFailed
        );
    }

    // -------------------------------------------------------------------------
    // APPLE SIGN-IN (iOS)
    // Requires Apple Sign In Unity plugin.
    // -------------------------------------------------------------------------
    public void LoginWithApple(string identityToken)
    {
        _pendingLoginType = PendingLoginType.Platform;
        Debug.Log("[Auth] Attempting Apple Sign-In...");
        PlayFabClientAPI.LoginWithApple(
            new LoginWithAppleRequest
            {
                IdentityToken = identityToken,
                CreateAccount = true,
                InfoRequestParameters = BuildInfoParams()
            },
            OnLoginSuccess, OnLoginFailed
        );
    }

    // -------------------------------------------------------------------------
    // ACCOUNT LINKING
    // Call while a guest session is active to permanently attach an identity.
    // -------------------------------------------------------------------------
    public void LinkGoogleAccount(string serverAuthCode)
    {
        PlayFabClientAPI.LinkGoogleAccount(
            new LinkGoogleAccountRequest { ServerAuthCode = serverAuthCode, ForceLink = false },
            result => {
                Debug.Log("[Auth] Google account linked.");
                EventBus.Publish(new PlayFabAccountLinkedPayload("Google"));
            },
            error => {
                Debug.LogError($"[Auth] Google link failed: {error.ErrorMessage}");
                EventBus.Publish(new PlayFabAccountLinkFailedPayload("Google", error.ErrorMessage));
            }
        );
    }

    public void LinkAppleAccount(string identityToken)
    {
        PlayFabClientAPI.LinkApple(
            new LinkAppleRequest { IdentityToken = identityToken, ForceLink = false },
            result => {
                Debug.Log("[Auth] Apple account linked.");
                EventBus.Publish(new PlayFabAccountLinkedPayload("Apple"));
            },
            error => {
                Debug.LogError($"[Auth] Apple link failed: {error.ErrorMessage}");
                EventBus.Publish(new PlayFabAccountLinkFailedPayload("Apple", error.ErrorMessage));
            }
        );
    }

    // -------------------------------------------------------------------------
    // DISPLAY NAME
    // -------------------------------------------------------------------------
    public void SetDisplayName(string name)
    {
        PlayFabClientAPI.UpdateUserTitleDisplayName(
            new UpdateUserTitleDisplayNameRequest { DisplayName = name },
            result => {
                Debug.Log($"[Auth] Display name updated: {result.DisplayName}");
                EventBus.Publish(new PlayFabDisplayNameUpdatedPayload(result.DisplayName));
            },
            error => Debug.LogError($"[Auth] Display name failed: {error.ErrorMessage}")
        );
    }

    // -------------------------------------------------------------------------
    // SHARED
    // -------------------------------------------------------------------------

    // Tracks the login method currently in flight so OnLoginSuccess knows
    // which GUID strategy to use without needing separate callbacks.
    private enum PendingLoginType { Silent, Guest, Email, Platform }
    private PendingLoginType _pendingLoginType = PendingLoginType.Guest;

    private void OnLoginSuccess(LoginResult result)
    {
        string displayName = result.InfoResultPayload?.PlayerProfile?.DisplayName ?? "Player";
        string loginTypeLabel = _pendingLoginType.ToString().ToLower();

        switch (_pendingLoginType)
        {
            case PendingLoginType.Silent:
                // Already have a valid session saved — just refresh the display name.
                // Don't touch the GUID, it's the one that just worked.
                SessionManager.Instance.SaveSession(
                    SessionManager.Instance.GetSessionGuid(), displayName, SessionManager.Instance.SavedLoginType);
                break;

            case PendingLoginType.Guest:
                // Device ID is the CustomId we used — save it directly.
                SessionManager.Instance.SaveSession(
                    SystemInfo.deviceUniqueIdentifier, displayName, loginTypeLabel);
                break;

            case PendingLoginType.Email:
            case PendingLoginType.Platform:
                // Generate a GUID and link it to this PlayFab account so silent
                // re-login via LoginWithCustomID works next launch.
                string guid = SessionManager.Instance.GenerateAndSaveSession(displayName, loginTypeLabel);
                LinkSessionGuid(guid);
                break;
        }

        PlayFabManager.Instance.SetLoginState(result.PlayFabId, displayName, result.NewlyCreated);
    }

    /// <summary>
    /// Links a generated GUID to the current PlayFab account as a CustomID.
    /// This is what makes silent re-login work for email and platform accounts.
    /// ForceLink = true because this account was just freshly authenticated —
    /// safe to overwrite any stale link from a previous install.
    /// </summary>
    private void LinkSessionGuid(string guid)
    {
        PlayFabClientAPI.LinkCustomID(
            new LinkCustomIDRequest { CustomId = guid, ForceLink = true },
            result => Debug.Log("[Auth] Session GUID linked to account."),
            error => {
                // Non-fatal — they're logged in, silent re-login just won't work
                // next launch and they'll see LoginMenu instead.
                Debug.LogWarning($"[Auth] Session GUID link failed: {error.ErrorMessage}");
                SessionManager.Instance.ClearSession();
            }
        );
    }

    private void OnLoginFailed(PlayFabError error)
    {
        _pendingLoginType = PendingLoginType.Guest; // Reset so retries start clean
        Debug.LogError($"[Auth] Login failed: {error.ErrorMessage}");
        EventBus.Publish(new PlayFabLoginFailedPayload(error.ErrorMessage));
    }

    private static GetPlayerCombinedInfoRequestParams BuildInfoParams() =>
        new GetPlayerCombinedInfoRequestParams
        {
            GetPlayerProfile   = true,
            GetUserAccountInfo = true
        };
}