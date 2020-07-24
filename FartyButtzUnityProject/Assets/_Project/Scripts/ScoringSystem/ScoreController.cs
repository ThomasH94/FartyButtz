using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using TMPro;

public class ScoreController : MonoBehaviour
{
    public static ScoreController Instance;
    public TextMeshProUGUI scoreText;
    public int currentScore;

    public static Action<int> ScoreUpdated;

    private void Awake()
    {
        // Singleton check
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        UpdateScoreText(0);
    }

    public void UpdateScoreText(int scoreAdjuster)
    {
        currentScore += scoreAdjuster;
        scoreText.text = currentScore.ToString();
        ScoreUpdated?.Invoke(currentScore);
    }
    
}