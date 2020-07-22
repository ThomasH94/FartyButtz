using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreUpdater : MonoBehaviour
{
    public int scoreAmount;
    private void OnTriggerExit2D(Collider2D collidedObject)
    {
        if (collidedObject.CompareTag("Player"))
        {
            Debug.Log("Added score!");
            ScoreController.Instance.UpdateScoreText(scoreAmount);
        }
    }
}
