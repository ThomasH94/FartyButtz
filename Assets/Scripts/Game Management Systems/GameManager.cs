using UnityEngine;

/// <summary>
/// Game entry point. Owns the full startup sequence including silent re-login.
///
/// Launch flow:
///   A) Returning player (saved session exists):
///      Silent login attempt -> success -> load data -> MainMenu
///                           -> fail   -> clear session -> show LoginMenu
///
///   B) New player (no saved session):
///      Show LoginMenu immediately
///
/// Post-login (both flows):
///   PlayFabLoginSuccessPayload -> load PlayerData + Inventory in parallel
///   PlayerDataLoadedPayload    -> open MainMenu
/// </summary>
[DefaultExecutionOrder(-1)]
public class GameManager : SingletonMonoBehaviour<GameManager>
{
    protected override void Awake()
    {
        base.Awake();
        EventBus.Subscribe<PlayFabLoginSuccessPayload>(OnLoginSuccess);
        EventBus.Subscribe<PlayFabLoginFailedPayload>(OnLoginFailed);
        EventBus.Subscribe<PlayFabSilentLoginFailedPayload>(OnSilentLoginFailed);
        EventBus.Subscribe<PlayerDataLoadedPayload>(OnPlayerDataLoaded);
        EventBus.Subscribe<LogoutRequestPayload>(OnLogoutRequested);
        EventBus.Subscribe<PlayerSkinEquippedPayload>(OnSkinEquipped);
        EventBus.Subscribe<PlayerAccountRefreshedPayload>(OnAccountRefreshed);
    }

    private void Start()
    {
        if (SessionManager.Instance.HasSavedSession())
        {
            Debug.Log("[GameManager] Saved session found — attempting silent login...");
            AuthManager.Instance.LoginWithSavedSession();
        }
        else
        {
            Debug.Log("[GameManager] No saved session — showing login screen.");
            ShowLoginMenu();
        }
    }

    // -------------------------------------------------------------------------
    // POST-LOGIN SEQUENCE
    // PlayerData loads first, then inventory. Sequenced rather than parallel
    // so IsNewAccount and EquippedSkinId are always set before inventory callback runs.
    // -------------------------------------------------------------------------
    private void OnLoginSuccess(PlayFabLoginSuccessPayload payload)
    {
        Debug.Log($"[GameManager] Login OK — {payload.DisplayName}. Loading player data...");
        PlayerDataManager.Instance.LoadPlayerData();
    }

    private void OnPlayerDataLoaded(PlayerDataLoadedPayload payload)
    {
        Debug.Log("[GameManager] Player data loaded. Loading inventory...");
        EconomyManager.Instance.LoadInventory();
    }

    private void OnSkinEquipped(PlayerSkinEquippedPayload payload)
        => ApplySkin(payload.SkinItemId);

    // Called when inventory finishes loading — this is the true "ready" signal.
    // Only open MainMenu on the first load after login, not on every refresh.
    private bool m_HasOpenedMainMenu = false;

    private void OnAccountRefreshed(PlayerAccountRefreshedPayload payload)
    {
        if (m_HasOpenedMainMenu) return;
        m_HasOpenedMainMenu = true;

        Debug.Log("[GameManager] Inventory loaded. Game ready — opening main menu.");
        ApplySkin(PlayerDataManager.Instance.EquippedSkinId);
        EventBus.Publish(new MenuRequestOpenPayload(typeof(MainMenu), null));
    }

    private void ApplySkin(string skinItemId)
    {
        if (string.IsNullOrEmpty(skinItemId)) return;

        var skin = ButtDB.Instance?.GetByPlayFabId(skinItemId);
        if (skin == null)
        {
            Debug.LogWarning($"[GameManager] Equipped skin '{skinItemId}' not found in ButtDB.");
            return;
        }

        Debug.Log($"[GameManager] Applying skin: {skin.displayName}");
        EventBus.Publish(new SkinApplyRequestPayload(skin));
    }

    // -------------------------------------------------------------------------
    // FAILURE HANDLING
    // -------------------------------------------------------------------------

    /// <summary>
    /// A manual login attempt failed (wrong password, network error etc).
    /// LoginMenu owns the error display — nothing to do here.
    /// </summary>
    private void OnLoginFailed(PlayFabLoginFailedPayload payload)
    {
        Debug.LogWarning($"[GameManager] Login failed: {payload.ErrorMessage}");
    }

    /// <summary>
    /// Silent re-login failed — session was stale or revoked.
    /// Session is already cleared by AuthManager. Just show the login screen.
    /// </summary>
    private void OnSilentLoginFailed(PlayFabSilentLoginFailedPayload payload)
    {
        Debug.LogWarning($"[GameManager] Silent login failed: {payload.ErrorMessage}. Showing login screen.");
        ShowLoginMenu();
    }

    /// <summary>
    /// Triggered by anything publishing LogoutRequestPayload (settings button, debug menu, etc.)
    /// Clears the session, resets manager state, and returns to the login screen.
    /// </summary>
    private void OnLogoutRequested(LogoutRequestPayload payload)
    {
        Debug.Log("[GameManager] Logout requested.");
        m_HasOpenedMainMenu = false;
        PlayFabManager.Instance.Logout();
        ShowLoginMenu();
    }

    // -------------------------------------------------------------------------
    // HELPERS
    // -------------------------------------------------------------------------
    private void ShowLoginMenu()
    {
        EventBus.Publish(new MenuRequestOpenPayload(typeof(LoginMenu), null));
    }

    private void OnDestroy()
    {
        EventBus.Unsubscribe<PlayFabLoginSuccessPayload>(OnLoginSuccess);
        EventBus.Unsubscribe<PlayFabLoginFailedPayload>(OnLoginFailed);
        EventBus.Unsubscribe<PlayFabSilentLoginFailedPayload>(OnSilentLoginFailed);
        EventBus.Unsubscribe<PlayerDataLoadedPayload>(OnPlayerDataLoaded);
        EventBus.Unsubscribe<LogoutRequestPayload>(OnLogoutRequested);
        EventBus.Unsubscribe<PlayerSkinEquippedPayload>(OnSkinEquipped);
        EventBus.Unsubscribe<PlayerAccountRefreshedPayload>(OnAccountRefreshed);
    }
}