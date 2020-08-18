using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ButtDataEditorWindow : EditorWindow
{
    public List<int> listOfInts = new List<int>{1,3,7,4};
    public static void Open(ButtData buttData)
    {
        ButtDataEditorWindow window = GetWindow<ButtDataEditorWindow>("Butt Data Editor");
    }
}
