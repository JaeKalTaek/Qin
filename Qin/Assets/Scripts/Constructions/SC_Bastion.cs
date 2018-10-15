using UnityEngine;

public class SC_Bastion : SC_Construction {

    protected override void Start() {

        base.Start();

        tileManager.UpdateWallGraph(gameObject);

        tileManager.UpdateNeighborWallGraph(Tile);

    }

    public override void DestroyConstruction () {

		gameObject.SetActive (false);

		base.DestroyConstruction ();

        tileManager.UpdateNeighborWallGraph (Tile);

	}

}
