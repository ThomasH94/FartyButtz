using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneController
{
	private static string _lastScene;

	private static SceneData s_PreviousSceneData;

	private static SceneData s_CurrentSceneData;

	private static readonly Stack<string> _loadedAdditiveScenes;

	private static readonly Stack<string> _singleSceneHistory;

	public static string LastScene => _lastScene;

	static SceneController()
	{
		s_PreviousSceneData = null;
		s_CurrentSceneData = null;
		_loadedAdditiveScenes = new Stack<string>();
		_singleSceneHistory = new Stack<string>();
		SceneManager.sceneUnloaded += delegate(Scene scene)
		{
			EventBus.Publish(new SceneUnloadedPayload(scene.name));
		};
		SceneManager.sceneLoaded += delegate(Scene scene, LoadSceneMode mode)
		{
			_lastScene = scene.name;
		};
	}

	public static void SetCurrentSceneData(SceneData sceneData)
	{
		s_CurrentSceneData = sceneData;
	}

	public static SceneData GetCurrentSceneData()
	{
		return s_CurrentSceneData;
	}

	private static SceneData ResolveSceneDataBySceneName(string sceneName)
	{
		if (s_CurrentSceneData != null)
		{
			if (!string.IsNullOrEmpty(s_CurrentSceneData.name) && s_CurrentSceneData.name == sceneName)
			{
				return s_CurrentSceneData;
			}
			if (s_CurrentSceneData.name == sceneName)
			{
				return s_CurrentSceneData;
			}
		}
		if (s_PreviousSceneData != null)
		{
			if (!string.IsNullOrEmpty(s_PreviousSceneData.name) && s_PreviousSceneData.name == sceneName)
			{
				return s_PreviousSceneData;
			}
			if (s_PreviousSceneData.name == sceneName)
			{
				return s_PreviousSceneData;
			}
		}
		SceneData[] source = Resources.LoadAll<SceneData>("");
		SceneData sceneData = source.FirstOrDefault((SceneData sd) => sd != null && !string.IsNullOrEmpty(sd.name) && sd.name == sceneName) ?? source.FirstOrDefault((SceneData sd) => sd != null && sd.name == sceneName);
		if (sceneData == null)
		{
			Debug.Log("[SceneController] No SceneData found for scene '" + sceneName + "'. EnterAsync will be skipped unless provided.");
		}
		else
		{
			Debug.Log("[SceneController] Resolved SceneData '" + sceneData.name + "' for scene '" + sceneName + "' via Resources scan.");
		}
		return sceneData;
	}

	private static SceneData ResolveSceneDataByAssetName(string assetName)
	{
		if (string.IsNullOrEmpty(assetName))
		{
			return null;
		}
		if (s_CurrentSceneData != null && s_CurrentSceneData.name == assetName)
		{
			return s_CurrentSceneData;
		}
		if (s_PreviousSceneData != null && s_PreviousSceneData.name == assetName)
		{
			return s_PreviousSceneData;
		}
		SceneData sceneData = Resources.LoadAll<SceneData>("").FirstOrDefault((SceneData sd) => sd != null && sd.name == assetName);
		if (sceneData == null)
		{
			Debug.Log("[SceneController] No SceneData asset found named '" + assetName + "'.");
		}
		else
		{
			Debug.Log("[SceneController] Resolved SceneData by asset name: '" + sceneData.name + "'.");
		}
		return sceneData;
	}

	public static async UniTask EnsureEnterForActiveSceneAsync(string preferredSdAssetName = null)
	{
		string active = SceneManager.GetActiveScene().name;
		SceneData sceneData = null;
		if (!string.IsNullOrEmpty(preferredSdAssetName))
		{
			sceneData = ResolveSceneDataByAssetName(preferredSdAssetName);
		}
		if ((object)sceneData == null)
		{
			sceneData = ResolveSceneDataBySceneName(active);
		}
		if (sceneData != null)
		{
			s_CurrentSceneData = sceneData;
			s_PreviousSceneData = sceneData;
			await sceneData.EnterAsync();
		}
		EventBus.Publish(new SceneLoadedPayload(active));
	}

	public static async UniTask EnsureEnterForActiveSceneAudioOnlyAsync(string preferredSdAssetName = null)
	{
		string name = SceneManager.GetActiveScene().name;
		SceneData sceneData = null;
		if (!string.IsNullOrEmpty(preferredSdAssetName))
		{
			sceneData = ResolveSceneDataByAssetName(preferredSdAssetName);
		}
		if ((object)sceneData == null)
		{
			sceneData = ResolveSceneDataBySceneName(name);
		}
		if (sceneData != null)
		{
			s_CurrentSceneData = sceneData;
			s_PreviousSceneData = sceneData;
			await sceneData.EnterAsync();
		}
	}

	public static void LoadSingle(string sceneName, SceneData sceneData = null, Action onLoaded = null)
	{
		string name = SceneManager.GetActiveScene().name;
		if (!string.IsNullOrEmpty(name) && name != sceneName)
		{
			_singleSceneHistory.Push(name);
		}
		SceneManager.sceneLoaded += OnSingleLoaded;
		SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
		void OnSingleLoaded(Scene s, LoadSceneMode m)
		{
			if (!(s.name != sceneName))
			{
				SceneManager.sceneLoaded -= OnSingleLoaded;
				s_CurrentSceneData = sceneData;
				SceneData sceneData2 = sceneData ?? ResolveSceneDataBySceneName(sceneName);
				if (sceneData2 != null)
				{
					s_CurrentSceneData = sceneData2;
					s_PreviousSceneData = sceneData2;
					sceneData2.EnterAsync().Forget();
				}
				onLoaded?.Invoke();
				EventBus.Publish(new SceneLoadedPayload(sceneName));
			}
		}
	}

	public static void LoadSingle(SceneData sceneData, Action onLoaded = null)
	{
		if (sceneData == null)
		{
			throw new ArgumentNullException("sceneData");
		}
		LoadSingle(sceneData.name, sceneData, onLoaded);
	}

	public static async UniTask LoadSingleAsync(string sceneName, SceneData sceneData = null, bool fade = true, float fadeDuration = 0.3f)
	{
		if (fade && SingletonMonoBehaviour<ScreenFader>.Instance != null)
		{
			await SingletonMonoBehaviour<ScreenFader>.Instance.FadeOut(fadeDuration);
		}
		string name = SceneManager.GetActiveScene().name;
		if (!string.IsNullOrEmpty(name) && name != sceneName)
		{
			_singleSceneHistory.Push(name);
		}
		await SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
		s_CurrentSceneData = sceneData;
		SceneData sceneData2 = sceneData ?? ResolveSceneDataBySceneName(sceneName);
		if (sceneData2 != null)
		{
			s_CurrentSceneData = sceneData2;
			s_PreviousSceneData = sceneData2;
			await sceneData2.EnterAsync();
		}
		EventBus.Publish(new SceneLoadedPayload(sceneName));
		if (fade && SingletonMonoBehaviour<ScreenFader>.Instance != null)
		{
			await SingletonMonoBehaviour<ScreenFader>.Instance.FadeIn(fadeDuration);
		}
	}

	public static UniTask LoadSingleAsync(SceneData sceneData, bool fade = true, float fadeDuration = 1f)
	{
		if (sceneData == null)
		{
			throw new ArgumentNullException("sceneData");
		}
		return LoadSingleAsync(sceneData.name, sceneData, fade, fadeDuration);
	}

	public static async UniTask LoadAdditive(string sceneName, SceneData sceneData = null, bool setActive = false, Action onLoaded = null, bool fade = false, float fadeDuration = 0.2f)
	{
		await SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
		_loadedAdditiveScenes.Push(sceneName);
		if (setActive)
		{
			SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));
		}
		onLoaded?.Invoke();
		SceneData sceneData2 = sceneData ?? ResolveSceneDataBySceneName(sceneName);
		if (sceneData2 != null)
		{
			await sceneData2.EnterAsync();
		}
		EventBus.Publish(new SceneLoadedPayload(sceneName));
	}

	public static UniTask LoadAdditive(SceneData sceneData, bool setActive = false, Action onLoaded = null, bool fade = false, float fadeDuration = 0.2f)
	{
		if (sceneData == null)
		{
			throw new ArgumentNullException("sceneData");
		}
		return LoadAdditive(sceneData.name, sceneData, setActive, onLoaded, fade, fadeDuration);
	}

	public static void UnloadAdditive(string sceneName, Action onUnloaded = null)
	{
		if (!_loadedAdditiveScenes.Contains(sceneName))
		{
			if (SceneManager.GetActiveScene().name == sceneName && _loadedAdditiveScenes.Count > 0)
			{
				Scene sceneByName = SceneManager.GetSceneByName(_loadedAdditiveScenes.Peek());
				if (sceneByName.IsValid())
				{
					SceneManager.SetActiveScene(sceneByName);
				}
			}
			onUnloaded?.Invoke();
			return;
		}
		SceneManager.UnloadSceneAsync(sceneName).completed += delegate
		{
			Stack<string> stack = new Stack<string>();
			bool flag = false;
			while (_loadedAdditiveScenes.Count > 0)
			{
				string text = _loadedAdditiveScenes.Pop();
				if (!flag && text == sceneName)
				{
					flag = true;
				}
				else
				{
					stack.Push(text);
				}
			}
			while (stack.Count > 0)
			{
				_loadedAdditiveScenes.Push(stack.Pop());
			}
			onUnloaded?.Invoke();
		};
	}

	public static async UniTask ReturnToPreviousAsync(bool fade = true, float fadeDuration = 0.3f)
	{
		if (_singleSceneHistory.Count == 0)
		{
			Debug.LogWarning("[SceneController] No previous scene to return to.");
		}
		else
		{
			await LoadSingleAsync(_singleSceneHistory.Pop(), s_PreviousSceneData, fade, fadeDuration);
		}
	}

	public static Scene GetActiveScene()
	{
		return SceneManager.GetActiveScene();
	}
}
