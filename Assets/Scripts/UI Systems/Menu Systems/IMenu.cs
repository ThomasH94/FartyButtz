public interface IMenu
{
	IMenuData MenuData { get; }

	bool CanOpen(IMenuData data);

	void OnOpen(IMenuData data);

	void OnClose();
}
