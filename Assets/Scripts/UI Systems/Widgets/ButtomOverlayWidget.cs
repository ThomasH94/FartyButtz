using Sirenix.Serialization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum BottomOverlayWidgetState
{
    NONE = 0,
    IDLE = 1,
    SELECTED = 2,
    DISABLED = 3
}

public class ButtomOverlayWidget
{
    [OdinSerialize] private TextMeshProUGUI m_WidgetTMP = null;
    [OdinSerialize] private Image m_WidgetBackground = null;
    [OdinSerialize] private Sprite m_WidgetIdleBackgroundSprite = null;
    [OdinSerialize] private Sprite m_WidgetSelectedBackgroundSprite = null;
    
    private BottomOverlayWidgetState m_State = BottomOverlayWidgetState.NONE;

    public void Setup()
    {
        
    }
    
}
