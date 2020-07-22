using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillOnTouch : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D other)
    {
        ButtController playerButt = other.gameObject.GetComponent<ButtController>();
        if (playerButt)
        {
            // Let's NOT use the singleton and instead fire an event..
            GameManager.Instance.KillPlayer(playerButt);
        }
    }
}
