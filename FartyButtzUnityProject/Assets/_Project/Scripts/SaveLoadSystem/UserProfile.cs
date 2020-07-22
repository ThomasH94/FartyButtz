using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A user profile will hold all the information about the user including;
/// Unlocked Buttz and Levels, Profile Icons(if applicable), High Scores, and more
/// </summary>
public class UserProfile : MonoBehaviour
{
    //private List<Buttz> unlockedButtz = new List<Buttz>();    // All of the collected Buttz
    //private List<Stage> unlockedStages = new List<Stage>();   // All of the collected stages

    public string userName;
    public string userID;    // Using a string for all available ASCII characters

    private float totalPlayTime;

    public void StartTickingPlayTime()
    {
        totalPlayTime += Time.time;
    }

    [ContextMenu("Restart Play Time")]
    private void RestartPlayTime()
    {
        totalPlayTime = 0f;
    }
}
