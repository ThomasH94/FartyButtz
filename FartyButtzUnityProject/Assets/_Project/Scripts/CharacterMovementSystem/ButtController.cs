using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtController : MonoBehaviour
{
    public Rigidbody2D buttRigidBody;

    public float holdTime;

    public Vector2 fartForce;

    public ButtData buttData;

    public SpriteRenderer buttRenderer;

    public bool isDebugMode = false;

    private bool canMove = true;

    #region Events

    public static Action OnPlayerDied;

    #endregion
    
    private void Start()
    {
        SetupButt();
    }

    private void SetupButt()
    {
        // Set our fart force based on the fart force in our butt data..TEMP
        fartForce = new Vector2(fartForce.x, buttData.fartForce);
        buttRenderer.sprite = buttData.buttSprite;
        buttRigidBody.gravityScale = buttData.gravityScale;
    }
    
    private void Update()
    {
        if(canMove)
            Fart();
    }

    void Fart()
    {
        // Fart
        if (Input.GetButtonDown("Fire1"))
        {
            buttRigidBody.velocity = Vector3.zero;
            buttRigidBody.AddForce(fartForce);
        }

        if (isDebugMode)
        {
            // Reset the level if you hold your fart long enough
            if (Input.GetButton("Fire1"))
            {
                holdTime += Time.deltaTime;
                if (holdTime > 3f)
                {
                    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                }
            }
            // Reset restart time whenever we release the mouse button
            if (Input.GetMouseButtonUp(0))
            {
                holdTime = 0f;
                Debug.Log("I did it");
            }
        }
    }

    public void DisableMovement()
    {
        OnPlayerDied?.Invoke();    // Did the player die AND have subscribers? Invoke.
        canMove = false;
    }
    
}