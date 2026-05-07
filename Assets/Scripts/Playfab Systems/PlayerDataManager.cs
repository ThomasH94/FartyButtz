using System;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using System.Collections.Generic;

/// <summary>
/// Reads and writes player-specific UserData and Statistics to PlayFab.
///
/// What lives here vs EconomyManager:
///   - HERE:     High score, equipped skin ID, settings (what you've done/set)
///   - ECONOMY:  Currency balances, owned item IDs (what you own/have bought)
///
/// Publishes: PlayerDataLoadedPayload, PlayerHighScoreUpdatedPayload,
///            PlayerSkinEquippedPayload, LeaderboardLoadedPayload
/// </summary>
public class PlayerDataManager : SingletonMonoBehaviour<PlayerDataManager>
{
    // Cached values — read these locally, don't poll PlayFab
    public int HighScore { get; private set; }
    public string EquippedSkinId { get; private set; } = string.Empty; // Empty until loaded from PlayFab
    public bool SoundEnabled { get; private set; } = true;

    // UserData keys
    private const string KEY_HIGH_SCORE    = "HighScore";
    private const string KEY_EQUIPPED_SKIN = "EquippedSkin";
    private const string KEY_SOUND_ENABLED = "SoundEnabled";

    // Statistic name (must match what you create in PlayFab portal > Leaderboards)
    private const string STAT_HIGH_SCORE = "HighScore";

    protected override void Awake() => base.Awake();

    private void OnEnable()
    {
        EventBus.Subscribe<SkinApplyRequestPayload>(OnSkinUpdated);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<SkinApplyRequestPayload>(OnSkinUpdated);
    }
    
    private void OnSkinUpdated(SkinApplyRequestPayload payload)
    {
        SetEquippedSkin(payload.SkinData.PlayFabItemId);
    }

    // -------------------------------------------------------------------------
    // LOAD — call once after login (GameManager handles sequencing)
    // -------------------------------------------------------------------------
    public void LoadPlayerData()
    {
        PlayFabClientAPI.GetUserData(
            new GetUserDataRequest
            {
                Keys = new List<string> { KEY_HIGH_SCORE, KEY_EQUIPPED_SKIN, KEY_SOUND_ENABLED }
            },
            result => {
                if (result.Data.TryGetValue(KEY_HIGH_SCORE, out var hs))
                    if (int.TryParse(hs.Value, out int parsed))
                        HighScore = parsed;

                if (result.Data.TryGetValue(KEY_EQUIPPED_SKIN, out var skin))
                    EquippedSkinId = skin.Value;

                if (result.Data.TryGetValue(KEY_SOUND_ENABLED, out var sound))
                    SoundEnabled = sound.Value == "true";

                Debug.Log($"[PlayerData] Loaded — HighScore={HighScore}, Skin={EquippedSkinId}");
                EventBus.Publish(new PlayerDataLoadedPayload(HighScore, EquippedSkinId, SoundEnabled));
            },
            error => {
                Debug.LogError($"[PlayerData] Load failed: {error.ErrorMessage}");
                // Still publish so the bootstrap doesn't hang waiting
                EventBus.Publish(new PlayerDataLoadedPayload(HighScore, EquippedSkinId, SoundEnabled));
            }
        );
    }

    // -------------------------------------------------------------------------
    // HIGH SCORE — only writes if it's a genuine new record
    // Saves to UserData AND updates the leaderboard Statistic in one go.
    // -------------------------------------------------------------------------
    public void SubmitScore(int score)
    {
        if (score <= HighScore) return;

        HighScore = score;

        PlayFabClientAPI.UpdateUserData(
            new UpdateUserDataRequest
            {
                Data = new Dictionary<string, string> { { KEY_HIGH_SCORE, score.ToString() } }
            },
            null,
            error => Debug.LogError($"[PlayerData] Score save failed: {error.ErrorMessage}")
        );

        PlayFabClientAPI.UpdatePlayerStatistics(
            new UpdatePlayerStatisticsRequest
            {
                Statistics = new List<StatisticUpdate>
                {
                    new StatisticUpdate { StatisticName = STAT_HIGH_SCORE, Value = score }
                }
            },
            result => {
                Debug.Log($"[PlayerData] Leaderboard updated: {score}");
                EventBus.Publish(new PlayerHighScoreUpdatedPayload(score));
            },
            error => Debug.LogError($"[PlayerData] Stat update failed: {error.ErrorMessage}")
        );
    }

    // -------------------------------------------------------------------------
    // EQUIPPED SKIN
    // EconomyManager confirms ownership — this just tracks the active selection.
    // -------------------------------------------------------------------------
    public void SetEquippedSkin(string skinItemId)
    {
        EquippedSkinId = skinItemId;

        PlayFabClientAPI.UpdateUserData(
            new UpdateUserDataRequest
            {
                Data = new Dictionary<string, string> { { KEY_EQUIPPED_SKIN, skinItemId } }
            },
            result => {
                Debug.Log($"[PlayerData] Equipped skin saved: {skinItemId}");
                EventBus.Publish(new PlayerSkinEquippedPayload(skinItemId));
            },
            error => Debug.LogError($"[PlayerData] Equip save failed: {error.ErrorMessage}")
        );
    }

    /// <summary>
    /// Returns the ButtData for the currently equipped skin.
    /// Always looks up by PlayFab Item ID — the canonical identifier.
    /// Falls back to ButtDB.DefaultSkin if nothing is equipped or the ID
    /// isn't found (e.g. first login before GiveNewAccountItems completes).
    /// </summary>
    public ButtData GetEquippedSkin()
    {
        if (!string.IsNullOrEmpty(EquippedSkinId))
        {
            var skin = ButtDB.Instance?.GetByPlayFabId(EquippedSkinId);
            if (skin != null) return skin;

            Debug.LogWarning($"[PlayerData] Equipped skin '{EquippedSkinId}' not found in ButtDB — using default.");
        }

        return ButtDB.Instance?.DefaultSkin;
    }

    // -------------------------------------------------------------------------
    // SETTINGS
    // -------------------------------------------------------------------------
    public void SetSoundEnabled(bool enabled)
    {
        SoundEnabled = enabled;

        PlayFabClientAPI.UpdateUserData(
            new UpdateUserDataRequest
            {
                Data = new Dictionary<string, string> { { KEY_SOUND_ENABLED, enabled ? "true" : "false" } }
            },
            null,
            error => Debug.LogError($"[PlayerData] Settings save failed: {error.ErrorMessage}")
        );
    }

    // -------------------------------------------------------------------------
    // LEADERBOARD
    // -------------------------------------------------------------------------
    public void GetTopScores(int maxResults = 10)
    {
        PlayFabClientAPI.GetLeaderboard(
            new GetLeaderboardRequest
            {
                StatisticName = STAT_HIGH_SCORE,
                StartPosition = 0,
                MaxResultsCount = maxResults
            },
            result => EventBus.Publish(new LeaderboardLoadedPayload(result.Leaderboard, isAroundPlayer: false)),
            error => Debug.LogError($"[PlayerData] Leaderboard fetch failed: {error.ErrorMessage}")
        );
    }

    public void GetScoreAroundPlayer(int radius = 5)
    {
        PlayFabClientAPI.GetLeaderboardAroundPlayer(
            new GetLeaderboardAroundPlayerRequest
            {
                StatisticName = STAT_HIGH_SCORE,
                MaxResultsCount = radius * 2 + 1
            },
            result => EventBus.Publish(new LeaderboardLoadedPayload(result.Leaderboard, isAroundPlayer: true)),
            error => Debug.LogError($"[PlayerData] Player leaderboard failed: {error.ErrorMessage}")
        );
    }
}