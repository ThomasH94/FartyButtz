// Handles ONLY physics/movement math. No input, no mode logic.

using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

public class PlayerController : SerializedMonoBehaviour
{
    public float strength = 5f;
    public float gravity = -9.81f;
    public float tilt = 5f;

    private Vector3 m_Direction = Vector3.zero;
    [OdinSerialize] private SpriteRenderer m_SpriteRenderer = null;
    
    private List<IPlayerAbility> m_Abilities = new List<IPlayerAbility>();

    public void Initialize(ButtData buttData)
    {
        m_SpriteRenderer.sprite = buttData.ButtSprite;
        // etc.
    }

    // Called by input handler or game mode — not by Update directly
    public void Fart()
    {
        m_Direction = Vector3.up * strength;
        // play audio via event or direct call
        EventBus.Publish(new PlayerFartPayload());
    }

    public void SetMovementEnabled(bool enabled) => this.enabled = enabled;

    private void Update()
    {
        m_Direction.y += gravity * Time.deltaTime;
        transform.position += m_Direction * Time.deltaTime;

        Vector3 rotation = transform.eulerAngles;
        rotation.z = m_Direction.y * tilt;
        transform.eulerAngles = rotation;
    }

    public void AddAbility(IPlayerAbility ability)
    {
        
    }

    public void ClearAbilities()
    {
        m_Abilities.Clear();
    }

    public void SetGravityDirection(Vector2 direction)
    {
        
    }
}