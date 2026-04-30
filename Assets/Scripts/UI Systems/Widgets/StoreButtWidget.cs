using Sirenix.OdinInspector;
using Sirenix.Serialization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Displays a single skin in the store and handles purchasing.
///
/// State machine:
///   Owned    → shows "Equip" button (or "Equipped" if active)
///   Not owned, can afford  → shows "Buy" button with price
///   Not owned, can't afford → shows price, button disabled
///
/// Reacts to PlayerAccountRefreshedPayload to stay in sync after any
/// account change without needing to be explicitly refreshed by the parent.
///
/// Setup: call Setup(buttData) after instantiation.
/// </summary>
public class StoreButtWidget : SerializedMonoBehaviour
{
    [Header("UI References")]
    [OdinSerialize] private Image           m_SkinPreview   = null;
    [OdinSerialize] private TextMeshProUGUI m_NameTMP       = null;
    [OdinSerialize] private TextMeshProUGUI m_PriceTMP      = null;
    [OdinSerialize] private TextMeshProUGUI m_ButtonLabelTMP = null;
    [OdinSerialize] private ExtendedButton  m_ActionButton  = null;
    [OdinSerialize] private GameObject      m_OwnedBadge    = null; // optional "OWNED" overlay

    private ButtData m_SkinData;

    public void Setup(ButtData skin)
    {
        m_SkinData = skin;
        m_ActionButton.RegisterClickAction(OnActionClicked);
        Refresh();
    }

    private void OnEnable()
    {
        EventBus.Subscribe<PlayerAccountRefreshedPayload>(OnAccountRefreshed);
        EventBus.Subscribe<PlayerSkinEquippedPayload>(OnSkinEquipped);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<PlayerAccountRefreshedPayload>(OnAccountRefreshed);
        EventBus.Unsubscribe<PlayerSkinEquippedPayload>(OnSkinEquipped);
    }

    // -------------------------------------------------------------------------
    // STATE
    // -------------------------------------------------------------------------
    private void Refresh()
    {
        if (m_SkinData == null) return;

        bool owned    = EconomyManager.Instance.OwnsItem(m_SkinData.PlayFabItemId);
        bool equipped = PlayerDataManager.Instance.EquippedSkinId == m_SkinData.PlayFabItemId;

        // Preview
        if (m_SkinPreview != null)
            m_SkinPreview.sprite = m_SkinData.ButtSprite;

        if (m_NameTMP != null)
            m_NameTMP.text = m_SkinData.displayName;

        if (m_OwnedBadge != null)
            m_OwnedBadge.SetActive(owned);

        if (owned)
        {
            m_PriceTMP.gameObject.SetActive(false);
            m_ButtonLabelTMP.text     = equipped ? "Equipped" : "Equip";
            m_ActionButton.interactable = !equipped;
        }
        else
        {
            m_PriceTMP.gameObject.SetActive(true);
            m_PriceTMP.text = BuildPriceString();

            bool canAfford            = CanAfford();
            m_ButtonLabelTMP.text     = "Buy";
            m_ActionButton.interactable = canAfford;
        }
    }

    // -------------------------------------------------------------------------
    // ACTIONS
    // -------------------------------------------------------------------------
    private void OnActionClicked()
    {
        if (m_SkinData == null) return;

        bool owned = EconomyManager.Instance.OwnsItem(m_SkinData.PlayFabItemId);

        if (owned)
        {
            // Equip — local save only, no PlayFab purchase needed
            PlayerDataManager.Instance.SetEquippedSkin(m_SkinData.PlayFabItemId);
        }
        else
        {
            // Buy — prefer coins, fall back to gems
            m_ActionButton.interactable = false;

            if (m_SkinData.CoinCost > 0 && EconomyManager.Instance.CoinBalance >= m_SkinData.CoinCost)
                EconomyManager.Instance.PurchaseSkinWithCoins(m_SkinData);
            else if (m_SkinData.GemCost > 0)
                EconomyManager.Instance.PurchaseSkinWithGems(m_SkinData);
            else
                Debug.LogWarning($"[StoreSkinWidget] {m_SkinData.PlayFabItemId} has no valid purchase path.");
        }
    }

    // -------------------------------------------------------------------------
    // EVENT HANDLERS
    // -------------------------------------------------------------------------
    private void OnAccountRefreshed(PlayerAccountRefreshedPayload payload)
        => Refresh();

    private void OnSkinEquipped(PlayerSkinEquippedPayload payload)
        => Refresh(); // Re-evaluate "Equipped" state across all widgets

    // -------------------------------------------------------------------------
    // HELPERS
    // -------------------------------------------------------------------------
    private string BuildPriceString()
    {
        if (m_SkinData.CoinCost > 0 && m_SkinData.GemCost > 0)
            return $"{m_SkinData.CoinCost} Coins  or  {m_SkinData.GemCost} Gems";
        if (m_SkinData.CoinCost > 0)
            return $"{m_SkinData.CoinCost} Coins";
        if (m_SkinData.GemCost > 0)
            return $"{m_SkinData.GemCost} Gems";
        return "Free";
    }

    private bool CanAfford()
    {
        if (m_SkinData.CoinCost > 0 && EconomyManager.Instance.CoinBalance >= m_SkinData.CoinCost) return true;
        if (m_SkinData.GemCost  > 0 && EconomyManager.Instance.GemBalance  >= m_SkinData.GemCost)  return true;
        return false;
    }
}