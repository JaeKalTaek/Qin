public class SC_Bastion : SC_Construction {

    protected override void Start() {

        base.Start();

        gameManager.UpdateWallGraph(gameObject);

        gameManager.UpdateNeighborWallGraph(tileManager.GetTileAt(gameObject));

    }

    public override void DestroyConstruction () {

		gameObject.SetActive (false);

		base.DestroyConstruction ();

		gameManager.UpdateNeighborWallGraph (tileManager.GetTileAt (gameObject));

	}

}
