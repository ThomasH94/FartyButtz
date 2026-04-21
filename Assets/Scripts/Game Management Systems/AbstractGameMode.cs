using System;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using TMPro;
using UnityEngine;

[Serializable]
public abstract class AbstractGameMode : SerializedMonoBehaviour
{ 
    protected PlayerController mPlayerController;
    
    [OdinSerialize] 
    protected Spawner m_Spawner;
    [OdinSerialize] 
    protected TextMeshProUGUI m_ScoreText;
    [OdinSerialize] 
    protected GameObject playButton;
    [OdinSerialize] 
    protected GameObject gameOver;
    
    public virtual void SetupPlayer(PlayerController playerController)
    {
        mPlayerController = playerController;
    }
}