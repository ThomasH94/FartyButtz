using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class LocalizationEntry
{
    public string Key;

    // Serialized for editor friendliness
    public List<LanguageValue> Values;

    // Runtime cache (not serialized)
    private Dictionary<SystemLanguage, string> _lookup;

    public void Build()
    {
        _lookup = new Dictionary<SystemLanguage, string>();
        foreach (var v in Values)
        {
            _lookup[v.Language] = v.Text;
        }
    }

    public string Get(SystemLanguage lang)
    {
        if (_lookup == null) Build();

        if (_lookup.TryGetValue(lang, out var value))
            return value;

        // fallback
        if (_lookup.TryGetValue(SystemLanguage.English, out var fallback))
            return fallback;

        return $"[Missing:{Key}]";
    }
}

[Serializable]
public class LanguageValue
{
    public SystemLanguage Language;
    [TextArea] public string Text;
}