using UnityEngine;

[CreateAssetMenu(menuName = "Audio/MusicData")]
public class MusicData : AudioData
{
	public AudioClip introClip;

	public AudioClip loopClip;

	public bool loop = true;

	public bool useLoopPoint;

	public float loopStartSeconds;

	public float loopEndSeconds;

	public float crossfadeTime = 0.5f;

	public bool persistAcrossScenes = true;
}
