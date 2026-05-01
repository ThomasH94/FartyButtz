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
    private ExtendedButton m_PlayButton = null;
    [SerializeField]
    private ExtendedButton m_InventoryButton = null;
    
    [SerializeField]
    private ExtendedButton m_StoreButton = null;
    
    [SerializeField]
    private ExtendedButton m_LogoutButton = null;

    private void Start()
    {
        m_PlayButton.RegisterClickAction(OnPlayButtonPressed);
        m_StoreButton.RegisterClickAction(OnStoreButtonClicked);
        m_InventoryButton.RegisterClickAction(OnInventoryClicked);
        m_LogoutButton.RegisterClickAction(OnLogoutClicked);
    }

    private void OnInventoryClicked()
    {
        EventBus.Publish(new MenuRequestOpenPayload(typeof(AvatarSelectionMenu), null));
    }

    private void OnStoreButtonClicked()
    {
        EventBus.Publish(new MenuRequestOpenPayload(typeof(StoreMenu), null));
    }
    
    private void OnLogoutClicked()
    {
        EventBus.Publish(new LogoutRequestPayload());
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
