using UnityEngine;
using UnityEngine.Networking;
using static SC_Global;

public class SC_Ruin : NetworkBehaviour {

    [Tooltip("Combat modifiers for this ruin")]
    public CombatModifiers combatModifers;

    void Start () {

        SC_Tile_Manager.Instance.GetTileAt(gameObject).Ruin = this;

    }

    public void DestroyRuin () {

        SC_Tile_Manager.Instance.GetTileAt(gameObject).Ruin = null;

        Destroy(gameObject);

    }

}
