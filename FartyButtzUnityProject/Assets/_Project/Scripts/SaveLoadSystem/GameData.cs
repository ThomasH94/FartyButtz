using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The purpose of this class is store store ALL of our defined Game Data that will be saved and loaded
/// with the SaveGameManager
/// The current approach is using the User Profiles and manipulating the data within those profiles
/// </summary>
[CreateAssetMenu(fileName = "New Game Data", menuName = "Scriptable Objects/Game Data")]
public class GameData : ScriptableObject
{
    public string profileName;
    public int highScore;
    public int currentActiveButtIndex;

}
