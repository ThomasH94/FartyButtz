public class OnPlayerDeathPayload
{
    public PlayerController Player {get; private set;}
    
    public OnPlayerDeathPayload(PlayerController player)
    {
        Player = player;
    }
}