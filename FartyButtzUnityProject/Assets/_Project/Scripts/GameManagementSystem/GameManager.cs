using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The Game Manager will manage the state of the game and send out messages based on the games current state
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public List<GameObject> movers;

    public GameObject player;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    private void OnEnable()
    {
        ButtController.OnPlayerDied += StopGame;
    }

    private void OnDisable()
    {
        ButtController.OnPlayerDied -= StopGame;
    }

    [ContextMenu("Start Game")]
    private void StartGame()
    {
        player.GetComponent<Rigidbody2D>().simulated = true;
        foreach (GameObject mover in movers)
        {
            mover.GetComponent<Mover>().StartMoving();
        }
    }

    // Stops the current game in progress - the farting and jumping
    private void StopGame()
    {
        foreach (GameObject mover in movers)
        {
            mover.GetComponent<Mover>().StopMoving();
        }
    }

    // When the app is closed...might rename to "OnAppClosed"
    private void EndGame()
    {
        
    }

    public void KillPlayer(ButtController buttController)
    {
        buttController.DisableMovement();
    }
}