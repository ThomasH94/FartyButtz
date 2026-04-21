using System;
using UnityEngine;

[Serializable]
public class ZenModeController : AbstractGameMode
{
    #region DEBUG

    public bool INVINCIBLE  = false;

    #endregion
    
    private int m_CurrentScore = 0;
    private int m_HighScore = 0;

    public int CurrentScore => m_CurrentScore;
    public PlayerController PlayerController;

    private void OnEnable()
    {
        EventBus.Subscribe<OnPlayerDeathPayload>(OnPlayerDeath);
        EventBus.Subscribe<IncreaseScorePayload>(OnPlayerScored);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<OnPlayerDeathPayload>(OnPlayerDeath);
        EventBus.Unsubscribe<IncreaseScorePayload>(OnPlayerScored);
    }
    
    private void OnPlayerDeath(OnPlayerDeathPayload payload)
    {
        if (!INVINCIBLE) 
            GameOver();
    }
    
    private void OnPlayerScored(IncreaseScorePayload payload)
    {
        IncreaseScore(payload.ScoreAmount);
    }

    private void Start()
    {
        SetupPlayer(PlayerController);
        Pause();
    }

    public void Pause()
    {
        Time.timeScale = 0f;
        mPlayerController.enabled = false;
    }

    public void Play()
    {
        m_CurrentScore = 0;
        m_ScoreText.text = m_CurrentScore.ToString();

        playButton.SetActive(false);
        gameOver.SetActive(false);

        Time.timeScale = 1f;
        mPlayerController.enabled = true;

        Pipes[] pipes = FindObjectsOfType<Pipes>();

        for (int i = 0; i < pipes.Length; i++) {
            Destroy(pipes[i].gameObject);
        }
    }

    public void GameOver()
    {
        playButton.SetActive(true);
        gameOver.SetActive(true);

        Pause();
    }

    public void IncreaseScore(int scoreAmount)
    {
        m_CurrentScore += scoreAmount;
        m_ScoreText.text = $"{m_CurrentScore}";
    }
}
