using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;

/// <summary>
/// Handles all authentication flows:
///   - Guest login via device ID
///   - Email + password login
///   - Email + password registration
///   - Google Play Games (Android)
///   - Apple Sign-In (iOS)
///   - Account linking: guest -> Google/Apple
///   - Display name updates
///
/// Publishes: PlayFabLoginSuccessPayload, PlayFabLoginFailedPayload,
///            PlayFabRegisterSuccessPayload, PlayFabRegisterFailedPayload,
///            PlayFabAccountLinkedPayload, PlayFabAccountLinkFailedPayload,
///            PlayFabDisplayNameUpdatedPayload
/// </summary>
public class AuthManager : SingletonMonoBehaviour<AuthManager>
{
    protected override void Awake() => base.Awake();

    // -------------------------------------------------------------------------
    // GUEST LOGIN
    // Uses the device's unique ID. Safe first-launch default.
    // -------------------------------------------------------------------------
    public void LoginAsGuest()
    {
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
    // The caller (RegisterMenu) is responsible for any username/display name step.
    // -------------------------------------------------------------------------
    public void RegisterWithEmail(string email, string password, string displayName)
    {
        Debug.Log("[Auth] Attempting email registration...");
        PlayFabClientAPI.RegisterPlayFabUser(
            new RegisterPlayFabUserRequest
            {
                Email = email,
                Password = password,
                DisplayName = displayName,   // Set at registration — can be changed later
                RequireBothUsernameAndEmail = false
            },
            result => {
                Debug.Log($"[Auth] Registration success: {result.PlayFabId}");
                EventBus.Publish(new PlayFabRegisterSuccessPayload(result.PlayFabId));

                // Auto-login after registration so they go straight into the game
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
    private void OnLoginSuccess(LoginResult result)
    {
        string displayName = result.InfoResultPayload?.PlayerProfile?.DisplayName ?? "Player";
        PlayFabManager.Instance.SetLoginState(result.PlayFabId, displayName, result.NewlyCreated);
    }

    private void OnLoginFailed(PlayFabError error)
    {
        Debug.LogError($"[Auth] Login failed: {error.ErrorMessage}");
        EventBus.Publish(new PlayFabLoginFailedPayload(error.ErrorMessage));
    }

    private static GetPlayerCombinedInfoRequestParams BuildInfoParams() =>
        new GetPlayerCombinedInfoRequestParams
        {
            GetPlayerProfile = true,
            GetUserAccountInfo = true
        };
}