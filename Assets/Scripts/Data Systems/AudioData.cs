using UnityEngine;

public abstract class AudioData : ScriptableObject
{
	[Range(0f, 1f)]
	public float defaultVolume = 1f;

	public int priority = 128;

	public string tag;
}
