public class MenuRequestClosePayload
{
	public BaseMenu Menu;

	public bool CloseTopMenu;

	public bool OpenTopMenu;

	public MenuRequestClosePayload(BaseMenu menu, bool closeTopMenu = true, bool openTopMenu = true)
	{
		Menu = menu;
		CloseTopMenu = closeTopMenu;
		OpenTopMenu = openTopMenu;
	}
}
