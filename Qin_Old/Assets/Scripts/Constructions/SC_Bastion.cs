public class SC_Bastion : SC_Construction {

	public override void DestroyConstruction () {

		gameObject.SetActive (false);

		base.DestroyConstruction ();

		gameManager.UpdateNeighborWallGraph (tileManager.GetTileAt (gameObject));

	}

}
