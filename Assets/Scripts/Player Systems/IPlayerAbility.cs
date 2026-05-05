using UnityEngine;

public interface IPlayerAbility
{
    void Activate(PlayerController player);
    void Tick(PlayerController player, float deltaTime); // for cooldowns etc.
    bool IsReady { get; }
}

public class ShieldAbility : MonoBehaviour, IPlayerAbility
{
    public bool IsReady { get; }
    public void Activate(PlayerController player)
    {
        
    }

    public void Tick(PlayerController player, float deltaTime)
    {

    }
}

public class SlowTimeAbility : MonoBehaviour, IPlayerAbility
{
    public bool IsReady { get; }
    public void Activate(PlayerController player)
    {
        
    }

    public void Tick(PlayerController player, float deltaTime)
    {

    }
}