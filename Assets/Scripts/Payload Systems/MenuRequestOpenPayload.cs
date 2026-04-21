using System;

public struct MenuRequestOpenPayload
{
	public readonly Type MenuType;

	public readonly IMenuData Data;

	public readonly bool CloseIfOpen;

	public MenuRequestOpenPayload(Type menuType, IMenuData data, bool closeIfOpen = false)
	{
		MenuType = menuType;
		Data = data;
		CloseIfOpen = closeIfOpen;
	}
}
