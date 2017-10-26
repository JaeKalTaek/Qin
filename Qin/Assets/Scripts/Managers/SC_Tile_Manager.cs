using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;

public class SC_Tile_Manager : NetworkBehaviour {

	public GameObject baseMapPrefab;
	public GameObject PlainPrefab, ForestPrefab, MountainPrefab, palacePrefab;

	[SyncVar]
	public int xSize, ySize;

	public SC_Tile[,] tiles;

	SC_GameManager gameManager;

	void Start () {

		gameManager = GameObject.FindObjectOfType<SC_GameManager> ();

		tiles = new SC_Tile[xSize, ySize];

		foreach (SC_Tile t in GameObject.FindObjectsOfType<SC_Tile>())
			tiles [(int)t.transform.position.x, (int)t.transform.position.y] = t;

		gameManager.FinishSetup ();

	}

	#region Set Methods
	public void SetHero(SC_Hero hero) {

		SC_Tile t =	SetCharacter (hero);

		t.constructable = false;

	}

	public SC_Tile SetCharacter(SC_Character character) {

		SC_Tile t = GetTileAt (character.gameObject);

		t.movementCost = 5000;
		t.canSetOn = false;

		//if (GetAt<SC_Construction> (t) == null)
		t.attackable = (character.coalition != gameManager.CoalitionTurn());			

		return t;

	}

	public void SetQin(SC_Qin qin) {

		SC_Tile t = GetTileAt (qin.gameObject);

		t.constructable = false;
		t.movementCost = 10000;
		t.canSetOn = false;
		t.attackable = gameManager.CoalitionTurn();

	}

	public void SetConstruction(SC_Construction construction) {

		SC_Tile t = GetTileAt (construction.gameObject);

		t.movementCost = 10000;
		t.attackable = (construction.GetType ().Equals (typeof(SC_Village)) || construction.GetType ().Equals (typeof(SC_Village))) ? false : gameManager.CoalitionTurn();

		if (!construction.GetType ().Equals (typeof(SC_Wall)))
			t.constructable = false;

		if (!construction.GetType ().Equals (typeof(SC_Village)))
			t.canSetOn = false;

	}
	#endregion

	public SC_Tile GetTileAt(GameObject g) {

		return tiles [(int)g.transform.position.x, (int)g.transform.position.y];

	}

	public SC_Tile GetTileAt(int x, int y) {

		return tiles [x, y];

	}

	public SC_Tile GetTileAt(Vector3 pos) {

		return tiles [(int)pos.x, (int)pos.y];

	}

	public T GetAt<T>(int x, int y) where T : MonoBehaviour {

		return GetAt<T> (tiles [x, y]);

	}

	public T GetAt<T>(GameObject g) where T : MonoBehaviour {

		return GetAt<T> (GetTileAt (g));

	}

	public T GetAt<T>(SC_Tile tile) where T : MonoBehaviour {

		T objectToReturn = null;

		foreach (T t in FindObjectsOfType<T>()) {

			if ((t.transform.position.x == (int)tile.transform.position.x) && (t.transform.position.y == (int)tile.transform.position.y) && (!t.Equals(tile)))
				objectToReturn = t;

		}

		return objectToReturn;

	}

}
