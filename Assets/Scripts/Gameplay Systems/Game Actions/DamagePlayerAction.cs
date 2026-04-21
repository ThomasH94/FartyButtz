using System;
using UnityEngine;

public class DamagePlayerAction : GameAction
{
    public override void Execute(GameActionContext context)
    {
        PlayerController player = context.GetData<PlayerController>();
        EventBus.Publish(new OnPlayerDeathPayload(player));
        Debug.Log("Player Damaged");
    }
}