using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

public class PlayerController : SerializedMonoBehaviour
{
    #region DEBUG

    public ButtData DEBUGBUTTDATA;
    #endregion
    
    public Sprite[] sprites;
    public float strength = 5f;
    public float gravity = -9.81f;
    public float tilt = 5f;

    [OdinSerialize]
    private SpriteRenderer spriteRenderer;
    private Vector3 direction;
    private int spriteIndex;
    private ButtData m_ButtData;
    private SfxData m_FartAudio;

    private void Start()
    {
        //InvokeRepeating(nameof(AnimateSprite), 0.15f, 0.15f);
    }

    private void OnEnable()
    {
        Vector3 position = transform.position;
        position.y = 0f;
        transform.position = position;
        direction = Vector3.zero;
        
        SetupButt(DEBUGBUTTDATA);
    }
    
    private void SetupButt(ButtData buttData)
    {
        m_ButtData = buttData;
        spriteRenderer.sprite = m_ButtData.ButtSprite;
        m_FartAudio = buttData.FartSFX;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
        {
            Fart();
        }

        // Apply gravity and update the position
        direction.y += gravity * Time.deltaTime;
        transform.position += direction * Time.deltaTime;

        // Tilt the bird based on the direction
        Vector3 rotation = transform.eulerAngles;
        rotation.z = direction.y * tilt;
        transform.eulerAngles = rotation;
    }

    private void Fart()
    {
        direction = Vector3.up * strength;
        AudioManager.Instance.PlaySFX(m_FartAudio);
    }

    private void AnimateSprite()
    {
        spriteIndex++;

        if (spriteIndex >= sprites.Length) {
            spriteIndex = 0;
        }

        if (spriteIndex < sprites.Length && spriteIndex >= 0) {
            spriteRenderer.sprite = sprites[spriteIndex];
        }
    }

}
