using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Audio/SfxData")]
public class SfxData : AudioData
{
	[Serializable]
	public struct ClipEntry
	{
		public AudioClip clip;

		public float weight;
	}

	public List<ClipEntry> clips = new List<ClipEntry>();

	public Vector2 pitchRange = new Vector2(1f, 1f);

	public Vector2 volumeRange = new Vector2(1f, 1f);

	public bool spatial;

	public float spatialBlend = 1f;

	public bool oneShot = true;

	public bool loop;

	public AudioClip GetRandomClip()
	{
		if (clips == null || clips.Count == 0)
		{
			return null;
		}
		float num = 0f;
		foreach (ClipEntry clip in clips)
		{
			num += Mathf.Max(0.001f, clip.weight);
		}
		float num2 = UnityEngine.Random.value * num;
		foreach (ClipEntry clip2 in clips)
		{
			num2 -= Mathf.Max(0.001f, clip2.weight);
			if (num2 <= 0f && clip2.clip != null)
			{
				return clip2.clip;
			}
		}
		return clips[clips.Count - 1].clip;
	}
}
