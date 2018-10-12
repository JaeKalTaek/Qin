using UnityEngine;

public class SC_MapEditorScript : MonoBehaviour {

    [Header("Editor Map Variables")]
    [Tooltip("Width of the map (in number of tiles)")]
    public int SizeMapX;

    [Tooltip("Height of the map (in number of tiles)")]
    public int SizeMapY;

    [Tooltip("Size of a tile")]
    public float TileSize = .96f;

    public void GenerateMap() {

        for (int x = 0; x < SizeMapX; x++) {

            for (int y = 0; y < SizeMapY; y++) {

                GameObject og = Resources.Load<GameObject>("Prefabs/Tiles/P_EditorTile");

                /*GameObject go =*/ Instantiate(og, new Vector3(x /**TileSize*/, y /**TileSize*/, 0), Quaternion.identity, transform);

                //go.transform.localScale = Vector3.one * TileSize;

            }

        }
    }

    /*[InitializeOnLoad]
    internal class PrefabExtension {

        static PrefabExtension () {

            PrefabUtility.prefabInstanceUpdated += (GameObject instance) => {

                GameObject prefab = (GameObject)PrefabUtility.GetCorrespondingObjectFromSource(instance);

                foreach (Transform t in prefab.transform)
                    print(t.GetComponent<SC_EditorTile>().river + ", " + t.GetComponent<SC_EditorTile>().riverSprite);

            };

        }

    }*/

}