using UnityEngine;

public class SC_EditorTile : MonoBehaviour {
	
    public float height;
    public TileType tileType;
	public GameObject heroPrefab;
	public bool spawnSoldier;
	public bool qin;
	public ConstructionType construction;
    public bool ruin;
	[Header("NE PAS TOUCHER A CES PREFAB")]
	public GameObject soldierPrefab;
	public GameObject qinPrefab;
	public GameObject villagePrefab;
	public GameObject workshopPrefab;
    public GameObject bastionPrefab;

    public static Material GetMaterialByName(string name) {

        return Resources.Load<Material>("Materials/M_" + name);

    }

}

public enum TileType {

    Plain, Forest, Mountain, Palace

}

public enum ConstructionType {

	None, Village, Workshop, Bastion

}