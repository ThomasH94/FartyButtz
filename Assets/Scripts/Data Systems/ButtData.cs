using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/ButtData")]
public class ButtData : DataSO
{
    [Header("PlayFab")]
    [Tooltip("Must match the Item ID exactly as entered in the PlayFab catalog")]
    public string PlayFabItemId;

    [Tooltip("Cost in Coins (CN). 0 = not purchasable with coins.")]
    public int CoinCost;

    [Tooltip("Cost in Gems (GM). 0 = not purchasable with gems.")]
    public int GemCost;

    [Header("Visuals")]
    public Sprite ButtSprite;

    [Header("Audio")]
    public SfxData FartSFX;

    // public ParticleSystem FartParticles;

    public bool IsFree => CoinCost == 0 && GemCost == 0;
}