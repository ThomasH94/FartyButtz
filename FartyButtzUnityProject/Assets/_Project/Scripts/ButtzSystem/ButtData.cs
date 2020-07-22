using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Buttz Data", menuName = "Scriptable Objects/Buttz Data")]
public class ButtData : ScriptableObject
{
    [Header("Butt Info")]
    public string buttName;
    public int buttID;
    public bool isUnlocked;

    [Header("Butt Data")] 
    public float fartForce;
    public float fartDelay;    // May consider having Buttz with delays to spice up gameplay
    public float gravityScale;
    public Animator buttAnimator;
    public Sprite buttSprite;
    public ParticleSystem buttParticles;
    public AudioClip fartSound;
}
