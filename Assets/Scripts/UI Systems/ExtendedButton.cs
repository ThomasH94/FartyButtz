using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ExtendedButton : Button, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
	[Header("Sound Effects")]
	[Tooltip("Audio clip to play when the button is hovered over.")]
	public AudioClip hoverSound;

	[Tooltip("Audio clip to play when the button is clicked.")]
	public AudioClip clickSound;

	private AudioSource _audioSource;

	[Header("Cooldown")]
	[Tooltip("If greater than 0, prevents multiple clicks within this duration.")]
	public float clickCooldown;

	private float _lastClickTime;

	public event Action OnClickAction;

	protected override void Awake()
	{
		base.Awake();
		_audioSource = GetComponent<AudioSource>();
		if (_audioSource == null)
		{
			_audioSource = base.gameObject.AddComponent<AudioSource>();
			_audioSource.playOnAwake = false;
			_audioSource.spatialBlend = 0f;
		}
		base.onClick.AddListener(HandleBaseClick);
	}

	public override void OnPointerEnter(PointerEventData eventData)
	{
		base.OnPointerEnter(eventData);
		if (base.interactable && hoverSound != null && _audioSource != null)
		{
			_audioSource.PlayOneShot(hoverSound);
		}
	}

	public override void OnPointerExit(PointerEventData eventData)
	{
		base.OnPointerExit(eventData);
		_ = base.interactable;
	}

	public override void OnPointerDown(PointerEventData eventData)
	{
		base.OnPointerDown(eventData);
		_ = base.interactable;
	}

	public override void OnPointerUp(PointerEventData eventData)
	{
		base.OnPointerUp(eventData);
		_ = base.interactable;
	}

	private void HandleBaseClick()
	{
		if (clickCooldown > 0f && Time.time < _lastClickTime + clickCooldown)
		{
			Debug.Log("Button on cooldown. Please wait.");
			return;
		}
		_lastClickTime = Time.time;
		if (clickSound != null && _audioSource != null)
		{
			_audioSource.PlayOneShot(clickSound);
		}
		this.OnClickAction?.Invoke();
		Debug.Log("Extended Button Clicked!");
	}

	public void SetInteractable(bool canInteract)
	{
		base.interactable = canInteract;
	}

	public void RegisterClickAction(Action action)
	{
		OnClickAction += action;
	}

	public void UnregisterClickAction(Action action)
	{
		OnClickAction -= action;
	}
}
