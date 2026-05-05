using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(menuName = "Localization/Database")]
public class LocDB : SerializedScriptableObject
{
    public List<SystemLanguage> Languages = new();
    public List<LocalizationRow> Rows = new();

    private Dictionary<string, Dictionary<SystemLanguage, string>> _table;

    public void Build()
    {
        _table = new Dictionary<string, Dictionary<SystemLanguage, string>>();

        foreach (var row in Rows)
        {
            if (string.IsNullOrEmpty(row.Key))
                continue;

            if (!_table.ContainsKey(row.Key))
                _table[row.Key] = new Dictionary<SystemLanguage, string>();

            for (int i = 0; i < Languages.Count; i++)
            {
                var lang = Languages[i];

                string value = (row.Values != null && i < row.Values.Length)
                    ? row.Values[i]
                    : string.Empty;

                _table[row.Key][lang] = value;
            }
        }
    }

    public string Get(string key, SystemLanguage lang)
    {
        if (_table == null)
            Build();

        if (_table.TryGetValue(key, out var langTable))
        {
            if (langTable.TryGetValue(lang, out var value) && !string.IsNullOrEmpty(value))
                return value;

            // fallback to English
            if (langTable.TryGetValue(SystemLanguage.English, out var fallback))
                return fallback;
        }

        return $"[Missing:{key}]";
    }
}

[Serializable]
public class LocalizationRow
{
    public string Key;

    [TextArea(2, 5)] // multi-line support
    public string[] Values;
}