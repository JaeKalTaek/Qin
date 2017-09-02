using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_Village : SC_Construction {

	public static Vector2[] spawnPositions = {

		new Vector2 (9, 3),
		new Vector2 (12, 6),
		new Vector2 (13, 1),
		new Vector2 (10, 9),
		new Vector2 (11, 12),
		new Vector2 (8, 14)

	};

	public static int number;

    protected override void Start() {
		        
		base.Start ();

		number++;

    }

 	protected override void OnMouseDown() {

		SC_Tile under = SC_GameManager.GetInstance().GetTileAt((int)transform.position.x, (int)transform.position.y);

		if (under.displayMovement)
			SC_Character.GetCharacterToMove().MoveTo ((int)transform.position.x, (int)transform.position.y);

	}

	public override void DestroyConstruction() {

		base.DestroyConstruction ();

        number--;

		SC_GameManager.GetInstance ().SpawnConvoy (transform.position + new Vector3 (-1, 0, 0));

	}

}
