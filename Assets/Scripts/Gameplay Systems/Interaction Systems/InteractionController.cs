using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

public class InteractionController : SerializedMonoBehaviour
{
    private GameObject m_Other = null;
    
    public bool IsTrigger = true;
    [OdinSerialize]
    public List<GameAction>  Actions = new List<GameAction>();
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (IsTrigger)
        {
            m_Other = other.gameObject;
            GameActionContext context = new GameActionContext(null, this, other);
            foreach (GameAction action in Actions)
            {
                action.Execute(context);
            }
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (IsTrigger && m_Other != null)
        {
            
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (!IsTrigger)
        {
            
        }
    }

    private void OnCollisionExit2D(Collision2D other)
    {
        if (!IsTrigger)
        {
            
        }
    }
}
