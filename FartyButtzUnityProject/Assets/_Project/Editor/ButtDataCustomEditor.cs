using System.Collections;
using System.Collections.Generic;
using UnityEditor.Callbacks;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public class AssetHandler
{
    [OnOpenAsset()]
    public static bool OpenEditor(int instanceID, int line)
    {
        ButtData buttData = EditorUtility.InstanceIDToObject(instanceID) as ButtData;
        return false;
    }
}

[CustomEditor(typeof(ButtData))]
public class ButtDataCustomEditor : Editor
{
    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("Open Editor"))
        {
            ButtDataEditorWindow.Open((ButtData)target);
        }
    }
}
