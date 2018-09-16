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

        if (SC_UI_Manager.CanInteract && !SC_Player.localPlayer.Busy && !tileManager.GetTileAt(gameObject).Character) {

            gameManager.CurrentWorkshop = this;

            uiManager.StartQinAction("workshop");

        }

	}

}
