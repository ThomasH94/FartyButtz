using System;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using TMPro;
using UnityEngine;

[Serializable]
public abstract class AbstractGameMode : SerializedMonoBehaviour
{
    protected PlayerController m_PlayerController;
    protected PlayerInputHandler m_InputHandler;

    [OdinSerialize] protected Spawner m_Spawner;
    [OdinSerialize] protected TextMeshProUGUI m_ScoreText;
    [OdinSerialize] protected GameObject m_PlayButton;
    [OdinSerialize] protected GameObject m_GameOverScreen;
    [OdinSerialize] protected ButtData m_PlayerButtData;

    // Lifecycle — all modes must implement these
    public abstract void StartGame();
    public abstract void PauseGame();
    public abstract void EndGame();
    public abstract void OnScoreChanged(int newScore);

    // Shared setup all modes get for free
    protected virtual void InitializePlayer(PlayerController player)
    {
        m_PlayerController = player;
        m_PlayerController.Initialize(m_PlayerButtData);
        m_InputHandler = player.GetComponent<PlayerInputHandler>();
    }
}