using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// This script will be the based Pooling script that other Poolers will inherit from
/// This will take an object to pool and an amount to keep in the Pool
/// </summary>
public abstract class BasePooler : MonoBehaviour
{
    [SerializeField] protected GameObject[] poolObjects; 
    [SerializeField] protected int poolSize;
    [SerializeField] protected Vector2 objectSpawnPosition;

    protected abstract void CreatePool();
}
