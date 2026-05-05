#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;
using System;

[CustomEditor(typeof(LocDB))]
public class LocalizationDatabaseEditor : OdinEditor
{
    private string search = "";

    public override void OnInspectorGUI()
    {
        var db = (LocDB)target;

        DrawLanguages(db);
        GUILayout.Space(10);

        DrawToolbar(db);
        GUILayout.Space(5);

        DrawTable(db);

        if (GUI.changed)
            EditorUtility.SetDirty(db);
    }

    private void DrawLanguages(LocDB db)
    {
        EditorGUILayout.LabelField("Languages", EditorStyles.boldLabel);

        for (int i = 0; i < db.Languages.Count; i++)
        {
            db.Languages[i] = (SystemLanguage)EditorGUILayout.EnumPopup(db.Languages[i]);
        }

        if (GUILayout.Button("Add Language"))
        {
            db.Languages.Add(SystemLanguage.English);

            foreach (var row in db.Rows)
                Array.Resize(ref row.Values, db.Languages.Count);
        }
    }

    private void DrawToolbar(LocDB db)
    {
        EditorGUILayout.BeginHorizontal();

        search = EditorGUILayout.TextField("Search", search);

        if (GUILayout.Button("Validate Keys"))
            ValidateKeys(db);

        if (GUILayout.Button("Export CSV"))
            ExportCSV(db);

        if (GUILayout.Button("Import CSV"))
            ImportCSV(db);

        EditorGUILayout.EndHorizontal();
    }

    private void DrawTable(LocDB db)
    {
        EditorGUILayout.BeginHorizontal();

        GUILayout.Label("Key", GUILayout.Width(200));

        foreach (var lang in db.Languages)
            GUILayout.Label(lang.ToString(), GUILayout.Width(150));

        EditorGUILayout.EndHorizontal();

        for (int r = 0; r < db.Rows.Count; r++)
        {
            var row = db.Rows[r];

            if (!string.IsNullOrEmpty(search) && !row.Key.ToLower().Contains(search.ToLower()))
                continue;

            EditorGUILayout.BeginHorizontal();

            row.Key = EditorGUILayout.TextField(row.Key, GUILayout.Width(200));

            for (int c = 0; c < db.Languages.Count; c++)
            {
                if (row.Values == null || row.Values.Length != db.Languages.Count)
                    Array.Resize(ref row.Values, db.Languages.Count);

                if (string.IsNullOrEmpty(row.Values[c]))
                    GUI.color = Color.red;

                row.Values[c] = EditorGUILayout.TextArea(row.Values[c], GUILayout.Width(150), GUILayout.Height(40));

                GUI.color = Color.white;
            }

            if (GUILayout.Button("X", GUILayout.Width(20)))
            {
                db.Rows.RemoveAt(r);
                break;
            }

            EditorGUILayout.EndHorizontal();
        }

        if (GUILayout.Button("Add Row"))
        {
            db.Rows.Add(new LocalizationRow
            {
                Key = "NEW_KEY",
                Values = new string[db.Languages.Count]
            });
        }
    }

    // --- Utilities ---

    private void ValidateKeys(LocDB db)
    {
        var set = new System.Collections.Generic.HashSet<string>();

        foreach (var row in db.Rows)
        {
            if (!set.Add(row.Key))
                Debug.LogError($"Duplicate key: {row.Key}", db);
        }

        Debug.Log("Validation complete.");
    }

    private void ExportCSV(LocDB db)
    {
        var path = EditorUtility.SaveFilePanel("Export CSV", "", "Localization.csv", "csv");

        if (string.IsNullOrEmpty(path)) return;

        var lines = new System.Collections.Generic.List<string>();

        string header = "Key";
        foreach (var lang in db.Languages)
            header += "," + lang;

        lines.Add(header);

        foreach (var row in db.Rows)
        {
            string line = row.Key;

            for (int i = 0; i < db.Languages.Count; i++)
            {
                var val = (row.Values != null && i < row.Values.Length)
                    ? row.Values[i]
                    : "";

                val = val.Replace("\n", "\\n").Replace(",", ";");
                line += "," + val;
            }

            lines.Add(line);
        }

        System.IO.File.WriteAllLines(path, lines);
        Debug.Log("CSV Exported.");
    }

    private void ImportCSV(LocDB db)
    {
        var path = EditorUtility.OpenFilePanel("Import CSV", "", "csv");

        if (string.IsNullOrEmpty(path)) return;

        var lines = System.IO.File.ReadAllLines(path);

        db.Rows.Clear();

        for (int i = 1; i < lines.Length; i++)
        {
            var split = lines[i].Split(',');

            var row = new LocalizationRow();
            row.Key = split[0];

            row.Values = new string[db.Languages.Count];

            for (int j = 0; j < db.Languages.Count; j++)
            {
                if (j + 1 < split.Length)
                    row.Values[j] = split[j + 1].Replace("\\n", "\n");
            }

            db.Rows.Add(row);
        }

        Debug.Log("CSV Imported.");
    }
}
#endif