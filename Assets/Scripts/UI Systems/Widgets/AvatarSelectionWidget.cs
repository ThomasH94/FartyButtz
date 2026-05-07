using System;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using TMPro;
using UnityEngine.UI;

public class AvatarSelectionWidget : SerializedMonoBehaviour 
{
    [OdinSerialize]
    private Image m_WidgetIcon;
    [OdinSerialize]
    private TextMeshProUGUI m_WidgetLabel;
    [OdinSerialize]
    private TextMeshProUGUI m_EquipLabel;

    [OdinSerialize] private ExtendedButton m_EquipButton = null;

    private ButtData m_ButtData = null;
    private bool m_IsEquipped = false;

    private void OnEnable()
    {
        EventBus.Subscribe<SkinApplyRequestPayload>(OnSkinEquipped);
    }
    
    private void OnDisable()
    {
        EventBus.Unsubscribe<SkinApplyRequestPayload>(OnSkinEquipped);
    }

    public void Initialize(ButtData buttData)
    {
        m_ButtData = buttData;
        m_WidgetIcon.sprite = m_ButtData.ButtSprite;
        m_WidgetLabel.text = m_ButtData.name;
        m_EquipButton.RegisterClickAction(EquipSkin);
        SetEquippedState();
    }

    private void EquipSkin()
    {
        EventBus.Publish(new SkinApplyRequestPayload(m_ButtData));
    }
    
    private void OnSkinEquipped(SkinApplyRequestPayload payload)
    {
        SetEquippedState();
    }

    public void SetEquippedState()
    {
        m_IsEquipped = m_ButtData == PlayerDataManager.Instance.GetEquippedSkin();
        m_EquipButton.interactable = !m_IsEquipped;
        m_EquipLabel.text = m_IsEquipped ? "Equipped" : "Equip";
    }
}