using Sirenix.OdinInspector;
using UnityEngine;

public abstract class SingletonMonoBehaviour<T> : SerializedMonoBehaviour where T : SerializedMonoBehaviour
{
	[SerializeField] 
	private bool m_DontDestroyOnLoad = true;
	
	public static T Instance { get; private set; }

	protected virtual void Awake()
	{
		if (Instance == null)
		{
			Instance = this as T;
			// NOTE: Hierarchy Folders are blank Gameobjects, so this is a hacky work around
			transform.SetParent(null);
			if (m_DontDestroyOnLoad) 
				DontDestroyOnLoad(gameObject);
		}
		else if (Instance != this)
		{
			Destroy(gameObject);
		}
	}
}
