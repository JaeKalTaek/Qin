﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[System.Serializable]
public class SC_Tile : NetworkBehaviour {

	[HideInInspector]
	public bool displayMovement;
	[HideInInspector]
	public bool constructable;
	bool displayAttack;
	[HideInInspector]
	public bool displayConstructable, displaySacrifice, displayResurrection;
	public int baseCost;
	[HideInInspector]
	public int movementCost;
	[HideInInspector]
	public bool canSetOn;
	[HideInInspector]
	public bool attackable;
	[HideInInspector]
	public SC_Tile parent;

	SC_GameManager gameManager;

	SC_Tile_Manager tileManager;

	void Awake() {

		constructable = !(name.Contains("Palace"));

		movementCost = baseCost;

		canSetOn = true;

		attackable = true;

	}

	void Start() {

		gameManager = GameObject.FindObjectOfType<SC_GameManager> ();

		tileManager = GameObject.FindObjectOfType<SC_Tile_Manager> ();

	}

	void OnMouseDown() {

		if ((displayConstructable) && (((SC_Qin.GetEnergy () - 50) > 0) || gameManager.IsBastion ())) {

			gameManager.ConstructAt (this);

			gameManager.StopConstruction ();

		} else if (displayMovement) {

			gameManager.GetCharacterToMove().MoveTo(this);

		} else if (displayAttack) {

			SC_Character attackingCharacter = SC_Character.GetAttackingCharacter ();
			SC_Tile attackingCharacterTile = tileManager.GetTileAt (attackingCharacter.gameObject);
			gameManager.rangedAttack = !gameManager.IsNeighbor (attackingCharacterTile, this);

			attackingCharacter.attackTarget = this;

			if (attackingCharacter.isHero ()) {

				((SC_Hero)attackingCharacter).ChooseWeapon ();

			} else {

				foreach (SC_Tile tile in tileManager.tiles)
					tile.RemoveFilter ();

				gameManager.Attack ();

			}

		} else if (displaySacrifice) {

			SC_Qin.ChangeEnergy (25);

			RemoveFilter ();

			Destroy (tileManager.GetAt<SC_Character>(this));

		} else if (displayResurrection) {

			gameManager.HideResurrectionTiles ();

			SC_Qin.UsePower (transform.position);

		}

	}

	public void SetFilter(string filterName) {

		foreach(SpriteRenderer sprite in GetComponentsInChildren<SpriteRenderer>())
			if (sprite.name.Equals(filterName)) sprite.enabled = true;

	}

	public void RemoveFilter() {

		displayMovement = false;
		displayConstructable = false;
		displayAttack = false;
		displaySacrifice = false;
		displayResurrection = false;

		foreach (SpriteRenderer sprite in GetComponentsInChildren<SpriteRenderer>())
			sprite.enabled = false;

	}

	public void DisplayMovement(bool valid) { 

		displayMovement = valid;

		/*if(valid) {

			displayMovement = true;
			SetFilter("T_DisplayMovement");

        }*/

	}

	public bool Qin() {

		Vector3 qinPos = FindObjectOfType<SC_Qin> ().transform.position;
		return ((transform.position.x == qinPos.x) && (transform.position.y == qinPos.y));

	}

	public bool IsEmpty() {

		return tileManager.GetAt<MonoBehaviour> (this);

	}

	public bool isPalace() {

		return name.Contains("Palace");

	}

	public bool CanConstructOn() {

		return displayConstructable;

	}

	public void SetCanConstruct(bool c) {

		displayConstructable = c;

	}

	public bool GetDisplayAttack() {

		return displayAttack;

	}

	public void SetDisplayAttack(bool d) {

		displayAttack = d;

	}

}
