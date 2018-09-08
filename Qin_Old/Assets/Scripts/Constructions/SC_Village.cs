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

	/*public void OnMouseDown() {

		tileManager.TryToMoveCharacter(gameObject);

	}*/

	public override void DestroyConstruction() {

		base.DestroyConstruction ();

		number--;

		//gameManager.SpawnConvoy (transform.position + new Vector3 (-1, 0, 0));

	}

}
