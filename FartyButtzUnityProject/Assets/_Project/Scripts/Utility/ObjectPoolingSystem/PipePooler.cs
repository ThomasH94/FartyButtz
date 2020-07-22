using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PipePooler : BasePooler
{
    [SerializeField] private float xOffset = 0f;
    public float yOffset;
    //[SerializeField, MinMaxSlider(-1,2)] private Vector2 yOffset = new Vector2();

    private void Start()
    {
        CreatePool();
    }
    
    protected override void CreatePool()
    {
        int index = Random.Range(0,poolObjects.Length);

        for (int i = 0; i < poolSize; i++)
        {
            GameObject pipe = Instantiate(poolObjects[index], transform, true);
            pipe.name = "Pipes " + i;
            
            float randomSpawnOffset = Random.Range(-0.5f, yOffset);
            Vector2 spawnPosition = new Vector2(objectSpawnPosition.x + (i + xOffset), randomSpawnOffset);
            pipe.transform.position = spawnPosition;
            GameManager.Instance.movers.Add(pipe);
        }

    }
}
