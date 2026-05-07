#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;

public class LocalizationEditorWindow : EditorWindow
{
    private LocDB db;

    private Vector2 scroll;
    private string search = "";
    private bool showOnlyMissing = false;

    [MenuItem("Tools/Localization Editor")]
    public static void Open()
    {
        GetWindow<LocalizationEditorWindow>("Localization");
    }

    private void OnGUI()
    {
        DrawToolbar();

        if (db == null)
        {
            EditorGUILayout.HelpBox("Assign a Localization Database", MessageType.Info);
            return;
        }

        DrawTable();

        if (GUI.changed)
        {
            EditorUtility.SetDirty(db);
        }
    }

    // =========================
    // TOOLBAR
    // =========================
    private void DrawToolbar()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

        db = (LocDB)EditorGUILayout.ObjectField(
            db, typeof(LocDB), false, GUILayout.Width(250));

        GUILayout.Space(10);

        search = GUILayout.TextField(search, EditorStyles.toolbarSearchField, GUILayout.Width(200));

        showOnlyMissing = GUILayout.Toggle(showOnlyMissing, "Missing Only", EditorStyles.toolbarButton);

        if (GUILayout.Button("Add Row", EditorStyles.toolbarButton))
            AddRow();

        if (GUILayout.Button("Add Language", EditorStyles.toolbarButton))
            AddLanguage();

        if (GUILayout.Button("Import CSV", EditorStyles.toolbarButton))
            ImportCSV();

        if (GUILayout.Button("Export CSV", EditorStyles.toolbarButton))
            ExportCSV();

        if (GUILayout.Button("Validate", EditorStyles.toolbarButton))
            Validate();

        EditorGUILayout.EndHorizontal();
    }

    // =========================
    // TABLE
    // =========================
    private void DrawTable()
    {
        scroll = EditorGUILayout.BeginScrollView(scroll);

        DrawHeader();

        for (int r = 0; r < db.Rows.Count; r++)
        {
            var row = db.Rows[r];

            if (!PassesSearch(row))
                continue;

            if (showOnlyMissing && !RowHasMissing(row))
                continue;

            EditorGUILayout.BeginHorizontal();

            // KEY
            row.Key = EditorGUILayout.TextField(row.Key, GUILayout.Width(200));

            // VALUES
            for (int c = 0; c < db.Languages.Count; c++)
            {
                EnsureRowSize(row);

                string value = row.Values[c];

                if (string.IsNullOrEmpty(value))
                    GUI.color = Color.red;

                row.Values[c] = EditorGUILayout.TextArea(
                    value,
                    GUILayout.Width(200),
                    GUILayout.Height(40));

                GUI.color = Color.white;
            }

            // DELETE
            if (GUILayout.Button("X", GUILayout.Width(25)))
            {
                db.Rows.RemoveAt(r);
                break;
            }

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();
    }

    private void DrawHeader()
    {
        EditorGUILayout.BeginHorizontal();

        GUILayout.Label("Key", EditorStyles.boldLabel, GUILayout.Width(200));

        foreach (var lang in db.Languages)
        {
            GUILayout.Label(lang.ToString(), EditorStyles.boldLabel, GUILayout.Width(200));
        }

        EditorGUILayout.EndHorizontal();
    }

    // =========================
    // UTILITIES
    // =========================

    private void AddRow()
    {
        db.Rows.Add(new LocalizationRow
        {
            Key = GenerateKey(),
            Values = new string[db.Languages.Count]
        });
    }

    private void AddLanguage()
    {
        db.Languages.Add(SystemLanguage.English);

        foreach (var row in db.Rows)
        {
            EnsureRowSize(row);
            Array.Resize(ref row.Values, db.Languages.Count);
        }
    }

    private void EnsureRowSize(LocalizationRow row)
    {
        if (row.Values == null || row.Values.Length != db.Languages.Count)
        {
            Array.Resize(ref row.Values, db.Languages.Count);
        }
    }

    private bool PassesSearch(LocalizationRow row)
    {
        if (string.IsNullOrEmpty(search))
            return true;

        return row.Key != null && row.Key.ToLower().Contains(search.ToLower());
    }

    private bool RowHasMissing(LocalizationRow row)
    {
        EnsureRowSize(row);

        foreach (var val in row.Values)
        {
            if (string.IsNullOrEmpty(val))
                return true;
        }

        return false;
    }

    private string GenerateKey()
    {
        int index = db.Rows.Count + 1;
        return $"KEY_{index:0000}";
    }

    // =========================
    // VALIDATION
    // =========================
    private void Validate()
    {
        var set = new HashSet<string>();

        foreach (var row in db.Rows)
        {
            if (string.IsNullOrEmpty(row.Key))
            {
                Debug.LogError("Empty key detected!", db);
                continue;
            }

            if (!set.Add(row.Key))
            {
                Debug.LogError($"Duplicate key: {row.Key}", db);
            }
        }

        Debug.Log("Localization validation complete.");
    }

    // =========================
    // CSV EXPORT
    // =========================
    private void ExportCSV()
    {
        var path = EditorUtility.SaveFilePanel("Export CSV", "", "Localization.csv", "csv");

        if (string.IsNullOrEmpty(path)) return;

        List<string> lines = new List<string>();

        // Header
        string header = "Key";
        foreach (var lang in db.Languages)
            header += "," + lang;

        lines.Add(header);

        // Rows
        foreach (var row in db.Rows)
        {
            EnsureRowSize(row);

            string line = row.Key;

            for (int i = 0; i < db.Languages.Count; i++)
            {
                string val = row.Values[i] ?? "";
                val = val.Replace("\n", "\\n").Replace(",", ";");

                line += "," + val;
            }

            lines.Add(line);
        }

        File.WriteAllLines(path, lines);
        Debug.Log("CSV Exported.");
    }

    // =========================
    // CSV IMPORT
    // =========================
    private void ImportCSV()
    {
        var path = EditorUtility.OpenFilePanel("Import CSV", "", "csv");

        if (string.IsNullOrEmpty(path)) return;

        var lines = File.ReadAllLines(path);

        if (lines.Length <= 1)
        {
            Debug.LogWarning("CSV is empty.");
            return;
        }

        db.Rows.Clear();

        for (int i = 1; i < lines.Length; i++)
        {
            var split = lines[i].Split(',');

            if (split.Length == 0)
                continue;

            var row = new LocalizationRow
            {
                Key = split[0],
                Values = new string[db.Languages.Count]
            };

            for (int j = 0; j < db.Languages.Count; j++)
            {
                if (j + 1 < split.Length)
                {
                    row.Values[j] = split[j + 1].Replace("\\n", "\n");
                }
            }

            db.Rows.Add(row);
        }

        Debug.Log("CSV Imported.");
    }
}
#endif