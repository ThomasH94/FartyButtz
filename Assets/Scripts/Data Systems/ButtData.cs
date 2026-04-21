using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/ButtData")]
public class ButtData : SerializedScriptableObject
{
    public Sprite ButtSprite;
    public SfxData FartSFX;
    // public ParticleSystem FartParticles;
}
