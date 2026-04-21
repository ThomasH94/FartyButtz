using System.Collections.Generic;
using UnityEngine;

public class GameActionContext
{
    public object Instigator;
    public List<object> Targets = new List<object>();
    public object Data;

    public GameActionContext() { }

    public GameActionContext(object data, object instigator, params object[] targets)
    {
        Data = data;
        Instigator = instigator;
        Targets = new List<object>(targets);
    }

    public T GetData<T>()
    {
        if (Data is T typedData)
            return typedData;

        Debug.LogWarning($"GameActionContext.GetData<{typeof(T).Name}>() returned null.");
        return default;
    }

    public IEnumerable<T> GetTargets<T>()
    {
        foreach (var target in Targets)
        {
            if (target is T typed)
                yield return typed;
        }
    }

    public GameObject InstigatorGO => Instigator as GameObject;

    public IEnumerable<GameObject> TargetGOs
    {
        get
        {
            foreach (var target in Targets)
            {
                if (target is GameObject go)
                    yield return go;
            }
        }
    }
}