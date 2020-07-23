using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathManager : MonoBehaviour
{
    private void OnEnable()
    {
        ButtController.OnPlayerDied += DeathPresentation;
    }

    private void OnDisable()
    {
        ButtController.OnPlayerDied -= DeathPresentation;
    }


    private void DeathPresentation()
    {
        Debug.Log(this.gameObject.name + " has received the PlayerDied message.");
    }
}
