using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class GroundPooler : BasePooler
{
    [SerializeField] private float xOffset = 0f;    // How far the objects should be spawned from each other
    private void Start()
    {
        // Create a pool or something..
        CreatePool();
    }

    protected override void CreatePool()
    {
        int index = Random.Range(0,poolObjects.Length);
        // Create the pool by instantiating the objects we need
        for (int i = 0; i < poolSize; i++)
        {
            GameObject groundPiece = Instantiate(poolObjects[index], transform, true);    // Create the pooled object and parent it to this object
            groundPiece.name = "Ground Piece(" + i + ")";
            groundPiece.transform.position = new Vector2(objectSpawnPosition.x + (i * xOffset), objectSpawnPosition.y);
            GameManager.Instance.movers.Add(groundPiece);
            groundPiece.GetComponent<Mover>().StartMoving();
        }
        
    }


}
