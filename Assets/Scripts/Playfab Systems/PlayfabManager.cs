using UnityEngine;
using PlayFab;

/// <summary>
/// Core PlayFab singleton. Sets the Title ID and exposes login state.
/// GameManager calls AuthManager.Instance.LoginAsGuest() on Start.
/// </summary>
public class PlayFabManager : SingletonMonoBehaviour<PlayFabManager>
{
    [Header("PlayFab Settings")]
    [Tooltip("Your Title ID from the PlayFab developer portal")]
    [SerializeField] private string titleID = "YOUR_TITLE_ID";

    public bool IsLoggedIn { get; private set; } = false;
    public string PlayFabId { get; private set; }
    public string DisplayName { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        InitializePlayFab();
    }

    private void InitializePlayFab()
    {
        if (string.IsNullOrEmpty(PlayFabSettings.staticSettings.TitleId))
            PlayFabSettings.staticSettings.TitleId = titleID;

        Debug.Log($"[PlayFab] Initialized. Title ID: {PlayFabSettings.staticSettings.TitleId}");
    }

    /// <summary>Called by AuthManager after any successful login.</summary>
    public void SetLoginState(string playfabId, string displayName, bool isNewAccount)
    {
        PlayFabId = playfabId;
        DisplayName = displayName;
        IsLoggedIn = true;
        EventBus.Publish(new PlayFabLoginSuccessPayload(playfabId, displayName, isNewAccount));
        Debug.Log($"[PlayFab] Logged in — {displayName} ({playfabId}), newAccount={isNewAccount}");
    }

    public void Logout()
    {
        PlayFabClientAPI.ForgetAllCredentials();
        PlayFabId = null;
        DisplayName = null;
        IsLoggedIn = false;
        EventBus.Publish(new PlayFabLogoutPayload());
        Debug.Log("[PlayFab] Logged out.");
    }
}