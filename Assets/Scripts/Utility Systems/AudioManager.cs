using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : SingletonMonoBehaviour<AudioManager>
{
	[SerializeField]
	private AudioSource musicSourceA;

	[SerializeField]
	private AudioSource musicSourceB;

	[SerializeField]
	private AudioSource sfxPrefab;

	[SerializeField]
	private int initialSfxPoolSize = 8;

	private AudioSource _activeMusic;

	private AudioSource _inactiveMusic;

	private Coroutine _musicRoutine;

	private readonly List<AudioSource> _sfxPool = new List<AudioSource>();

	public bool testMusicCrossfade = true;

	[Range(0f, 1f)]
	public float testMusicVolume = 1f;
	

	public bool testSfxSpatial;

	public Vector3 testSfxWorldPos = Vector3.zero;

	private void Odin_StopAllSfx()
	{
		StopAllSFX();
	}

	protected override void Awake()
	{
		base.Awake();
		
		_activeMusic = musicSourceA;
		_inactiveMusic = musicSourceB;
		if (_activeMusic == null || _inactiveMusic == null)
		{
			AudioSource[] componentsInChildren = GetComponentsInChildren<AudioSource>(includeInactive: true);
			if (componentsInChildren != null && componentsInChildren.Length != 0)
			{
				if (_activeMusic == null)
				{
					_activeMusic = componentsInChildren[0];
				}
				if (_inactiveMusic == null)
				{
					_inactiveMusic = ((componentsInChildren.Length > 1) ? componentsInChildren[1] : componentsInChildren[0]);
				}
				Debug.Log("[AudioManager] Assigned missing music source(s) from child AudioSources.");
			}
		}
		if (_activeMusic == null || _inactiveMusic == null)
		{
			Debug.LogError("[AudioManager] musicSourceA or musicSourceB is null at runtime. Make sure the AudioManager instance in the scene has them assigned.");
			return;
		}
		AudioSource activeMusic = _activeMusic;
		bool playOnAwake = (_inactiveMusic.playOnAwake = false);
		activeMusic.playOnAwake = playOnAwake;
		_activeMusic.volume = 0f;
		_inactiveMusic.volume = 0f;
		for (int i = 0; i < initialSfxPoolSize; i++)
		{
			_sfxPool.Add(CreateSfxSource());
		}
	}

	public AudioSource PlayMusic(MusicData music, bool forceCrossfade = true, float targetVolumeMultiplier = 1f)
	{
		if (music == null)
		{
			return null;
		}
		if (_musicRoutine != null)
		{
			StopCoroutine(_musicRoutine);
			_musicRoutine = null;
		}
		_musicRoutine = StartCoroutine(MusicRoutine(music, forceCrossfade, targetVolumeMultiplier));
		if (!(music.loopClip != null))
		{
			return _activeMusic;
		}
		return _inactiveMusic;
	}

	public void StopMusic(bool fade = false, float fadeDuration = 0.5f)
	{
		if (!fade)
		{
			_activeMusic.Stop();
			_inactiveMusic.Stop();
			return;
		}
		if (_musicRoutine != null)
		{
			StopCoroutine(_musicRoutine);
			_musicRoutine = null;
		}
		StartCoroutine(FadeOutAndStop(_activeMusic, fadeDuration));
		StartCoroutine(FadeOutAndStop(_inactiveMusic, fadeDuration));
	}

	private IEnumerator MusicRoutine(MusicData music, bool forceCrossfade, float targetVolumeMultiplier)
	{
		float targetVolume = Mathf.Clamp01(music.defaultVolume) * Mathf.Clamp01(targetVolumeMultiplier);
		if (_activeMusic == null && _inactiveMusic == null)
		{
			Debug.LogError("AudioManager: both music sources are null.");
			yield break;
		}
		if (_inactiveMusic == null)
		{
			_inactiveMusic = ((_activeMusic == musicSourceA) ? musicSourceB : musicSourceA);
			if (_inactiveMusic == null)
			{
				Debug.LogWarning("AudioManager: inactive music source was null; using active source for playback.");
				_inactiveMusic = _activeMusic;
			}
		}
		if (_activeMusic == null)
		{
			_activeMusic = ((_inactiveMusic == musicSourceA) ? musicSourceB : musicSourceA);
			if (_activeMusic == null)
			{
				_activeMusic = _inactiveMusic;
			}
		}
		if (music.introClip != null)
		{
			_activeMusic.Stop();
			_activeMusic.clip = music.introClip;
			_activeMusic.loop = false;
			_activeMusic.volume = targetVolume;
			_activeMusic.Play();
			yield return new WaitWhile(() => _activeMusic.isPlaying);
		}
		if (music.loopClip != null)
		{
			AudioSource loopTarget = _inactiveMusic;
			if (loopTarget == null)
			{
				loopTarget = _activeMusic;
			}
			loopTarget.Stop();
			loopTarget.clip = music.loopClip;
			loopTarget.loop = music.loop;
			loopTarget.volume = 0f;
			loopTarget.Play();
			float t = 0f;
			float dur = (forceCrossfade ? Mathf.Max(0f, music.crossfadeTime) : 0f);
			if (dur <= 0f)
			{
				loopTarget.volume = targetVolume;
				_activeMusic.Stop();
				SwapActiveInactive();
			}
			else
			{
				while (t < dur)
				{
					t += Time.deltaTime;
					float t2 = Mathf.Clamp01(t / dur);
					loopTarget.volume = Mathf.Lerp(0f, targetVolume, t2);
					_activeMusic.volume = Mathf.Lerp(targetVolume, 0f, t2);
					yield return null;
				}
				loopTarget.volume = targetVolume;
				_activeMusic.Stop();
				SwapActiveInactive();
			}
			_musicRoutine = null;
			yield break;
		}
		AudioClip audioClip = music.introClip ?? _activeMusic.clip;
		if (audioClip == null)
		{
			_musicRoutine = null;
			yield break;
		}
		_activeMusic.Stop();
		_activeMusic.clip = audioClip;
		_activeMusic.loop = false;
		_activeMusic.volume = targetVolume;
		_activeMusic.Play();
		if (music.useLoopPoint)
		{
			int num = Mathf.Max(1, _activeMusic.clip.frequency);
			int startSample = Mathf.Clamp(Mathf.FloorToInt(music.loopStartSeconds * (float)num), 0, _activeMusic.clip.samples - 1);
			int endSample = Mathf.Clamp(Mathf.FloorToInt(music.loopEndSeconds * (float)num), 0, _activeMusic.clip.samples);
			if (endSample <= startSample)
			{
				endSample = _activeMusic.clip.samples;
			}
			while (_activeMusic.isPlaying && !(_activeMusic.clip == null) && _activeMusic.clip.samples > 0)
			{
				if (_activeMusic.timeSamples >= endSample)
				{
					_activeMusic.timeSamples = startSample;
				}
				yield return null;
			}
			yield break;
		}
		if (music.loop)
		{
			_activeMusic.loop = true;
			if (!_activeMusic.isPlaying)
			{
				_activeMusic.Play();
			}
		}
		else
		{
			if (!_activeMusic.isPlaying)
			{
				_activeMusic.Play();
			}
			yield return new WaitWhile(() => _activeMusic.isPlaying);
		}
		_musicRoutine = null;
	}

	private void SwapActiveInactive()
	{
		AudioSource activeMusic = _activeMusic;
		_activeMusic = _inactiveMusic;
		_inactiveMusic = activeMusic;
	}

	private IEnumerator FadeOutAndStop(AudioSource src, float duration)
	{
		if (!(src == null))
		{
			float start = src.volume;
			float t = 0f;
			while (t < duration)
			{
				t += Time.deltaTime;
				src.volume = Mathf.Lerp(start, 0f, t / duration);
				yield return null;
			}
			src.Stop();
			src.volume = start;
		}
	}

	public AudioSource PlaySFX(SfxData data, Vector3? worldPos = null)
	{
		if (data == null)
		{
			return null;
		}
		AudioClip randomClip = data.GetRandomClip();
		if (randomClip == null)
		{
			return null;
		}
		float volume = Random.Range(data.volumeRange.x, data.volumeRange.y) * data.defaultVolume;
		float pitch = Random.Range(data.pitchRange.x, data.pitchRange.y);
		if (data.spatial && worldPos.HasValue)
		{
			GameObject gameObject = new GameObject("SFX3D_" + data.name);
			gameObject.transform.position = worldPos.Value;
			AudioSource audioSource = gameObject.AddComponent<AudioSource>();
			audioSource.clip = randomClip;
			audioSource.spatialBlend = data.spatialBlend;
			audioSource.volume = volume;
			audioSource.pitch = pitch;
			audioSource.loop = data.loop;
			audioSource.Play();
			if (!data.loop)
			{
				Object.Destroy(gameObject, randomClip.length / Mathf.Max(0.01f, Mathf.Abs(audioSource.pitch)) + 0.1f);
			}
			return audioSource;
		}
		AudioSource obj = _sfxPool.Find((AudioSource s) => !s.isPlaying) ?? CreateAndAddSfxSource();
		obj.clip = randomClip;
		obj.volume = volume;
		obj.pitch = pitch;
		obj.loop = data.loop;
		obj.spatialBlend = 0f;
		obj.Play();
		return obj;
	}

	public void StopSFX(AudioSource src, float fadeDuration = 0.25f)
	{
		if (!(src == null))
		{
			StartCoroutine(FadeOutAndStop(src, fadeDuration));
		}
	}

	public void StopAllSFX()
	{
		foreach (AudioSource item in _sfxPool)
		{
			if (item.isPlaying)
			{
				item.Stop();
			}
		}
	}

	private AudioSource CreateSfxSource()
	{
		if (sfxPrefab == null)
		{
			GameObject obj = new GameObject("SFX_PooledSource");
			obj.transform.SetParent(base.transform);
			AudioSource audioSource = obj.AddComponent<AudioSource>();
			audioSource.playOnAwake = false;
			return audioSource;
		}
		AudioSource audioSource2 = Object.Instantiate(sfxPrefab, base.transform);
		audioSource2.playOnAwake = false;
		return audioSource2;
	}

	private AudioSource CreateAndAddSfxSource()
	{
		AudioSource audioSource = CreateSfxSource();
		_sfxPool.Add(audioSource);
		return audioSource;
	}
}
