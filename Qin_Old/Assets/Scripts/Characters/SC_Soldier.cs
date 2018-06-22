using System;
using System.Collections;
using System.Collections.Generic;
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

	protected override void Start () {
		
		base.Start ();

		tileManager.SetCharacter (this);

	}

    public static Vector2[] GetSpawnPositions() {

        return spawnPositions;

    }

	protected override void OnMouseDown () {

		if(gameManager.player.IsQin())
			base.OnMouseDown ();

	}

	protected override void PrintMovements () {

		if (canMove) {

			uiManager.ToggleButton ("construct");

			uiManager.workshopPanel.gameObject.SetActive (false);

			gameManager.CheckMovements (this);

		}

	}

	public override bool Hit(int damages, bool saving) {

		base.Hit(damages, saving);
        
		if (health <= 0) {

			DestroyCharacter (); 

		} else {

			lifebar.UpdateGraph (health, maxHealth);
			uiManager.UpdateCharacterHealth (gameObject);

		}

        return (health <= 0);

	}

    public override void DestroyCharacter() {

        base.DestroyCharacter();

        Destroy(gameObject);

    }

}
