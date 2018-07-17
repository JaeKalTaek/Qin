﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[System.Serializable]
public class SC_Tile : NetworkBehaviour {

	public bool displayMovement { get; set; }

	public bool constructable { get; set; }

	bool displayAttack;

	public bool displayConstructable { get; set; }

	public bool displaySacrifice { get; set; }

	public bool displayResurrection { get; set; }

	public int baseCost;

	public int movementCost  { get; set; }

	public bool canSetOn  { get; set; }

	public bool attackable  { get; set; }

	public SC_Tile parent  { get; set; }

	static SC_GameManager gameManager;

	static SC_Tile_Manager tileManager;

	void Awake() {

		constructable = !name.Contains("Palace");

		movementCost = baseCost;

		canSetOn = true;

		attackable = true;

	}

	void Start() {

		if(gameManager == null)
			gameManager = GameObject.FindObjectOfType<SC_GameManager> ();

		if(tileManager == null)
			tileManager = GameObject.FindObjectOfType<SC_Tile_Manager> ();

	}

	void OnMouseDown() {

		if (displayConstructable && (((SC_Qin.GetEnergy () - 50) > 0) || gameManager.IsBastion ())) {

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
					tile.RemoveFilters ();

				gameManager.Attack ();

			}

		} else if (displaySacrifice) {

			SC_Qin.ChangeEnergy (25);

			RemoveFilters ();

			Destroy (tileManager.GetAt<SC_Character>(this));

		} else if (displayResurrection) {

			gameManager.HideResurrectionTiles ();

			SC_Qin.UsePower (transform.position);

		}

	}

	public void SetFilter(string filterName) {

		foreach(SpriteRenderer sprite in GetComponentsInChildren<SpriteRenderer>())
			sprite.enabled = sprite.name.Equals(filterName);

	}

	public void RemoveFilters() {

		displayMovement = false;
		displayConstructable = false;
		displayAttack = false;
		displaySacrifice = false;
		displayResurrection = false;

		foreach (SpriteRenderer sprite in GetComponentsInChildren<SpriteRenderer>())
			sprite.enabled = false;

	}

	public void DisplayMovement() { 
			
		displayMovement = true;

		SetFilter ("T_DisplayMovement");

	}

	public void DisplayAttack() { 

		displayAttack = true;

		SetFilter ("T_DisplayAttack");

	}

	public void DisplayConstructable() { 

		displayConstructable = true;

		SetFilter ("T_CanConstruct");

	}

	public bool Qin() {

		Vector3 qinPos = FindObjectOfType<SC_Qin> ().transform.position;
		return ((transform.position.x == qinPos.x) && (transform.position.y == qinPos.y));

	}

	public bool IsEmpty() {

		return tileManager.GetAt<MonoBehaviour> (this) == null;

	}

	public bool isPalace() {

		return name.Contains("Palace");

	}

	/*public bool GetDisplayConstructable() {

		return displayConstructable;

	}*/

	public bool GetDisplayAttack() {

		return displayAttack;

	}

}