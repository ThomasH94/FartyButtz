using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(2000)]
public class ScreenFader : SingletonMonoBehaviour<ScreenFader>
{
	[Header("Assign in Inspector (optional)")]
	[SerializeField]
	private Canvas targetCanvas;

	[SerializeField]
	private Image fadeImage;

	[Header("Defaults")]
	[SerializeField]
	private Color fadeColor = Color.black;

	[SerializeField]
	private int canvasSortOrder = 1000;

	private CanvasGroup _canvasGroup;

	protected override void Awake()
	{
		base.Awake();
		EnsureSetup();
	}

	private void EnsureSetup()
	{
		if (_canvasGroup != null)
		{
			return;
		}
		if (targetCanvas != null && fadeImage != null)
		{
			_canvasGroup = targetCanvas.GetComponent<CanvasGroup>();
			if (_canvasGroup == null)
			{
				_canvasGroup = targetCanvas.gameObject.AddComponent<CanvasGroup>();
			}
			fadeImage.color = fadeColor;
			fadeImage.raycastTarget = false;
			return;
		}
		Canvas canvas = Object.FindObjectOfType<Canvas>();
		if (canvas != null && targetCanvas == null)
		{
			targetCanvas = canvas;
		}
		if (targetCanvas != null)
		{
			_canvasGroup = targetCanvas.GetComponent<CanvasGroup>() ?? targetCanvas.gameObject.AddComponent<CanvasGroup>();
			if (fadeImage == null)
			{
				Transform transform = targetCanvas.transform.Find("ScreenFaderImage");
				if (transform != null)
				{
					fadeImage = transform.GetComponent<Image>();
				}
			}
		}
		if (targetCanvas == null || fadeImage == null)
		{
			CreateCanvasAndImage(out targetCanvas, out fadeImage, out _canvasGroup);
		}
		fadeImage.color = fadeColor;
		fadeImage.raycastTarget = false;
	}

	private void CreateCanvasAndImage(out Canvas canvas, out Image image, out CanvasGroup cg)
	{
		GameObject gameObject = new GameObject("ScreenFader_Canvas");
		canvas = gameObject.AddComponent<Canvas>();
		canvas.renderMode = RenderMode.ScreenSpaceOverlay;
		canvas.sortingOrder = canvasSortOrder;
		canvas.gameObject.layer = LayerMask.NameToLayer("UI");
		cg = canvas.gameObject.AddComponent<CanvasGroup>();
		cg.blocksRaycasts = false;
		GameObject gameObject2 = new GameObject("ScreenFaderImage");
		gameObject2.transform.SetParent(canvas.transform, worldPositionStays: false);
		image = gameObject2.AddComponent<Image>();
		image.rectTransform.anchorMin = Vector2.zero;
		image.rectTransform.anchorMax = Vector2.one;
		image.rectTransform.offsetMin = Vector2.zero;
		image.rectTransform.offsetMax = Vector2.zero;
		image.color = fadeColor;
		image.raycastTarget = false;
	}

	public async UniTask DoTransition(float start, float end, float duration)
	{
		EnsureSetup();
		start = Mathf.Clamp01(start);
		end = Mathf.Clamp01(end);
		if (duration <= 0f)
		{
			SetAlpha(end);
			return;
		}
		float t = 0f;
		SetAlpha(start);
		while (t < duration)
		{
			await UniTask.Yield(PlayerLoopTiming.Update);
			t += Time.deltaTime;
			float alpha = Mathf.Lerp(start, end, t / duration);
			SetAlpha(alpha);
		}
		SetAlpha(end);
	}

	public UniTask FadeOut(float duration = 1f)
	{
		return DoTransition(0f, 1f, duration);
	}

	public UniTask FadeIn(float duration = 1f)
	{
		return DoTransition(1f, 0f, duration);
	}

	private void SetAlpha(float a)
	{
		EnsureSetup();
		if (_canvasGroup == null)
		{
			_canvasGroup = targetCanvas.GetComponent<CanvasGroup>();
		}
		if (_canvasGroup != null)
		{
			_canvasGroup.alpha = a;
			_canvasGroup.blocksRaycasts = a > 0.01f;
		}
		else if (fadeImage != null)
		{
			Color color = fadeImage.color;
			color.a = a;
			fadeImage.color = color;
			fadeImage.raycastTarget = a > 0.01f;
		}
	}
}
