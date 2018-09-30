using UnityEngine.Networking;

public class SC_Ruin : NetworkBehaviour {

    void Start () {

        SC_Tile_Manager.Instance.GetTileAt(gameObject).Ruin = this;

    }

    public void DestroyRuin () {

        SC_Tile_Manager.Instance.GetTileAt(gameObject).Ruin = null;

        Destroy(gameObject);

    }

}
