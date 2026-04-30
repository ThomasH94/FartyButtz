using System.Collections.Generic;
using Sirenix.Serialization;
using UnityEngine;

public class StoreMenuData : IMenuData
{
    public CurrencyType FocusTab { get; }
    public StoreMenuData(CurrencyType focusTab = CurrencyType.NONE) => FocusTab = focusTab;
}

/// <summary>
/// Store menu with two sections:
///   - Currency packs (buy Coins/Gems via IAP stub)
///   - Skins (buy ButtData items from PlayFab catalog)
///
/// Currency packs are configured in the Inspector.
/// Skin widgets are populated from ButtDB — ALL skins are shown so the
/// player can see what's available to buy, not just what they own.
/// StoreButtWidget handles owned/unowned state itself via PlayFab inventory.
/// </summary>
public class StoreMenu : BaseMenu
{
    [System.Serializable]
    public class CurrencyPackConfig
    {
        [Tooltip("Display label e.g. '100 Coins', 'Bag of Gems'")]
        public string Label;
        [Tooltip("Real-money display price (stub — no actual charge yet)")]
        public float DisplayPrice;
        [Tooltip("How much currency to grant via CloudScript")]
        public int GrantAmount;
    }

    [Header("Templates")]
    [OdinSerialize] private StoreCurrencyWidget m_StoreCurrencyWidgetTemplate = null;
    [OdinSerialize] private StoreButtWidget     m_ButtWidgetTemplate          = null;

    [Header("Holders")]
    [OdinSerialize] private Transform m_CoinWidgetsHolder = null;
    [OdinSerialize] private Transform m_GemWidgetsHolder  = null;
    [OdinSerialize] private Transform m_ButtWidgetsHolder = null;

    [Header("Currency Pack Config")]
    [SerializeField] private List<CurrencyPackConfig> m_CoinPacks = new()
    {
        new CurrencyPackConfig { Label = "100 Coins",  DisplayPrice = 0.99f,  GrantAmount = 100  },
        new CurrencyPackConfig { Label = "550 Coins",  DisplayPrice = 4.99f,  GrantAmount = 550  },
        new CurrencyPackConfig { Label = "1200 Coins", DisplayPrice = 9.99f,  GrantAmount = 1200 },
    };

    [SerializeField] private List<CurrencyPackConfig> m_GemPacks = new()
    {
        new CurrencyPackConfig { Label = "50 Gems",  DisplayPrice = 0.99f, GrantAmount = 50  },
        new CurrencyPackConfig { Label = "280 Gems", DisplayPrice = 4.99f, GrantAmount = 280 },
        new CurrencyPackConfig { Label = "600 Gems", DisplayPrice = 9.99f, GrantAmount = 600 },
    };

    private readonly List<StoreCurrencyWidget> m_SpawnedCurrencyWidgets = new();
    private readonly List<StoreButtWidget>     m_SpawnedButtWidgets     = new();

    public override void OnOpen(IMenuData data)
    {
        base.OnOpen(data);
        ClearAllWidgets();
        CreateCurrencyWidgets();
        CreateButtWidgets();
    }

    public override void OnClose()
    {
        base.OnClose();
        ClearAllWidgets();
    }

    // -------------------------------------------------------------------------
    // CURRENCY WIDGETS
    // -------------------------------------------------------------------------
    private void CreateCurrencyWidgets()
    {
        m_StoreCurrencyWidgetTemplate.gameObject.SetActive(false);

        foreach (var pack in m_CoinPacks)
            SpawnCurrencyWidget("CN", pack, m_CoinWidgetsHolder);

        foreach (var pack in m_GemPacks)
            SpawnCurrencyWidget("GM", pack, m_GemWidgetsHolder);
    }

    private void SpawnCurrencyWidget(string currencyCode, CurrencyPackConfig pack, Transform holder)
    {
        var widget = Instantiate(m_StoreCurrencyWidgetTemplate, holder);
        widget.gameObject.SetActive(true);
        widget.Setup(new StoreCurrencyWidgetData(currencyCode, pack.DisplayPrice, pack.GrantAmount, pack.Label));
        m_SpawnedCurrencyWidgets.Add(widget);
    }

    // -------------------------------------------------------------------------
    // BUTT / SKIN WIDGETS
    // Shows every skin in ButtDB — StoreButtWidget handles owned vs buyable state.
    // -------------------------------------------------------------------------
    private void CreateButtWidgets()
    {
        if (m_ButtWidgetTemplate == null || m_ButtWidgetsHolder == null) return;

        m_ButtWidgetTemplate.gameObject.SetActive(false);

        var db = ButtDB.Instance;
        if (db == null)
        {
            Debug.LogError("[StoreMenu] ButtDB not found. Place it at Resources/DB/ButtDB.asset");
            return;
        }

        foreach (var skin in db.Entries)
        {
            if (skin == null) continue;
            var widget = Instantiate(m_ButtWidgetTemplate, m_ButtWidgetsHolder);
            widget.gameObject.SetActive(true);
            widget.Setup(skin);
            m_SpawnedButtWidgets.Add(widget);
        }

        Debug.Log($"[StoreMenu] Spawned {m_SpawnedButtWidgets.Count} skin widgets.");
    }

    // -------------------------------------------------------------------------
    // CLEANUP
    // -------------------------------------------------------------------------
    private void ClearAllWidgets()
    {
        foreach (var w in m_SpawnedCurrencyWidgets)
            if (w != null) Destroy(w.gameObject);
        m_SpawnedCurrencyWidgets.Clear();

        foreach (var w in m_SpawnedButtWidgets)
            if (w != null) Destroy(w.gameObject);
        m_SpawnedButtWidgets.Clear();
    }
}