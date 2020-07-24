using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Scoreboard : MonoBehaviour
{
    public GameObject scoreBoard;
    [SerializeField] private Image medalImage;
    [SerializeField] private Sprite[] medalSprites = new Sprite[4]; 
    [SerializeField] private TextMeshProUGUI currentScoreText = null;
    [SerializeField] private TextMeshProUGUI highScoreText = null;

    #region Score
    
    private int highScore = 0;

    private int bronzeMedal = 1;
    private int silverMedal = 20;
    private int goldMedal = 50;
    private int platMedal = 100;
    
    #endregion

    [SerializeField] private LeanTweenType easeType;

    private void Start()
    {

    }
    
    private void OnEnable()
    {
        ButtController.OnPlayerDied += DisplayResults;
        ScoreController.ScoreUpdated += UpdateScoreBoard;
        
        medalImage.color = new Color(0, 0, 0, 0);    // Set it transparent unless we get a token
        // TODO: Get the highscore from some external source and set it
    }

    private void OnDisable()
    {
        ButtController.OnPlayerDied -= DisplayResults;
        ScoreController.ScoreUpdated -= UpdateScoreBoard;
    }


    // Listen for the player death, then do the score board presentation
    [ContextMenu("DisplayResults")]
    private void DisplayResults()
    {
        // TODO: Save our results, then display them
        // TODO: Create a coroutine/promise/delayed method that will do the display presentation
        //scoreBoard.SetActive(true);

        float easeTime = 1.5f;
        scoreBoard.LeanMoveLocalX(0, easeTime).setEase(easeType);
    }

    private void UpdateScoreBoard(int scoreAmount)
    {
        currentScoreText.text = scoreAmount.ToString();

        if (scoreAmount > highScore)
        {
            highScore = scoreAmount;
        }
        
        highScoreText.text = highScore.ToString();
        SetToken(scoreAmount);
    }
    
    private void SetToken(int index)
    {
        int token = CheckScoreForToken(index);
        if (token != 0)
        {
            medalImage.color = Color.white;    // The token is transparent to start
            medalImage.sprite = medalSprites[token];    // Update this with the result of the score threshold     
        }

    }

    private int CheckScoreForToken(int scoreAmount)
    {
        int token = 0;    // Default - Not eligible for a token

        // Update if our score is within these thresholds
        if (scoreAmount >= bronzeMedal && scoreAmount < silverMedal)
        {
            token = 1;
        }
        else if (scoreAmount >= silverMedal && scoreAmount < goldMedal)
        {
            token = 2;
        }
        else if (scoreAmount >= goldMedal && scoreAmount < platMedal)
        {
            token = 3;
        }
        else if(scoreAmount >= platMedal)
        {
            token = 4;
        }
        
        return token; 
    }
    
}