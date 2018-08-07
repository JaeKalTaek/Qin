using UnityEngine;

public class SC_EditorTile : MonoBehaviour {
	
    public float height;
    public tileType tileType;
	public GameObject heroPrefab;
	public bool spawnSoldier;
	public bool qin;
	public constructionType construction;
	[Header("NE PAS TOUCHER A CES PREFAB")]
	public GameObject soldierPrefab;
	public GameObject qinPrefab;
	public GameObject villagePrefab;
	public GameObject workshopPrefab;
    public GameObject bastionPrefab;

    public static Material GetMaterialByName(string name) {

        return Resources.Load<Material>("Materials/Tiles/M_" + name);

    }

}

public enum tileType {

    Plain, Forest, Mountain, Palace

}

public enum constructionType {

	None, Village, Workshop, Bastion

}