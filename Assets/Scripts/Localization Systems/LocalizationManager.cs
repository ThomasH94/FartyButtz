using System;
using TMPro;
using UnityEngine;

public class LanguageChangedPayload
{
    public SystemLanguage Language { get; set; }
}

public class LocalizationManager : SingletonMonoBehaviour<LocalizationManager>
{
    public LocDB Database;
    public LocalizationConfig FontConfig;

    public SystemLanguage CurrentLanguage { get; private set; } = SystemLanguage.English;

    protected override void Awake()
    {
        base.Awake();
        Database.Build();

        EventBus.Subscribe<LanguageChangedPayload>(OnLanguageChanged);
    }

    private void OnDestroy()
    {
        EventBus.Unsubscribe<LanguageChangedPayload>(OnLanguageChanged);
    }

    private void OnLanguageChanged(LanguageChangedPayload evt)
    {
        SetLanguage(evt.Language);
    }

    public void SetLanguage(SystemLanguage language)
    {
        CurrentLanguage = language;
        EventBus.Publish(new LanguageChangedPayload { Language = language });
    }

    // --- API ---

    public static string Localize(string key)
    {
        return Instance.Database.Get(key, Instance.CurrentLanguage);
    }

    public static string Localize(string key, params object[] args)
    {
        var format = Instance.Database.Get(key, Instance.CurrentLanguage);
        return SafeFormat(format, args);
    }

    public static string Format(string raw, params object[] args)
    {
        return SafeFormat(raw, args);
    }

    private static string SafeFormat(string format, params object[] args)
    {
        try
        {
            return string.Format(format, args);
        }
        catch
        {
            return $"[FormatError:{format}]";
        }
    }

    public TMP_FontAsset GetFont()
    {
        return FontConfig != null 
            ? FontConfig.Get(CurrentLanguage) 
            : null;
    }
}