using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_Bastion : SC_Construction {

	public override void DestroyConstruction () {

		gameObject.SetActive (false);

		base.DestroyConstruction ();

		gameManager.UpdateNeighborWallGraph (tileManager.GetTileAt (gameObject)); //SC_GameManager.GetInstance ().GetTileAt ((int)transform.position.x, (int)transform.position.y));

	}

}
