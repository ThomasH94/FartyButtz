using System.Collections.Generic;
using PlayFab.ClientModels;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.UI;

public class AvatarSelectionMenu : BaseMenu
{
    [OdinSerialize] private Image m_AvatarIcon = null;
    [OdinSerialize] private AvatarSelectionWidget m_TemplateWidget  = null;
    [OdinSerialize] private Transform             m_WidgetContainer = null;

    private readonly List<AvatarSelectionWidget> m_SpawnedWidgets = new();
    private ButtData m_ButtData = null;

    protected override void OnEnable()
    {
        base.OnEnable();
        EventBus.Subscribe<SkinApplyRequestPayload>(OnSkinUpdateRequest);
        EventBus.Subscribe<PlayerAccountRefreshedPayload>(OnAccountRefreshed);
    }

    public override void OnOpen(IMenuData data)
    {
        base.OnOpen(data);
        m_ButtData = PlayerDataManager.Instance.GetEquippedSkin();
        SetupWidgets();
        UpdateAvatar(m_ButtData);
    }
    
    private void OnSkinUpdateRequest(SkinApplyRequestPayload payload)
    {
        UpdateAvatar(payload.SkinData);
    }

    private void UpdateAvatar(ButtData buttData)
    {
        m_ButtData = buttData;
        m_AvatarIcon.sprite = m_ButtData.ButtSprite;
    }

    // -------------------------------------------------------------------------
    // SETUP
    // Asks PlayFab what the player owns, then uses ButtDB to hydrate each
    // item ID into its local ButtData (sprite, sound, etc).
    // -------------------------------------------------------------------------
    private void SetupWidgets()
    {
        ClearWidgets();
        m_TemplateWidget.gameObject.SetActive(false);

        var db = ButtDB.Instance;
        if (db == null)
        {
            Debug.LogError("[AvatarSelectionMenu] ButtDB not found. Place it at Resources/DB/ButtDB.asset");
            return;
        }

        // Get what PlayFab says the player owns, then look up the ButtData for each
        var ownedSkins = db.GetOwnedSkins(EconomyManager.Instance.OwnedItems
            .ConvertAll(item => item.ItemId));

        if (ownedSkins.Count == 0)
        {
            Debug.LogWarning("[AvatarSelectionMenu] Player owns no skins — inventory may not be loaded yet.");
            return;
        }

        foreach (var skin in ownedSkins)
        {
            var widget = Instantiate(m_TemplateWidget, m_WidgetContainer);
            widget.gameObject.SetActive(true);
            widget.Initialize(skin);
            m_SpawnedWidgets.Add(widget);
            widget.SetEquippedState();
        }

        Debug.Log($"[AvatarSelectionMenu] Populated {m_SpawnedWidgets.Count} owned skins.");
    }

    // Re-populate if a purchase completes while this menu is open
    private void OnAccountRefreshed(PlayerAccountRefreshedPayload payload) => SetupWidgets();
    
        
    public override void OnClose()
    {
        base.OnClose();
        EventBus.Unsubscribe<PlayerAccountRefreshedPayload>(OnAccountRefreshed);
        ClearWidgets();
    }

    private void ClearWidgets()
    {
        foreach (var w in m_SpawnedWidgets)
            if (w != null) Destroy(w.gameObject);
        m_SpawnedWidgets.Clear();
    }
}