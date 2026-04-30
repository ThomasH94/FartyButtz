using Sirenix.OdinInspector;
using Sirenix.Serialization;
using TMPro;
using UnityEngine;

public enum CurrencyType
{
    NONE = 0,
    COINS,
    GEMS,
}

/// <summary>
/// Displays the player's current balance for a given CurrencyType.
/// Subscribes to PlayerAccountRefreshedPayload so it stays in sync whenever
/// the economy refreshes (login, purchase, ad reward, IAP).
///
/// The plus button stubs out navigation to the relevant store section.
/// Wire up m_StoreButton in the Inspector once the store menu exists.
/// </summary>
public class CurrencyWidget : SerializedMonoBehaviour
{
    [SerializeField] private CurrencyType m_CurrencyType = CurrencyType.NONE;

    [Header("UI")]
    [SerializeField] private TMP_Text m_BalanceText = null;
    [SerializeField] private ExtendedButton m_StoreButton = null;

    private void OnEnable()
    {
        EventBus.Subscribe<PlayerAccountRefreshedPayload>(OnInventoryLoaded);
        m_StoreButton?.RegisterClickAction(OnStoreButtonClicked);

        // If economy is already loaded (e.g. widget enabled after login),
        // populate immediately rather than waiting for the next refresh.
        if (EconomyManager.Instance != null && EconomyManager.Instance.IsInventoryLoaded)
            UpdateDisplay(GetBalance());
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<PlayerAccountRefreshedPayload>(OnInventoryLoaded);
    }

    // -------------------------------------------------------------------------
    // EVENT HANDLERS
    // -------------------------------------------------------------------------
    private void OnInventoryLoaded(PlayerAccountRefreshedPayload payload)
    {
        int balance = m_CurrencyType switch
        {
            CurrencyType.COINS => payload.CoinBalance,
            CurrencyType.GEMS  => payload.GemBalance,
            _                  => 0
        };

        UpdateDisplay(balance);
    }

    // -------------------------------------------------------------------------
    // STORE NAVIGATION (stubbed)
    // -------------------------------------------------------------------------
    private void OnStoreButtonClicked()
    {
        // TODO: replace with real store menu type and pass the section as data
        // e.g. EventBus.Publish(new MenuRequestOpenPayload(typeof(StoreMenu), new StoreMenuData(m_CurrencyType)));
        Debug.Log($"[CurrencyWidget] Open store — section: {m_CurrencyType}");
    }

    // -------------------------------------------------------------------------
    // HELPERS
    // -------------------------------------------------------------------------
    private void UpdateDisplay(int balance)
    {
        if (m_BalanceText == null) return;
        m_BalanceText.text = FormatBalance(balance);
    }

    private int GetBalance() => m_CurrencyType switch
    {
        CurrencyType.COINS => EconomyManager.Instance.CoinBalance,
        CurrencyType.GEMS  => EconomyManager.Instance.GemBalance,
        _                  => 0
    };

    /// <summary>Formats large numbers readably: 1200 -> "1.2K", 1500000 -> "1.5M"</summary>
    private static string FormatBalance(int amount)
    {
        if (amount >= 1_000_000) return $"{amount / 1_000_000f:0.#}M";
        if (amount >= 10_000)    return $"{amount / 1_000f:0.#}K";
        return amount.ToString("N0");
    }
}