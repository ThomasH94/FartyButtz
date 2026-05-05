using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class LocalizedText : MonoBehaviour
{
    public string Key;

    [SerializeReference]
    public object[] Arguments;

    [Header("Font")]
    public bool OverrideFont = true;

    private TextMeshProUGUI _text;

    private void Awake()
    {
        _text = GetComponent<TextMeshProUGUI>();
        Refresh();

        EventBus.Subscribe<LanguageChangedPayload>(OnRefresh);
    }

    private void OnDestroy()
    {
        EventBus.Unsubscribe<LanguageChangedPayload>(OnRefresh);
    }

    private void OnRefresh(LanguageChangedPayload payload)
    {
        Refresh();
    }

    public void SetArgs(params object[] args)
    {
        Arguments = args;
        Refresh();
    }

    public void Refresh()
    {
        // Text
        _text.text = LocalizationManager.Localize(Key, Arguments);

        // Font
        if (OverrideFont)
        {
            var font = LocalizationManager.Instance.GetFont();
            if (font != null && _text.font != font)
            {
                _text.font = font;
                _text.UpdateFontAsset();
            }
        }
    }
}