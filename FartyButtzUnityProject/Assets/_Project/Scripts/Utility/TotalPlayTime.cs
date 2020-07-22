using System.Collections;
using UnityEngine;

/// <summary>
/// This class will track the players play time
/// To be used in conjunction with the UserProfile
/// </summary>
public class TotalPlayTime: MonoBehaviour
{
    public float totalPlayedTime; 
    public float seconds;
    public float minutes;
    public float hours;
    public float days;

    private void Start()
    {
        StartCoroutine((StartPlayTimerRoutine()));
    }

    [ContextMenu("Display the Timers current time")]
    private void DisplayTimer()
    {
        Debug.Log(totalPlayedTime);
    }

    public IEnumerator StartPlayTimerRoutine()
    {
        WaitForSeconds timeToWait = new WaitForSeconds(1);
        while (true)
        {
            yield return timeToWait;
            totalPlayedTime += 1.0f;
        }
    }
    
    public void ResetPlayTime()
    {
        // Reset the total playtime...
    }
    
}