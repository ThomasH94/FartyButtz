using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MenuManager : SingletonMonoBehaviour<MenuManager>
{
	[Header("UI Root (Canvas → Menus)")]
	[SerializeField]
	private Transform _uiRoot;

	[Header("Available Menu Prefabs")]
	[SerializeField]
	private List<MenuPrefabEntry> _menuPrefabs;

	private Dictionary<Type, BaseMenu> _prefabLookup;

	private readonly Stack<BaseMenu> _stack = new Stack<BaseMenu>();

	private BaseMenu m_PreviousMenu;

	private IMenuData m_PreviousMenuData;

	protected override void Awake()
	{
		base.Awake();
		_prefabLookup = _menuPrefabs.Where((MenuPrefabEntry e) => e.MenuPrefab != null).ToDictionary((MenuPrefabEntry e) => e.MenuPrefab.GetType(), (MenuPrefabEntry e) => e.MenuPrefab);
		EventBus.Publish(new MenuManagerReadyPayload());
	}

	private void OnEnable()
	{
		EventBus.Subscribe<MenuRequestOpenPayload>(OnRequestOpen);
		EventBus.Subscribe<MenuRequestClosePayload>(OnRequestClose);
		EventBus.Subscribe<SceneUnloadedPayload>(OnSceneUnloaded);
	}

	private void OnDisable()
	{
		EventBus.Unsubscribe<MenuRequestOpenPayload>(OnRequestOpen);
		EventBus.Unsubscribe<MenuRequestClosePayload>(OnRequestClose);
		EventBus.Unsubscribe<SceneUnloadedPayload>(OnSceneUnloaded);
	}

	private void OnRequestOpen(MenuRequestOpenPayload req)
	{
		Debug.Log($"[MenuManager] Open request for {req.MenuType.Name} (CloseIfOpen={req.CloseIfOpen})");
		if (req.CloseIfOpen)
		{
			BaseMenu baseMenu = _stack.Reverse().FirstOrDefault((BaseMenu m) => m.GetType() == req.MenuType);
			if (baseMenu != null)
			{
				m_PreviousMenu = baseMenu;
				m_PreviousMenuData = m_PreviousMenu.MenuData;
				Stack<BaseMenu> stack = new Stack<BaseMenu>();
				while (_stack.Count > 0)
				{
					BaseMenu baseMenu2 = _stack.Pop();
					if (baseMenu2 == baseMenu)
					{
						baseMenu2.OnClose();
						UnityEngine.Object.Destroy(baseMenu2.gameObject);
						break;
					}
					stack.Push(baseMenu2);
				}
				while (stack.Count > 0)
				{
					_stack.Push(stack.Pop());
				}
				Debug.Log("[MenuManager] Toggled off " + req.MenuType.Name + " via CloseIfOpen");
				if (_stack.Count > 0)
				{
					_stack.Peek().OnOpen(_stack.Peek().MenuData);
				}
				return;
			}
		}
		if (req.CloseIfOpen && _stack.Count > 0)
		{
			BaseMenu baseMenu3 = _stack.Pop();
			baseMenu3.OnClose();
			UnityEngine.Object.Destroy(baseMenu3.gameObject);
		}
		if (!_prefabLookup.TryGetValue(req.MenuType, out var value))
		{
			Debug.LogError("[MenuManager] No prefab for " + req.MenuType.Name);
			return;
		}
		BaseMenu baseMenu4 = UnityEngine.Object.Instantiate(value, _uiRoot);
		baseMenu4.name = req.MenuType.Name;
		baseMenu4.OnOpen(req.Data);
		_stack.Push(baseMenu4);
		Debug.Log($"[MenuManager] Opened {req.MenuType.Name}, stack depth {_stack.Count}");
	}

	private void OnRequestClose(MenuRequestClosePayload payload)
	{
		Debug.Log("[MenuManager] Close request");
		if (_stack.Count == 0)
		{
			return;
		}
		if (payload.Menu != null)
		{
			if (_stack.Contains(payload.Menu))
			{
				Stack<BaseMenu> stack = new Stack<BaseMenu>();
				while (_stack.Count > 0 && _stack.Peek() != payload.Menu)
				{
					stack.Push(_stack.Pop());
				}
				BaseMenu baseMenu = _stack.Pop();
				baseMenu.OnClose();
				UnityEngine.Object.Destroy(baseMenu.gameObject);
				Debug.Log("[MenuManager] Closed specific " + baseMenu.GetType().Name);
				while (stack.Count > 0)
				{
					_stack.Push(stack.Pop());
				}
				if (_stack.Count > 0 && payload.OpenTopMenu)
				{
					_stack.Peek().OnOpen(_stack.Peek().MenuData);
				}
			}
		}
		else if (payload.CloseTopMenu)
		{
			BaseMenu baseMenu2 = _stack.Pop();
			baseMenu2.OnClose();
			UnityEngine.Object.Destroy(baseMenu2.gameObject);
			Debug.Log($"[MenuManager] Closed {baseMenu2.GetType().Name}, stack depth {_stack.Count}");
			if (_stack.Count > 0)
			{
				_stack.Peek().OnOpen(_stack.Peek().MenuData);
			}
		}
	}

	private void OnSceneUnloaded(SceneUnloadedPayload _)
	{
		_stack.Clear();
	}

	public bool IsMenuOpen(BaseMenu menu)
	{
		return _stack.Contains(menu);
	}

	public void OpenPreviousMenu()
	{
		EventBus.Publish(new MenuRequestOpenPayload(m_PreviousMenu.GetType(), m_PreviousMenuData));
	}
}
