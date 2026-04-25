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

/// <summary>Published when currency balances and inventory are loaded/refreshed.</summary>
public class InventoryLoadedPayload
{
    public int CoinBalance { get; }
    public int GemBalance { get; }
    public List<string> OwnedItemIds { get; }

    public InventoryLoadedPayload(int coinBalance, int gemBalance, List<string> ownedItemIds)
    {
        CoinBalance = coinBalance;
        GemBalance = gemBalance;
        OwnedItemIds = ownedItemIds;
    }
}

/// <summary>Published when a cosmetic item is successfully purchased.</summary>
public class ItemPurchasedPayload
{
    public string ItemId { get; }
    public int CoinCost { get; }
    public int GemCost { get; }

    public ItemPurchasedPayload(string itemId, int coinCost, int gemCost)
    {
        ItemId = itemId;
        CoinCost = coinCost;
        GemCost = gemCost;
    }
}

/// <summary>Published when a purchase attempt fails.</summary>
public class ItemPurchaseFailedPayload
{
    public string ItemId { get; }
    public string ErrorMessage { get; }

    public ItemPurchaseFailedPayload(string itemId, string errorMessage)
    {
        ItemId = itemId;
        ErrorMessage = errorMessage;
    }
}

/// <summary>Published when a rewarded ad successfully grants currency.</summary>
public class AdRewardGrantedPayload
{
    public int CoinsGranted { get; }
    public AdRewardGrantedPayload(int coinsGranted) => CoinsGranted = coinsGranted;
}

/// <summary>Published when a real-money IAP is validated and fulfilled.</summary>
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