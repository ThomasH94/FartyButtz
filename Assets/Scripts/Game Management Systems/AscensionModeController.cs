// Ascension just tells the player/camera to reorient — no player subclass needed

using System.Collections.Generic;
using Sirenix.Serialization;
using UnityEngine;

public class AscensionModeController : AbstractGameMode
{
    public override void StartGame()
    {
        // Rotate the world/camera 90 degrees, adjust gravity direction
        Physics2D.gravity = new Vector2(-9.81f, 0f); // or rotate player root
        m_PlayerController.SetGravityDirection(Vector2.left);
        // ...
    }

    public override void PauseGame()
    {

    }

    public override void EndGame()
    {

    }

    public override void OnScoreChanged(int newScore)
    {

    }
}

// Boss Rush injects abilities
public class BossRushModeController : AbstractGameMode
{
    [OdinSerialize] private List<IPlayerAbility> m_StartingAbilities;

    public override void StartGame()
    {
        foreach (var ability in m_StartingAbilities)
            m_PlayerController.AddAbility(ability);
        // spawn first boss...
    }

    public override void PauseGame()
    {
        
    }

    public override void EndGame()
    {

    }

    public override void OnScoreChanged(int newScore)
    {

    }
}