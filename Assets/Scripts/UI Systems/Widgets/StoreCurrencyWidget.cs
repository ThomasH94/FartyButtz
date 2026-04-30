using Sirenix.OdinInspector;
using Sirenix.Serialization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Data passed to a StoreCurrencyWidget to configure a single currency pack.
/// CurrencyCode matches PlayFab's currency codes ("CN" = Coins, "GM" = Gems).
/// GrantAmount is how much to add via CloudScript (test stub).
/// DisplayPrice is shown to the player (real money — not charged until IAP is wired).
/// </summary>
public class StoreCurrencyWidgetData
{
    public string CurrencyCode { get; }
    public float DisplayPrice { get; }
    public int GrantAmount { get; }
    public string Label { get; }

    public StoreCurrencyWidgetData(string currencyCode, float displayPrice, int grantAmount, string label = "")
    {
        CurrencyCode  = currencyCode;
        DisplayPrice  = displayPrice;
        GrantAmount   = grantAmount;
        Label         = string.IsNullOrEmpty(label) ? $"{grantAmount} {currencyCode}" : label;
    }
}

/// <summary>
/// A single purchasable currency pack widget in the store.
///
/// OnTryPurchase stubs through EconomyManager.GrantCurrency (CloudScript).
/// Replace with EconomyManager.ValidateIAP() when real money is ready.
///
/// Reacts to AccountGrantSuccessPayload to re-enable the button and show feedback.
/// Reacts to PurchaseFailedPayload to re-enable and surface the error.
/// </summary>
public class StoreCurrencyWidget : SerializedMonoBehaviour
{
    [Header("Debug Icons")]
    [OdinSerialize] private Sprite m_DebugCoinIcon = null;
    [OdinSerialize] private Sprite m_DebugGemIcon  = null;
    [OdinSerialize] private Sprite m_CoinBackground = null;
    [OdinSerialize] private Sprite m_GemBackground = null;

    [Header("UI References")]
    [OdinSerialize] private Image           m_CurrencyIcon      = null;
    [OdinSerialize] private Image           m_WidgetIcon        = null;
    [OdinSerialize] private Image m_WidgetBackground = null;
    [OdinSerialize] private TextMeshProUGUI m_CurrencyAmountTMP = null;
    [OdinSerialize] private TextMeshProUGUI m_PurchaseCostTMP   = null;
    [OdinSerialize] private ExtendedButton  m_PurchaseButton    = null;

    private StoreCurrencyWidgetData m_WidgetData;

    private const string CURRENCY_COINS = "CN";

    public void Setup(StoreCurrencyWidgetData data)
    {
        m_WidgetData = data;

        m_CurrencyAmountTMP.text = m_WidgetData.Label;
        m_PurchaseCostTMP.text   = $"${m_WidgetData.DisplayPrice:0.00}";
        m_CurrencyIcon.sprite    = m_WidgetData.CurrencyCode == CURRENCY_COINS
            ? m_DebugCoinIcon
            : m_DebugGemIcon;
        
        m_WidgetIcon.sprite    = m_WidgetData.CurrencyCode == CURRENCY_COINS
            ? m_DebugCoinIcon
            : m_DebugGemIcon;
        
        m_WidgetBackground.sprite    = m_WidgetData.CurrencyCode == CURRENCY_COINS
            ? m_CoinBackground
            : m_GemBackground;


        m_PurchaseButton.RegisterClickAction(OnTryPurchase);
    }

    private void OnEnable()
    {
        EventBus.Subscribe<AccountGrantSuccessPayload>(OnGrantSuccess);
        EventBus.Subscribe<PurchaseFailedPayload>(OnPurchaseFailed);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<AccountGrantSuccessPayload>(OnGrantSuccess);
        EventBus.Unsubscribe<PurchaseFailedPayload>(OnPurchaseFailed);
    }

    // -------------------------------------------------------------------------
    // PURCHASE (stub)
    // Swap GrantCurrency for ValidateIAP once real money is wired up.
    // -------------------------------------------------------------------------
    private void OnTryPurchase()
    {
        if (m_WidgetData == null) return;
        m_PurchaseButton.interactable = false;
        EconomyManager.Instance.GrantCurrency(m_WidgetData.CurrencyCode, m_WidgetData.GrantAmount);
    }

    private void OnGrantSuccess(AccountGrantSuccessPayload payload)
    {
        if (payload.ItemId != m_WidgetData?.CurrencyCode) return;
        m_PurchaseButton.interactable = true;
        Debug.Log($"[StoreCurrencyWidget] +{payload.Amount} {payload.ItemId} confirmed.");
    }

    private void OnPurchaseFailed(PurchaseFailedPayload payload)
    {
        if (payload.ItemId != m_WidgetData?.CurrencyCode) return;
        m_PurchaseButton.interactable = true;
        Debug.LogWarning($"[StoreCurrencyWidget] Purchase failed: {payload.ErrorMessage}");
    }
}