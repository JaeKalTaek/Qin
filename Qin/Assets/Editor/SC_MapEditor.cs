using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SC_MapEditorScript))]
public class SC_MapEditor : Editor {

    bool generated;

    public override void OnInspectorGUI() {

        DrawDefaultInspector();

        generated = ((SC_MapEditorScript)target).transform.childCount > 0;

        if (GameObject.Find(target.name)) {

            if (!generated && GUILayout.Button("Generate map")) {

                generated = true;

                ((SC_MapEditorScript)target).GenerateMap();

            }

            if (!SC_EditorTile.currentQinTile)
                EditorGUILayout.HelpBox("Qin is missing from the map", MessageType.Warning);

            if(SC_EditorTile.heroesOnTiles.Count < 6)
                EditorGUILayout.HelpBox("Not all heroes are on this map", MessageType.Warning);

        }        

    }

}