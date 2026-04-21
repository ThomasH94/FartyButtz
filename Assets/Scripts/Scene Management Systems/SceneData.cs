using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu(menuName = "Scene/SceneData")]
public class SceneData : SerializedScriptableObject
{
	public async UniTask EnterAsync()
	{

	}

	public async UniTask ExitAsync()
	{

	}

	public void Enter()
	{
		EnterAsync().Forget();
	}

	public void Exit()
	{
		ExitAsync().Forget();
	}
}
