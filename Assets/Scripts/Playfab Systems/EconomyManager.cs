using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using System.Collections.Generic;

/// <summary>
/// Handles the player's wallet and catalog using PlayFab's Legacy Economy API.
///
/// Currencies (set up in PlayFab portal > Economy > Currencies):
///   CN = Coins  (earned in-game, rewarded by ads)
///   GM = Gems   (premium, bought via IAP)
///
/// Catalog / Inventory:
///   Items live in your PlayFab Catalog (v1). Each item has a price set in CN or GM.
///   The player's owned items live in their Character or User inventory.
///
/// IAP flow (never grant currency client-side):
///   Unity IAP -> receipt -> ValidateIAP() -> PlayFab verifies with Apple/Google
///   -> PlayFab grants the bundle you configured in your Store -> LoadCurrencies()
///
/// Publishes: InventoryLoadedPayload, ItemPurchasedPayload, ItemPurchaseFailedPayload,
///            AdRewardGrantedPayload, IAPValidatedPayload, IAPValidationFailedPayload
/// </summary>
public class EconomyManager : SingletonMonoBehaviour<EconomyManager>
{
    // Cached balances — read these locally
    public int CoinBalance { get; private set; }
    public int GemBalance { get; private set; }
    public List<ItemInstance> OwnedItems { get; private set; } = new();

    private const string CURRENCY_COINS = "CN";
    private const string CURRENCY_GEMS  = "GM";

    protected override void Awake() => base.Awake();

    // -------------------------------------------------------------------------
    // LOAD CURRENCIES + INVENTORY
    // Call after login. Fetches balances and owned catalog items in parallel,
    // then publishes InventoryLoadedPayload when both are ready.
    // -------------------------------------------------------------------------
    public void LoadInventory()
    {
        bool currenciesLoaded = false;
        bool itemsLoaded = false;

        // Fetch currency balances
        PlayFabClientAPI.GetUserInventory(
            new GetUserInventoryRequest(),
            result => {
                CoinBalance = result.VirtualCurrency.TryGetValue(CURRENCY_COINS, out int cn) ? cn : 0;
                GemBalance  = result.VirtualCurrency.TryGetValue(CURRENCY_GEMS,  out int gm) ? gm : 0;
                OwnedItems  = result.Inventory ?? new List<ItemInstance>();

                Debug.Log($"[Economy] Loaded — Coins={CoinBalance}, Gems={GemBalance}, Items={OwnedItems.Count}");

                currenciesLoaded = true;
                itemsLoaded = true; // GetUserInventory returns both currencies AND items
                PublishIfReady(ref currenciesLoaded, ref itemsLoaded);
            },
            error => {
                Debug.LogError($"[Economy] Inventory load failed: {error.ErrorMessage}");
                PublishIfReady(ref currenciesLoaded, ref itemsLoaded);
            }
        );

        // Note: GetUserInventory already returns everything (currency + items) in one call.
        // The two-bool gate above is kept for future cases where you split these calls.
        // For now it resolves immediately in the single callback above.
    }

    // -------------------------------------------------------------------------
    // CHECK OWNERSHIP
    // Use in shop UI to decide: show "Buy" or "Equip"
    // -------------------------------------------------------------------------
    public bool OwnsItem(string itemId)
    {
        return OwnedItems.Exists(i => i.ItemId == itemId);
    }

    // -------------------------------------------------------------------------
    // PURCHASE WITH COINS (CN)
    // Price must match the catalog entry in PlayFab portal.
    // -------------------------------------------------------------------------
    public void PurchaseWithCoins(string itemId, string catalogVersion = "")
    {
        if (!CanAffordCoins(itemId)) return;

        PlayFabClientAPI.PurchaseItem(
            new PurchaseItemRequest
            {
                ItemId = itemId,
                VirtualCurrency = CURRENCY_COINS,
                Price = GetCatalogPriceCN(itemId), // see helper below
                CatalogVersion = catalogVersion
            },
            result => {
                Debug.Log($"[Economy] Purchased with Coins: {itemId}");
                LoadInventory(); // Refresh balances
                EventBus.Publish(new ItemPurchasedPayload(itemId, coinCost: GetCatalogPriceCN(itemId), gemCost: 0));
            },
            error => {
                Debug.LogError($"[Economy] Coin purchase failed: {error.ErrorMessage}");
                EventBus.Publish(new ItemPurchaseFailedPayload(itemId, error.ErrorMessage));
            }
        );
    }

    // -------------------------------------------------------------------------
    // PURCHASE WITH GEMS (GM)
    // -------------------------------------------------------------------------
    public void PurchaseWithGems(string itemId, int gemCost, string catalogVersion = "")
    {
        if (GemBalance < gemCost)
        {
            Debug.Log("[Economy] Not enough gems.");
            EventBus.Publish(new ItemPurchaseFailedPayload(itemId, "Not enough gems."));
            return;
        }

        PlayFabClientAPI.PurchaseItem(
            new PurchaseItemRequest
            {
                ItemId = itemId,
                VirtualCurrency = CURRENCY_GEMS,
                Price = gemCost,
                CatalogVersion = catalogVersion
            },
            result => {
                Debug.Log($"[Economy] Purchased with Gems: {itemId}");
                LoadInventory();
                EventBus.Publish(new ItemPurchasedPayload(itemId, coinCost: 0, gemCost: gemCost));
            },
            error => {
                Debug.LogError($"[Economy] Gem purchase failed: {error.ErrorMessage}");
                EventBus.Publish(new ItemPurchaseFailedPayload(itemId, error.ErrorMessage));
            }
        );
    }

    // -------------------------------------------------------------------------
    // GRANT AD REWARD
    // Calls a CloudScript function server-side — coins are granted there, not here.
    // Create a CloudScript function named "GrantAdReward" that calls
    // server.AddUserVirtualCurrency({ PlayFabId, VirtualCurrency: "CN", Amount: 50 })
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
                // The CloudScript returns how many coins were granted
                int coinsGranted = 0;
                if (result.FunctionResult != null)
                    int.TryParse(result.FunctionResult.ToString(), out coinsGranted);

                Debug.Log($"[Economy] Ad reward granted: +{coinsGranted} coins");
                LoadInventory(); // Refresh CN balance
                EventBus.Publish(new AdRewardGrantedPayload(coinsGranted));
            },
            error => Debug.LogError($"[Economy] Ad reward failed: {error.ErrorMessage}")
        );
    }

    // -------------------------------------------------------------------------
    // IAP RECEIPT VALIDATION
    // Call from Unity IAP's ProcessPurchase callback:
    //   EconomyManager.Instance.ValidateIAP(args.purchasedProduct.receipt, args.purchasedProduct.definition.id);
    //
    // PlayFab validates server-to-server, then grants the bundle you configured
    // in PlayFab portal > Economy > Stores to match that product ID.
    // -------------------------------------------------------------------------
    public void ValidateIAP(string receipt, string productId)
    {
#if UNITY_ANDROID
        ValidateGoogleIAP(receipt, productId);
#elif UNITY_IOS
        ValidateAppleIAP(receipt, productId);
#else
        // In-editor: skip validation and simulate success for testing
        Debug.Log("[Economy] IAP validation skipped (Editor).");
        LoadInventory();
        EventBus.Publish(new IAPValidatedPayload(productId));
#endif
    }

    private void ValidateGoogleIAP(string receipt, string productId)
    {
        var wrapper = JsonUtility.FromJson<GoogleReceiptWrapper>(receipt);

        PlayFabClientAPI.ValidateGooglePlayPurchase(
            new ValidateGooglePlayPurchaseRequest
            {
                ReceiptJson = wrapper.Payload,
                Signature   = wrapper.Signature
            },
            result => {
                Debug.Log("[Economy] Google IAP validated.");
                LoadInventory();
                EventBus.Publish(new IAPValidatedPayload(productId));
            },
            error => {
                Debug.LogError($"[Economy] Google IAP failed: {error.ErrorMessage}");
                EventBus.Publish(new IAPValidationFailedPayload(productId, error.ErrorMessage));
            }
        );
    }

    private void ValidateAppleIAP(string receipt, string productId)
    {
        PlayFabClientAPI.ValidateIOSReceipt(
            new ValidateIOSReceiptRequest
            {
                ReceiptData   = receipt,
                CurrencyCode  = "USD",
                PurchasePrice = 0
            },
            result => {
                Debug.Log("[Economy] Apple IAP validated.");
                LoadInventory();
                EventBus.Publish(new IAPValidatedPayload(productId));
            },
            error => {
                Debug.LogError($"[Economy] Apple IAP failed: {error.ErrorMessage}");
                EventBus.Publish(new IAPValidationFailedPayload(productId, error.ErrorMessage));
            }
        );
    }

    // -------------------------------------------------------------------------
    // HELPERS
    // -------------------------------------------------------------------------
    private void PublishIfReady(ref bool a, ref bool b)
    {
        if (!a || !b) return;
        var ownedIds = new List<string>();
        foreach (var item in OwnedItems) ownedIds.Add(item.ItemId);
        EventBus.Publish(new InventoryLoadedPayload(CoinBalance, GemBalance, ownedIds));
    }

    /// <summary>
    /// In a real implementation you'd cache the catalog on startup and look up
    /// prices here. For now, fetch the price before calling PurchaseWithCoins,
    /// or hardcode prices in a ScriptableObject shop config.
    /// </summary>
    private int GetCatalogPriceCN(string itemId) => 0; // TODO: replace with catalog lookup

    private bool CanAffordCoins(string itemId)
    {
        int price = GetCatalogPriceCN(itemId);
        if (CoinBalance >= price) return true;
        EventBus.Publish(new ItemPurchaseFailedPayload(itemId, "Not enough coins."));
        return false;
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
