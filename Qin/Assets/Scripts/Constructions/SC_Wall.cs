using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_Wall : SC_Construction {
	
	protected override void OnMouseDown() {

		base.OnMouseDown ();

        SC_Tile under = SC_GameManager.GetInstance().GetTileAt((int)transform.position.x, (int)transform.position.y);

        if ( under.canConstruct && ( ((SC_Qin.GetEnergy() - 100) > 0) || SC_GameManager.GetInstance().IsBastion() ) ) {

			SC_GameManager.GetInstance().ConstructAt(under.transform.position);

            SC_GameManager.GetInstance().StopConstruction();

        }

    }

	public override void DestroyConstruction () {

		gameObject.SetActive (false);

		base.DestroyConstruction ();

		SC_GameManager.GetInstance ().UpdateNeighborWallGraph (SC_GameManager.GetInstance ().GetTileAt ((int)transform.position.x, (int)transform.position.y));

	}

}
