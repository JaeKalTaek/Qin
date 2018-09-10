using UnityEngine;

public class SC_Soldier : SC_Character {

	static Vector2[] spawnPositions = {
		
		new Vector2 (28, 8),
		new Vector2 (28, 6),
		new Vector2 (28, 4),
		new Vector2 (28, 10),
		new Vector2 (25, 6),
		new Vector2 (22, 8),
		new Vector2 (23, 6),
		new Vector2 (23, 10),
		new Vector2 (21, 4),
		new Vector2 (27, 10),
		new Vector2 (26, 10),
		new Vector2 (24, 7),
		new Vector2 (25, 5),
		new Vector2 (22, 12),
		new Vector2 (23, 13),
		new Vector2 (23, 9),
		new Vector2 (21, 7),
		new Vector2 (27, 4),
		new Vector2 (26, 5),
		new Vector2 (24, 11),
		new Vector2 (23, 2),
		new Vector2 (25, 1),
		new Vector2 (26, 4),
		new Vector2 (24, 3)

	};

    public SC_Weapon weapon;

	[HideInInspector]
	public bool curse1;

    protected override void Awake() {

		base.Awake ();
		
        coalition = false;

    }

    public static Vector2[] GetSpawnPositions() {

        return spawnPositions;

    }

	/*protected override void OnMouseDown () {

		if(gameManager.player.IsQin())
			base.OnMouseDown ();

	}*/

	protected override void PrintMovements () {

		if (CanMove) {

			uiManager.SetButtonActivated ("construct", true);
            uiManager.SetButtonActivated("sacrifice", true);
            uiManager.SetButtonActivated("qinPower", true);
            uiManager.workshopPanel.gameObject.SetActive (false);

            SC_Player.localPlayer.CmdCheckMovements((int)transform.position.x, (int)transform.position.y);

		}

	}

	public override bool Hit(int damages, bool saving) {

		base.Hit(damages, saving);

        if (Health <= 0)
            DestroyCharacter();
        else
            UpdateHealth();

        return (Health <= 0);

	}

    public override void DestroyCharacter() {

        base.DestroyCharacter();

        if(isServer)
		    SC_Player.localPlayer.CmdDestroyGameObject (gameObject);

    }

}
