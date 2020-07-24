using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mover : MonoBehaviour
{
    public MoverData moverData;
    public bool setRandomY = false;

    public bool canMove = false;

    public void StartMoving()
    {
        canMove = true;
    }

    public void StopMoving()
    {
        canMove = false;
    }

    private void FixedUpdate()
    {
        if (!canMove)
        {
            return;
        }
        
        transform.position += (moverData.moveDirection * moverData.moveSpeed * Time.deltaTime);
        
        if(transform.position.x < moverData.boundaries.x)
        {
            ReSpawn();
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // Nothing to report..
        }
    }

    private void ReSpawn()
    {
        // TODO: Set the position of this object equal to the last item in the pools position so it snaps to the end of it
        if (!setRandomY)
        {
            transform.position = moverData.respawnPosition;   
        }
        else
        {
            float randomY = Random.Range(0, 0.5f);
            transform.position = new Vector3(moverData.respawnPosition.x, randomY, 0);
        }
    }
    
    private void OnBecameInvisible()
    {
        // Remove from the pool or something..
        transform.position = moverData.respawnPosition;
        Debug.Log("You lost me");
    }
}   
