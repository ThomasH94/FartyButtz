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
    
    [ContextMenu("Start Game")]
    private void StartGame()
    {
        player.GetComponent<Rigidbody2D>().simulated = true;
        foreach (GameObject mover in movers)
        {
            mover.GetComponent<Mover>().StartMoving();
        }
    }

    public void KillPlayer(ButtController buttController)
    {
        buttController.DisableMovement();
    }
}