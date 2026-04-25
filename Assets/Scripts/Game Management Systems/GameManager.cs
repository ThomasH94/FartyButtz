using UnityEngine;

/// <summary>
/// Game entry point.
///
/// Startup sequence:
///   1. PlayFabManager initializes (Awake, sets Title ID)
///   2. GameManager opens the LoginMenu — the player decides how to authenticate
///   3. LoginMenu calls AuthManager (guest / email / platform)
///   4. On PlayFabLoginSuccessPayload -> load player data + inventory in parallel
///   5. On PlayerDataLoadedPayload    -> game is fully ready, open MainMenu
///
/// The login decision lives in LoginMenu, not here. GameManager only reacts
/// to the result and drives the post-login data loading sequence.
/// </summary>
[DefaultExecutionOrder(-1)]
public class GameManager : SingletonMonoBehaviour<GameManager>
{
    protected override void Awake()
    {
        base.Awake();
        EventBus.Subscribe<PlayFabLoginSuccessPayload>(OnLoginSuccess);
        EventBus.Subscribe<PlayFabLoginFailedPayload>(OnLoginFailed);
        EventBus.Subscribe<PlayerDataLoadedPayload>(OnPlayerDataLoaded);
    }

    private void Start()
    {
        // Hand off to the player — they choose guest, email, or register
        EventBus.Publish(new MenuRequestOpenPayload(typeof(DebugInfoMenu), null));
        EventBus.Publish(new MenuRequestOpenPayload(typeof(LoginMenu), null));
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
        EventBus.Publish(new MenuRequestOpenPayload(typeof(MainMenu), null, true));
    }

    private void OnLoginFailed(PlayFabLoginFailedPayload payload)
    {
        Debug.LogError($"[GameManager] Login failed: {payload.ErrorMessage}");
        // LoginMenu handles showing the error to the player — nothing to do here
    }

    private void OnDestroy()
    {
        EventBus.Unsubscribe<PlayFabLoginSuccessPayload>(OnLoginSuccess);
        EventBus.Unsubscribe<PlayFabLoginFailedPayload>(OnLoginFailed);
        EventBus.Unsubscribe<PlayerDataLoadedPayload>(OnPlayerDataLoaded);
    }
}