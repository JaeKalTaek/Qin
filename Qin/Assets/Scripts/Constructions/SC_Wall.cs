using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_Wall : SC_Bastion {
	
	protected override void OnMouseDown() {

		base.OnMouseDown ();

		SC_Tile under = tileManager.GetTileAt (gameObject); //SC_GameManager.GetInstance().GetTileAt((int)transform.position.x, (int)transform.position.y);

        if ( under.displayConstructable && ( ((SC_Qin.GetEnergy() - 100) > 0) || SC_GameManager.GetInstance().IsBastion() ) ) {

			SC_GameManager.GetInstance().ConstructAt(under);

            SC_GameManager.GetInstance().StopConstruction();

        }

    }

}
