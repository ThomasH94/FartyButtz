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

    public void Setup(ButtData buttData)
    {
        m_WidgetIcon.sprite = buttData.ButtSprite;
        m_WidgetLabel.text = buttData.name;
    }
}