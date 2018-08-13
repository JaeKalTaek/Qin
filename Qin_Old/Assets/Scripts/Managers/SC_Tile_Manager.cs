using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class SC_Tile_Manager : NetworkBehaviour {

	public GameObject baseMapPrefab;
	public GameObject plainPrefab, ForestPrefab, MountainPrefab, palacePrefab;

	[SyncVar]
	public int xSize, ySize;

	public SC_Tile[,] tiles;

	SC_GameManager gameManager;

	void Start () {

		gameManager = FindObjectOfType<SC_GameManager> ();

		tiles = new SC_Tile[xSize, ySize];

		foreach (SC_Tile t in FindObjectsOfType<SC_Tile>())
			tiles [(int)t.transform.position.x, (int)t.transform.position.y] = t;

		gameManager.FinishSetup ();

		if (gameManager.player)
			gameManager.player.SetTileManager (this);

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

	public List<SC_Tile> GetRange(GameObject center) {

		List<SC_Tile> range = new List<SC_Tile> ();

		int x = (int)center.transform.position.x;
		int y = (int)center.transform.position.y;

		for (int i = (x - 2); i <= (x + 2); i++) {

			for (int j = (y - 2); j <= (y + 2); j++) {

				if ((i >= 0) && (i < xSize) && (j >= 0) && (j < ySize)) {

					bool validTile = true;

					if ( ( (i == (x - 2)) || (i == (x + 2)) ) && (j != y))	validTile = false;
					if ( ( (j == (y - 2)) || (j == (y + 2)) ) && (i != x))	validTile = false;
					if ( ( (i == (x - 1)) || (i == (x + 1)) ) && ( (j < (y - 1)) || (j > (y + 1)) ) ) validTile = false;
					if ( ( (j == (y - 1)) || (j == (y + 1)) ) && ( (i < (x - 1)) || (i > (x + 1)) ) ) validTile = false;

					if (validTile) range.Add (tiles [i, j]);

				}

			}

		}

		return range;

	}

	public List<SC_Tile> GetNeighbors(SC_Tile tileParam) {

		List<SC_Tile> neighbors = new List<SC_Tile>();
		int x = (int)tileParam.transform.position.x;
		int y = (int)tileParam.transform.position.y;

		if ((x - 1) >= 0)
			neighbors.Add(tiles[x - 1, y]);

		if ((x + 1) < tiles.GetLength(0))
			neighbors.Add(tiles[x + 1, y]);

		if ((y - 1) >= 0)
			neighbors.Add(tiles[x, y - 1]);

		if ((y + 1) < tiles.GetLength(1))
			neighbors.Add(tiles[x, y + 1]);

		return neighbors;

	}

	public bool TryToMoveCharacter(GameObject target) {

		SC_Tile tile = GetTileAt (target);

		if (tile.displayMovement) {

            SC_Player.localPlayer.CmdMoveCharacterTo((int)tile.transform.position.x, (int)tile.transform.position.y);

			return true;

		} else {

			return false;

		} 

	}

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

			if ((t.transform.position.x == (int)tile.transform.position.x) && (t.transform.position.y == (int)tile.transform.position.y) && (t.GetType() != typeof(SC_Tile)))
				objectToReturn = t;

		}

		return objectToReturn;

	}

    public int[][] GetArraysFromArray<T>(T[,] arrayP) where T : MonoBehaviour {

        int[][] array = new int[2][];

        array[0] = new int[0];
        array[1] = new int[0];

        int i = 0;

        foreach(MonoBehaviour m in arrayP) {

            array[0][i] = (int)m.gameObject.transform.position.x;
            array[1][i] = (int)m.gameObject.transform.position.y;

            i++;

        }

        return array;

    }

    public int[][] GetArraysFromList<T>(List<T> list) where T : MonoBehaviour {

        int[][] array = new int[2][];

        array[0] = new int[list.Count];
        array[1] = new int[list.Count];

        int i = 0;

        foreach (MonoBehaviour m in list) {

            array[0][i] = (int)m.gameObject.transform.position.x;
            array[1][i] = (int)m.gameObject.transform.position.y;

            i++;

        }

        return array;

    }

}
