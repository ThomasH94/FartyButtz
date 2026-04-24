using System;
using Sirenix.Serialization;
using UnityEngine;

[DefaultExecutionOrder(-1)]
public class GameManager : SingletonMonoBehaviour<GameManager>
{
    protected override void Awake()
    {
        base.Awake();
    }

    private void Start()
    {
        EventBus.Publish(new MenuRequestOpenPayload(typeof(MainMenu), null));
    }
}