using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(SC_MapEditorScript))]
public class SC_MapEditor : Editor {

    public override void OnInspectorGUI() {

        DrawDefaultInspector();

        SC_MapEditorScript myScript = (SC_MapEditorScript)target;

        if (GUILayout.Button("Generate map"))
            myScript.GenerateMap();

    }

}