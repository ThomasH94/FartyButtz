using UnityEngine;

public class PlayerInputHandler : MonoBehaviour
{
    private PlayerController m_Player;

    public void Initialize(PlayerController player) => m_Player = player;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
            m_Player.Fart();
    }
}