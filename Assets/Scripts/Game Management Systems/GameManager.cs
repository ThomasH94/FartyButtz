using Sirenix.Serialization;
using UnityEngine;

[DefaultExecutionOrder(-1)]
public class GameManager : SingletonMonoBehaviour<GameManager>
{
    [OdinSerialize] private SceneData m_TESTSCENE;
    
    protected override void Awake()
    {
        base.Awake();
        SceneController.LoadSingle(m_TESTSCENE);
    }
}