using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Mover Data",menuName = "Scriptable Objects/Mover Data")]
public class MoverData : ScriptableObject
{
    public Vector3 moveDirection;
    public float moveSpeed;
    public Vector2 boundaries;
    public Vector2 respawnPosition; 
}
