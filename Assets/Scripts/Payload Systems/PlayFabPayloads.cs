using System.Collections.Generic;
using PlayFab.ClientModels;

// =============================================================================
// AUTH PAYLOADS
// =============================================================================

/// <summary>Published when any login method succeeds.</summary>
public class PlayFabLoginSuccessPayload
{
    public string PlayFabId { get; }
    public string DisplayName { get; }
    public bool IsNewAccount { get; }

    public PlayFabLoginSuccessPayload(string playFabId, string displayName, bool isNewAccount)
    {
        PlayFabId = playFabId;
        DisplayName = displayName;
        IsNewAccount = isNewAccount;
    }
}

/// <summary>Published when any login method fails.</summary>
public class PlayFabLoginFailedPayload
{
    public string ErrorMessage { get; }
    public PlayFabLoginFailedPayload(string errorMessage) => ErrorMessage = errorMessage;
}

/// <summary>
/// Published when a silent re-login attempt fails (stale or invalid session).
/// GameManager handles this by falling through to LoginMenu.
/// Separate from PlayFabLoginFailedPayload so LoginMenu doesn't react to it.
/// </summary>
public class PlayFabSilentLoginFailedPayload
{
    public string ErrorMessage { get; }
    public PlayFabSilentLoginFailedPayload(string errorMessage) => ErrorMessage = errorMessage;
}

/// <summary>Published when a new email account is successfully registered.</summary>
public class PlayFabRegisterSuccessPayload
{
    public string PlayFabId { get; }
    public PlayFabRegisterSuccessPayload(string playFabId) => PlayFabId = playFabId;
}

/// <summary>Published when email registration fails (e.g. email already in use).</summary>
public class PlayFabRegisterFailedPayload
{
    public string ErrorMessage { get; }
    public PlayFabRegisterFailedPayload(string errorMessage) => ErrorMessage = errorMessage;
}

/// <summary>Published when a guest account is successfully linked to Google or Apple.</summary>
public class PlayFabAccountLinkedPayload
{
    public string Provider { get; }
    public PlayFabAccountLinkedPayload(string provider) => Provider = provider;
}

/// <summary>Published when account linking fails.</summary>
public class PlayFabAccountLinkFailedPayload
{
    public string Provider { get; }
    public string ErrorMessage { get; }
    public PlayFabAccountLinkFailedPayload(string provider, string errorMessage)
    {
        Provider = provider;
        ErrorMessage = errorMessage;
    }
}

/// <summary>Published when the player logs out.</summary>
public class PlayFabLogoutPayload { }

/// <summary>Published when a display name is successfully updated.</summary>
public class PlayFabDisplayNameUpdatedPayload
{
    public string DisplayName { get; }
    public PlayFabDisplayNameUpdatedPayload(string displayName) => DisplayName = displayName;
}

// =============================================================================
// PLAYER DATA PAYLOADS
// =============================================================================

/// <summary>Published when all player UserData has been fetched from PlayFab.</summary>
public class PlayerDataLoadedPayload
{
    public int HighScore { get; }
    public string EquippedSkinId { get; }
    public bool SoundEnabled { get; }

    public PlayerDataLoadedPayload(int highScore, string equippedSkinId, bool soundEnabled)
    {
        HighScore = highScore;
        EquippedSkinId = equippedSkinId;
        SoundEnabled = soundEnabled;
    }
}

/// <summary>Published when the player sets a new high score.</summary>
public class PlayerHighScoreUpdatedPayload
{
    public int NewHighScore { get; }
    public PlayerHighScoreUpdatedPayload(int newHighScore) => NewHighScore = newHighScore;
}

/// <summary>Published when the player equips a different skin.</summary>
public class PlayerSkinEquippedPayload
{
    public string SkinItemId { get; }
    public PlayerSkinEquippedPayload(string skinItemId) => SkinItemId = skinItemId;
}

/// <summary>Published when global leaderboard data is fetched.</summary>
public class LeaderboardLoadedPayload
{
    public List<PlayerLeaderboardEntry> Entries { get; }
    public bool IsAroundPlayer { get; }

    public LeaderboardLoadedPayload(List<PlayerLeaderboardEntry> entries, bool isAroundPlayer)
    {
        Entries = entries;
        IsAroundPlayer = isAroundPlayer;
    }
}

// =============================================================================
// ECONOMY PAYLOADS
// =============================================================================

/// <summary>
/// Published whenever the player's account state is refreshed from PlayFab.
/// This is the single source of truth — any UI that shows currency, inventory,
/// or ownership should subscribe to this and redraw from it.
/// Covers: login load, post-purchase, post-grant, post-IAP, post-ad.
/// </summary>
public class PlayerAccountRefreshedPayload
{
    public int CoinBalance { get; }
    public int GemBalance { get; }
    public List<string> OwnedItemIds { get; }

    public PlayerAccountRefreshedPayload(int coinBalance, int gemBalance, List<string> ownedItemIds)
    {
        CoinBalance  = coinBalance;
        GemBalance   = gemBalance;
        OwnedItemIds = ownedItemIds;
    }
}

/// <summary>
/// Published when a CloudScript grant call completes successfully.
/// Carries what was granted so UI can show feedback ("+100 Coins!").
/// Always followed by a PlayerAccountRefreshedPayload once LoadInventory() completes.
/// </summary>
public class AccountGrantSuccessPayload
{
    public string ItemId { get; }        // PlayFab item ID, or currency code ("CN"/"GM")
    public int Amount { get; }           // 0 for non-stackable items
    public bool IsCurrency { get; }

    public AccountGrantSuccessPayload(string itemId, int amount, bool isCurrency)
    {
        ItemId     = itemId;
        Amount     = amount;
        IsCurrency = isCurrency;
    }
}

/// <summary>
/// Published when any purchase completes — currency buy, skin buy, IAP.
/// Subscribers only need to care about the item ID and how it was paid for.
/// Always followed by a PlayerAccountRefreshedPayload.
/// </summary>
public class PurchaseSuccessPayload
{
    public string ItemId { get; }
    public int CoinCost { get; }
    public int GemCost { get; }
    public bool WasRealMoney { get; }

    public PurchaseSuccessPayload(string itemId, int coinCost = 0, int gemCost = 0, bool wasRealMoney = false)
    {
        ItemId       = itemId;
        CoinCost     = coinCost;
        GemCost      = gemCost;
        WasRealMoney = wasRealMoney;
    }
}

/// <summary>
/// Published when any purchase or grant fails for any reason.
/// ItemId may be a currency code or catalog item ID.
/// </summary>
public class PurchaseFailedPayload
{
    public string ItemId { get; }
    public string ErrorMessage { get; }

    public PurchaseFailedPayload(string itemId, string errorMessage)
    {
        ItemId       = itemId;
        ErrorMessage = errorMessage;
    }
}

/// <summary>Published when a real-money IAP is validated and fulfilled by PlayFab.</summary>
public class IAPValidatedPayload
{
    public string ProductId { get; }
    public IAPValidatedPayload(string productId) => ProductId = productId;
}

/// <summary>Published when IAP validation fails.</summary>
public class IAPValidationFailedPayload
{
    public string ProductId { get; }
    public string ErrorMessage { get; }

    public IAPValidationFailedPayload(string productId, string errorMessage)
    {
        ProductId = productId;
        ErrorMessage = errorMessage;
    }
}