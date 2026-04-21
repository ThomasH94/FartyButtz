using System;
using UnityEngine;


public abstract class GameAction
{
    public abstract void Execute(GameActionContext context);
}