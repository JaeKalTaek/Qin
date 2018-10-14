using UnityEngine;

public class SC_Bastion : SC_Construction {

    protected override void Start() {

        base.Start();

        tileManager.UpdateWallGraph(gameObject);

        tileManager.UpdateNeighborWallGraph(tileManager.GetTileAt(gameObject));

    }

    public override void DestroyConstruction () {

		gameObject.SetActive (false);

		base.DestroyConstruction ();

        tileManager.UpdateNeighborWallGraph (tileManager.GetTileAt (gameObject));

	}

}
