using System;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using TMPro;
using UnityEngine;

[Serializable]
public abstract class AbstractGameModeController : SerializedMonoBehaviour
{
    protected PlayerController m_PlayerController;
    protected PlayerInputHandler m_InputHandler;

    [OdinSerialize] protected Transform m_PlayerSpawn = null;
    [OdinSerialize] protected Spawner m_Spawner;
    [OdinSerialize] protected TextMeshProUGUI m_ScoreText;
    [OdinSerialize] protected GameObject m_PlayButton;
    [OdinSerialize] protected GameObject m_GameOverScreen;

    // Lifecycle — all modes must implement these
    public virtual void StartGame()
    {
        m_InputHandler.enabled = true;
        m_GameOverScreen.SetActive(false);
    }
    public abstract void PauseGame();
    public abstract void EndGame();
    public abstract void OnScoreChanged(int newScore);

    // Shared setup all modes get for free
    protected virtual void InitializePlayer(PlayerController player)
    {
        m_PlayerController = Instantiate(player, m_PlayerSpawn);
        m_InputHandler = m_PlayerController.GetComponent<PlayerInputHandler>();
        m_PlayerController.Initialize(PlayerDataManager.Instance.GetEquippedSkin());
        m_InputHandler.Initialize(m_PlayerController);
    }
}