using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_Workshop : SC_Construction {

	public static Vector2[] spawnPositions = {

		new Vector2 (16, 3),
		new Vector2 (17, 6),
		new Vector2 (18, 1),
		new Vector2 (16, 9),
		new Vector2 (17, 12),
		new Vector2 (18, 14)

	};

    protected override void OnMouseDown() {

		SC_Tile under = tileManager.GetTileAt (gameObject); //SC_GameManager.GetInstance().GetTileAt((int)transform.position.x, (int)transform.position.y);

		if (under.displayMovement) {
			SC_Character.GetCharacterToMove ().MoveTo ((int)transform.position.x, (int)transform.position.y);
			DestroyConstruction ();
		}

        SC_GameManager.GetInstance().currentWorkshop = this;

        SC_GameManager.GetInstance().DisplayWorkshopPanel();

    }

}
