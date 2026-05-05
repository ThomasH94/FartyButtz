using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[CreateAssetMenu(menuName = "Localization/Font Config")]
public class LocalizationConfig : ScriptableObject
{
    public List<LanguageFont> Fonts;

    private Dictionary<SystemLanguage, TMP_FontAsset> _lookup;

    public void Build()
    {
        _lookup = new Dictionary<SystemLanguage, TMP_FontAsset>();

        foreach (var f in Fonts)
        {
            if (f.Font != null)
                _lookup[f.Language] = f.Font;
        }
    }

    public TMP_FontAsset Get(SystemLanguage lang)
    {
        if (_lookup == null)
            Build();

        if (_lookup.TryGetValue(lang, out var font))
            return font;

        // fallback to English
        if (_lookup.TryGetValue(SystemLanguage.English, out var fallback))
            return fallback;

        return null;
    }
}

[Serializable]
public class LanguageFont
{
    public SystemLanguage Language;
    public TMP_FontAsset Font;
}