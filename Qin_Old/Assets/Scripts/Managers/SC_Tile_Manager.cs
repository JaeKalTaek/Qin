using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using static SC_Enums;

public class SC_Tile_Manager : NetworkBehaviour {

	public GameObject baseMapPrefab;
	public GameObject plainPrefab, ForestPrefab, MountainPrefab, palacePrefab;

	[SyncVar]
	public int xSize, ySize;

	public SC_Tile[,] tiles;

	static SC_Game_Manager gameManager;

    public static SC_Tile_Manager Instance { get; set; }

    private void Awake() {

        Instance = this;

    }

    void Start () {

        if(!gameManager)
            gameManager = SC_Game_Manager.Instance;

        FindObjectOfType<SC_Camera>().Setup(xSize, ySize);

        tiles = new SC_Tile[xSize, ySize];

		foreach (SC_Tile t in FindObjectsOfType<SC_Tile>())
			tiles [(int)t.transform.position.x, (int)t.transform.position.y] = t;

		gameManager.FinishSetup ();

		if (gameManager.Player)
			gameManager.Player.SetTileManager (this);

	}

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

		if (x >= 1)
			neighbors.Add(tiles[x - 1, y]);

		if ((x + 1) < tiles.GetLength(0))
			neighbors.Add(tiles[x + 1, y]);

		if (y >= 1)
			neighbors.Add(tiles[x, y - 1]);

		if ((y + 1) < tiles.GetLength(1))
			neighbors.Add(tiles[x, y + 1]);

		return neighbors;

	}

    public bool IsNeighbor(SC_Tile pos, SC_Tile target) {

        bool neighbor = false;

        foreach(SC_Tile tile in GetNeighbors(pos))
            if(tile.transform.position == target.transform.position)
                neighbor = true;

        return neighbor;

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

    public void RemoveAllFilters() {

        foreach(SC_Tile tile in tiles)
            tile.RemoveFilter();

    }

}
