using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using System.Collections.Generic;
using Sirenix.Serialization;

/// <summary>
/// Handles the player's wallet and catalog using PlayFab's Legacy Economy API.
///
/// Currencies (set up in PlayFab portal > Economy > Currency (Legacy)):
///   CN = Coins  (earned in-game, ad rewards)
///   GM = Gems   (premium, bought via IAP)
///
/// Core philosophy:
///   - PlayFab is the source of truth for what the player owns
///   - This manager never reads local DBs to decide what to grant
///   - All grants go through CloudScript (server-authoritative)
///   - After any account change, LoadInventory() refreshes and publishes
///     PlayerAccountRefreshedPayload so all UI redraws from one event
///
/// Publishes: PlayerAccountRefreshedPayload, AccountGrantSuccessPayload,
///            PurchaseSuccessPayload, PurchaseFailedPayload,
///            IAPValidatedPayload, IAPValidationFailedPayload
/// </summary>
public class EconomyManager : SingletonMonoBehaviour<EconomyManager>
{
    #region New Player Account Items

    [OdinSerialize] private ButtData m_DefaultButtData = null;
    // Add more default items here as the game grows (starter coins, titles, etc.)

    #endregion

    // Cached — read locally, never poll PlayFab per-frame
    public int CoinBalance { get; private set; }
    public int GemBalance { get; private set; }
    public List<ItemInstance> OwnedItems { get; private set; } = new();
    public bool IsInventoryLoaded { get; private set; } = false;

    private const string CURRENCY_COINS   = "CN";
    private const string CURRENCY_GEMS    = "GM";
    // IMPORTANT! This exists in our cloud script as "main", which is CURRENTLY what our Catalog is named (we only have 1 catalog ATM)
    private const string CATALOG_VERSION  = "main"; // Must match Catalogs (Legacy) name in PlayFab portal

    protected override void Awake() => base.Awake();

    // -------------------------------------------------------------------------
    // LOAD — call after login, and after any account change
    // Fetches balances + inventory in one call, then publishes
    // PlayerAccountRefreshedPayload so all widgets redraw automatically.
    // -------------------------------------------------------------------------
    public void LoadInventory()
    {
        PlayFabClientAPI.GetUserInventory(
            new GetUserInventoryRequest(),
            result => {
                CoinBalance = result.VirtualCurrency.TryGetValue(CURRENCY_COINS, out int cn) ? cn : 0;
                GemBalance  = result.VirtualCurrency.TryGetValue(CURRENCY_GEMS,  out int gm) ? gm : 0;
                OwnedItems  = result.Inventory ?? new List<ItemInstance>();

                Debug.Log($"[Economy] Refreshed — Coins={CoinBalance}, Gems={GemBalance}, Items={OwnedItems.Count}");

                IsInventoryLoaded = true;

                if (PlayFabManager.Instance.IsNewAccount)
                    GiveNewAccountItems();

                PublishAccountRefreshed();
            },
            error => Debug.LogError($"[Economy] Inventory load failed: {error.ErrorMessage}")
        );
    }

    // -------------------------------------------------------------------------
    // NEW ACCOUNT GRANTS
    // Sends only the PlayFab Item ID to CloudScript — no local DB lookup here.
    // The client never decides what to grant; it just passes the ID.
    // -------------------------------------------------------------------------
    private void GiveNewAccountItems()
    {
        if (m_DefaultButtData == null)
        {
            Debug.LogWarning("[Economy] No DefaultButtData assigned.");
            return;
        }

        string itemId = m_DefaultButtData.PlayFabItemId;

        if (OwnsItem(itemId))
        {
            Debug.Log($"[Economy] Default item already owned: {itemId}");
            return;
        }

        PlayFabClientAPI.ExecuteCloudScript(
            new ExecuteCloudScriptRequest
            {
                FunctionName = "GiveNewAccountItems",
                FunctionParameter = new { ItemId = itemId },
                GeneratePlayStreamEvent = true
            },
            result => {
                Debug.Log("[Economy] New account items granted.");
                LoadInventory();
            },
            error => Debug.LogError($"[Economy] GiveNewAccountItems failed: {error.ErrorMessage}")
        );
    }

    // -------------------------------------------------------------------------
    // CHECK OWNERSHIP
    // Ask this before showing Buy vs Equip in any UI.
    // -------------------------------------------------------------------------
    public bool OwnsItem(string itemId)
        => OwnedItems.Exists(i => i.ItemId == itemId);

    // -------------------------------------------------------------------------
    // GRANT CURRENCY (test stub + ad reward)
    // Routes through CloudScript. Never call AddUserVirtualCurrency client-side.
    // -------------------------------------------------------------------------
    public void GrantCurrency(string currencyCode, int amount)
    {
        if (string.IsNullOrEmpty(currencyCode))
        {
            Debug.LogWarning("[Economy] GrantCurrency called with empty code.");
            return;
        }

        PlayFabClientAPI.ExecuteCloudScript(
            new ExecuteCloudScriptRequest
            {
                FunctionName = "GrantCurrency",
                FunctionParameter = new { currency = currencyCode, amount },
                GeneratePlayStreamEvent = true
            },
            result => {
                Debug.Log($"[Economy] Granted {amount} {currencyCode}.");
                LoadInventory();
                EventBus.Publish(new AccountGrantSuccessPayload(currencyCode, amount, isCurrency: true));
            },
            error => {
                Debug.LogError($"[Economy] GrantCurrency failed: {error.ErrorMessage}");
                EventBus.Publish(new PurchaseFailedPayload(currencyCode, error.ErrorMessage));
            }
        );
    }

    // -------------------------------------------------------------------------
    // PURCHASE SKIN WITH COINS
    // Price comes from ButtData.CoinCost — must match PlayFab catalog exactly.
    // -------------------------------------------------------------------------
    public void PurchaseSkinWithCoins(ButtData skin)
    {
        if (skin == null) return;

        if (CoinBalance < skin.CoinCost)
        {
            EventBus.Publish(new PurchaseFailedPayload(skin.PlayFabItemId, "Not enough coins."));
            return;
        }

        PlayFabClientAPI.PurchaseItem(
            new PurchaseItemRequest
            {
                ItemId          = skin.PlayFabItemId,
                VirtualCurrency = CURRENCY_COINS,
                Price           = skin.CoinCost,
                CatalogVersion  = CATALOG_VERSION
            },
            result => {
                Debug.Log($"[Economy] Skin purchased with Coins: {skin.PlayFabItemId}");
                LoadInventory();
                EventBus.Publish(new PurchaseSuccessPayload(skin.PlayFabItemId, coinCost: skin.CoinCost));
            },
            error => {
                Debug.LogError($"[Economy] Coin purchase failed: {error.ErrorMessage}");
                EventBus.Publish(new PurchaseFailedPayload(skin.PlayFabItemId, error.ErrorMessage));
            }
        );
    }

    // -------------------------------------------------------------------------
    // PURCHASE SKIN WITH GEMS
    // -------------------------------------------------------------------------
    public void PurchaseSkinWithGems(ButtData skin)
    {
        if (skin == null) return;

        if (GemBalance < skin.GemCost)
        {
            EventBus.Publish(new PurchaseFailedPayload(skin.PlayFabItemId, "Not enough gems."));
            return;
        }

        PlayFabClientAPI.PurchaseItem(
            new PurchaseItemRequest
            {
                ItemId          = skin.PlayFabItemId,
                VirtualCurrency = CURRENCY_GEMS,
                Price           = skin.GemCost,
                CatalogVersion  = CATALOG_VERSION
            },
            result => {
                Debug.Log($"[Economy] Skin purchased with Gems: {skin.PlayFabItemId}");
                LoadInventory();
                EventBus.Publish(new PurchaseSuccessPayload(skin.PlayFabItemId, gemCost: skin.GemCost));
            },
            error => {
                Debug.LogError($"[Economy] Gem purchase failed: {error.ErrorMessage}");
                EventBus.Publish(new PurchaseFailedPayload(skin.PlayFabItemId, error.ErrorMessage));
            }
        );
    }

    // -------------------------------------------------------------------------
    // GRANT AD REWARD
    // -------------------------------------------------------------------------
    public void GrantAdReward()
    {
        PlayFabClientAPI.ExecuteCloudScript(
            new ExecuteCloudScriptRequest
            {
                FunctionName = "GrantAdReward",
                GeneratePlayStreamEvent = true
            },
            result => {
                int coinsGranted = 0;
                if (result.FunctionResult != null)
                    int.TryParse(result.FunctionResult.ToString(), out coinsGranted);

                Debug.Log($"[Economy] Ad reward: +{coinsGranted} CN");
                LoadInventory();
                EventBus.Publish(new AccountGrantSuccessPayload(CURRENCY_COINS, coinsGranted, isCurrency: true));
            },
            error => Debug.LogError($"[Economy] Ad reward failed: {error.ErrorMessage}")
        );
    }

    // -------------------------------------------------------------------------
    // IAP RECEIPT VALIDATION
    // -------------------------------------------------------------------------
    public void ValidateIAP(string receipt, string productId)
    {
#if UNITY_ANDROID
        ValidateGoogleIAP(receipt, productId);
#elif UNITY_IOS
        ValidateAppleIAP(receipt, productId);
#else
        Debug.Log("[Economy] IAP validation skipped (Editor).");
        LoadInventory();
        EventBus.Publish(new IAPValidatedPayload(productId));
#endif
    }

    private void ValidateGoogleIAP(string receipt, string productId)
    {
        var wrapper = JsonUtility.FromJson<GoogleReceiptWrapper>(receipt);
        PlayFabClientAPI.ValidateGooglePlayPurchase(
            new ValidateGooglePlayPurchaseRequest { ReceiptJson = wrapper.Payload, Signature = wrapper.Signature },
            result => { Debug.Log("[Economy] Google IAP validated."); LoadInventory(); EventBus.Publish(new IAPValidatedPayload(productId)); },
            error  => { Debug.LogError($"[Economy] Google IAP failed: {error.ErrorMessage}"); EventBus.Publish(new IAPValidationFailedPayload(productId, error.ErrorMessage)); }
        );
    }

    private void ValidateAppleIAP(string receipt, string productId)
    {
        PlayFabClientAPI.ValidateIOSReceipt(
            new ValidateIOSReceiptRequest { ReceiptData = receipt, CurrencyCode = "USD", PurchasePrice = 0 },
            result => { Debug.Log("[Economy] Apple IAP validated."); LoadInventory(); EventBus.Publish(new IAPValidatedPayload(productId)); },
            error  => { Debug.LogError($"[Economy] Apple IAP failed: {error.ErrorMessage}"); EventBus.Publish(new IAPValidationFailedPayload(productId, error.ErrorMessage)); }
        );
    }

    // -------------------------------------------------------------------------
    // HELPERS
    // -------------------------------------------------------------------------
    private void PublishAccountRefreshed()
    {
        var ownedIds = new List<string>();
        foreach (var item in OwnedItems) ownedIds.Add(item.ItemId);
        EventBus.Publish(new PlayerAccountRefreshedPayload(CoinBalance, GemBalance, ownedIds));
    }

    [System.Serializable]
    private class GoogleReceiptWrapper
    {
        public string Store;
        public string TransactionID;
        public string Payload;
        public string Signature;
    }
}