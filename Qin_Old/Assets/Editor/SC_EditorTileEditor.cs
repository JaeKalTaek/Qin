using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SC_EditorTile))]
public class SC_EditorTileEditor : Editor {

	public override void OnInspectorGUI() {

        DrawDefaultInspector();

        SC_EditorTile myScript = (SC_EditorTile)target;

        Selection.activeGameObject.GetComponent<Renderer>().material = SC_EditorTile.GetMaterialByName(Selection.activeGameObject.GetComponent<SC_EditorTile>().tileType.ToString());

        Selection.activeTransform.position = new Vector3(Selection.activeTransform.position.x, Selection.activeTransform.position.y, Selection.activeGameObject.GetComponent<SC_EditorTile>().height);
			
		if (Selection.activeGameObject.GetComponent<SC_EditorTile> ().spawnSoldier) {

			Selection.activeGameObject.GetComponent<SC_EditorTile> ().qin = false;
			Selection.activeGameObject.GetComponent<SC_EditorTile> ().heroPrefab = null;
			Selection.activeGameObject.GetComponent<SC_EditorTile> ().construction = constructionType.None;

			if (Selection.activeTransform.Find ("P_Soldier") == null) {

				GameObject go = Instantiate (Selection.activeGameObject.GetComponent<SC_EditorTile> ().soldierPrefab, Selection.activeTransform);
				go.transform.position = Selection.activeTransform.position + new Vector3 (0, 0, -1);
				go.name = "P_Soldier";

			}

		} else if (Selection.activeTransform.Find ("P_Soldier") != null) {
			
				DestroyImmediate (Selection.activeTransform.Find ("P_Soldier").gameObject);

		}

		if (Selection.activeGameObject.GetComponent<SC_EditorTile> ().qin) {

			Selection.activeGameObject.GetComponent<SC_EditorTile> ().spawnSoldier = false;
			Selection.activeGameObject.GetComponent<SC_EditorTile> ().heroPrefab = null;
			Selection.activeGameObject.GetComponent<SC_EditorTile> ().construction = constructionType.None;

			if (Selection.activeTransform.Find ("P_Qin") == null) {

				GameObject go = Instantiate (Selection.activeGameObject.GetComponent<SC_EditorTile> ().qinPrefab, Selection.activeTransform);
				go.transform.position = Selection.activeTransform.position + new Vector3 (0, 0, -1);
				go.name = "P_Qin";

			}

		} else if (Selection.activeTransform.Find ("P_Qin") != null) {

			DestroyImmediate (Selection.activeTransform.Find ("P_Qin").gameObject);

		}

		if (Selection.activeGameObject.GetComponent<SC_EditorTile> ().heroPrefab != null) {

			Selection.activeGameObject.GetComponent<SC_EditorTile> ().qin = false;
			Selection.activeGameObject.GetComponent<SC_EditorTile> ().spawnSoldier = false;
			Selection.activeGameObject.GetComponent<SC_EditorTile> ().construction = constructionType.None;

			if (Selection.activeTransform.Find ("P_Hero") == null) {
				
				GameObject go = Instantiate (Selection.activeGameObject.GetComponent<SC_EditorTile> ().heroPrefab, Selection.activeTransform);
				go.transform.position = Selection.activeTransform.position + new Vector3 (0, 0, -1);
				go.name = "P_Hero";

			}

		} else if (Selection.activeTransform.Find ("P_Hero") != null) {
			
				DestroyImmediate (Selection.activeTransform.Find ("P_Hero").gameObject);

		}

		if (Selection.activeGameObject.GetComponent<SC_EditorTile> ().construction != constructionType.None) {

			Selection.activeGameObject.GetComponent<SC_EditorTile> ().qin = false;
			Selection.activeGameObject.GetComponent<SC_EditorTile> ().spawnSoldier = false;
			Selection.activeGameObject.GetComponent<SC_EditorTile> ().heroPrefab = null;

			if (Selection.activeTransform.Find ("P_Construction") != null)
				DestroyImmediate (Selection.activeTransform.Find ("P_Construction").gameObject);

			GameObject go;

			if (Selection.activeGameObject.GetComponent<SC_EditorTile> ().construction == constructionType.Village)
				go = Instantiate (Selection.activeGameObject.GetComponent<SC_EditorTile> ().villagePrefab, Selection.activeTransform);
			else if (Selection.activeGameObject.GetComponent<SC_EditorTile>().construction == constructionType.Workshop)
                go = Instantiate (Selection.activeGameObject.GetComponent<SC_EditorTile> ().workshopPrefab, Selection.activeTransform);
            else
                go = Instantiate(Selection.activeGameObject.GetComponent<SC_EditorTile>().bastionPrefab, Selection.activeTransform);

            go.transform.position = Selection.activeTransform.position + new Vector3 (0, 0, -1);
			go.name = "P_Construction";

		} else if (Selection.activeTransform.Find ("P_Construction") != null) {

			DestroyImmediate (Selection.activeTransform.Find ("P_Construction").gameObject);

		}

    }

}
