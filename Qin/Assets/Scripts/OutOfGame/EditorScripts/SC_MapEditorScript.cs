using UnityEngine;
using System.Collections;

public class SC_MapEditorScript : MonoBehaviour {

    public int SizeMapX, SizeMapY;
    public GameObject editorTilePrefab;

    public void GenerateMap() {

		for (int x = 0; x < SizeMapX; x++) { 

            for (int y = 0; y < SizeMapY; y++) {

                GameObject go = Instantiate(editorTilePrefab, new Vector3(x, y, 0), editorTilePrefab.transform.rotation);
				go.transform.parent = GameObject.Find ("Empty_Map_Prefab").transform;

            }    
                
        }

    }

}