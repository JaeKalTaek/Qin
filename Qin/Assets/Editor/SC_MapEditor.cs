using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SC_MapEditorScript))]
public class SC_MapEditor : Editor {

    public override void OnInspectorGUI() {

        DrawDefaultInspector();

        if (GUILayout.Button("Generate map"))
            ((SC_MapEditorScript)target).GenerateMap();

    }

}