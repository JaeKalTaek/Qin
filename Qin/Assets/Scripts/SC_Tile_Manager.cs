using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;

public class SC_Tile_Manager : NetworkBehaviour {

	[SyncVar]
	public int xSize, ySize;

	public SC_Tile[,] tiles;

	void Start() {

		tiles = new SC_Tile[xSize, ySize];

		foreach (SC_Tile t in GameObject.FindObjectsOfType<SC_Tile>())
			tiles [(int)t.transform.position.x, (int)t.transform.position.y] = t;

	}

	public void SetHero(SC_Hero hero) {

		SC_Tile t =	SetCharacter (hero);

		t.constructable = false;

	}

	public SC_Tile SetCharacter(SC_Character character) {

		SC_Tile t = GetTileAt (character.gameObject);

		t.movementCost = 5000;
		t.canSetOn = false;

		//if (GetAt<SC_Construction> (t) == null)
		t.attackable = (character.coalition != SC_GameManager.GetInstance ().CoalitionTurn ());			

		return t;

	}

	public void SetQin(SC_Qin qin) {

		SC_Tile t = GetTileAt (qin.gameObject);

		t.constructable = false;
		t.movementCost = 5000;
		t.canSetOn = false;
		t.attackable = SC_GameManager.GetInstance ().CoalitionTurn ();

	}

	public SC_Tile GetTileAt(GameObject g) {

		return tiles [(int)g.transform.position.x, (int)g.transform.position.y];

	}

	public T GetAt<T>(SC_Tile tile) where T : MonoBehaviour {

		T objectToReturn = null;

		foreach (T t in FindObjectsOfType<T>()) {

			if ((t.transform.position.x == (int)tile.transform.position.x) && (t.transform.position.y == (int)tile.transform.position.y))
				objectToReturn = t;

		}

		return objectToReturn;

	}

}
