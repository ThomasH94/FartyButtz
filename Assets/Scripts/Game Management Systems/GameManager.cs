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
    // -------------------------------------------------------------------------
    private void OnLoginSuccess(PlayFabLoginSuccessPayload payload)
    {
        Debug.Log($"[GameManager] Login OK — {payload.DisplayName}. Loading data...");
        PlayerDataManager.Instance.LoadPlayerData();
        EconomyManager.Instance.LoadInventory();
    }

    private void OnPlayerDataLoaded(PlayerDataLoadedPayload payload)
    {
        Debug.Log("[GameManager] All data loaded. Opening main menu.");
        EventBus.Publish(new MenuRequestOpenPayload(typeof(MainMenu), null));
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
    }
}