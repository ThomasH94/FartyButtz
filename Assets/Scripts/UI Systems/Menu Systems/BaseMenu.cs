using Cysharp.Threading.Tasks;
using UnityEngine;

public abstract class BaseMenu : MonoBehaviour, IMenu
{
	[SerializeField]
	protected GameObject m_ContentRect;

	[SerializeField]
	protected ExtendedButton m_CloseButton;

	public bool IsOpen { get; protected set; }

	public IMenuData MenuData { get; private set; }

	protected virtual void OnEnable()
	{
		m_CloseButton?.RegisterClickAction(CloseMenuInternal);
	}

	protected virtual void OnDisable()
	{
		m_CloseButton?.UnregisterClickAction(CloseMenuInternal);
	}

	public virtual void OnOpen(IMenuData data)
	{
		MenuData = data;
		m_ContentRect.SetActive(value: true);
		IsOpen = true;
	}

	public virtual async UniTask OpenMenuAsync(IMenuData data)
	{
	}

	public virtual void OnClose()
	{
		m_ContentRect.SetActive(value: false);
		IsOpen = false;
	}

	protected void CloseMenuInternal()
	{
		CloseMenu();
	}

	public virtual void CloseMenu(bool closeTop = false, bool openTop = true)
	{
		EventBus.Publish(new MenuRequestClosePayload(this, closeTop, openTop));
	}

	public virtual async UniTask CloseMenuAsync()
	{
	}

	public virtual bool CanOpen(IMenuData data)
	{
		return true;
	}
}
