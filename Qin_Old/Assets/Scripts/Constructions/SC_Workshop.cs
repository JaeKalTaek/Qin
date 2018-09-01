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

	public void OnMouseDown() {

		if (tileManager.TryToMoveCharacter (gameObject))
			DestroyConstruction ();

		gameManager.currentWorkshop = this;

		gameManager.DisplayWorkshopPanel();

	}

}
