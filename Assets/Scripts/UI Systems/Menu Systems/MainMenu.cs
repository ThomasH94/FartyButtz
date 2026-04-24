using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuData : IMenuData
{
    // Info about the previous game state or what tried to open the Main Menu etc
}

public class MainMenu : BaseMenu
{
    [OdinSerialize] 
    private SceneData m_TESTSCENE;
    
    [SerializeField]
    private Button m_PlayButton = null;
    
    [SerializeField]
    private Button m_StoreButton = null;

    private void Start()
    {
        m_PlayButton.onClick.AddListener
        (() =>
            {
                OnPlayButtonPressed();
            }
        );
    }
    
    public override void OnOpen(IMenuData data)
    {
        base.OnOpen(data);
        MainMenuData mainMenuData = data as MainMenuData;
    }

    private void OnPlayButtonPressed()
    {
        EventBus.Publish(new MenuRequestClosePayload(this));
        SceneController.LoadSingle(m_TESTSCENE);
    }
}
