using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Scoreboard : MonoBehaviour
{
    public GameObject scoreBoard;
    [SerializeField] private Image scoreToken;
    [SerializeField] private Sprite[] scoreTokens = new Sprite[3];
    [SerializeField] private TextMeshProUGUI currentScoreText = null;
    [SerializeField] private TextMeshProUGUI highScoreText = null;

    private int scoreThreshold = 0;    // This will be set by the score manager and will determine which token the player receives


    // Listen for the player death, then do the score board presentation
    [ContextMenu("DisplayResults")]
    private void DisplayResults()
    {
        scoreBoard.SetActive(true);
        currentScoreText.text = "5";
        highScoreText.text = "10";
        scoreToken.sprite = scoreTokens[2];    // Update this with the result of the score threshold 
    }

}