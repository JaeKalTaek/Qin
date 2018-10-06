using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using static SC_Global;

public class SC_Tile_Manager : NetworkBehaviour {

	[SyncVar]
	public int xSize, ySize;

	public SC_Tile[,] tiles;

	static SC_Game_Manager gameManager;

    static SC_UI_Manager uiManager;

    public static SC_Tile_Manager Instance { get; set; }

    //Variables used to determine the movements possible
    List<SC_Tile> OpenList { get; set; }
    List<SC_Tile> ClosedList { get; set; }
    Dictionary<SC_Tile, int> movementPoints = new Dictionary<SC_Tile, int>();

    public SC_Pump DisplayedPump { get; set; }

    void Awake() {

        Instance = this;

    }

    void Start () {

        gameManager = SC_Game_Manager.Instance;

        uiManager = SC_UI_Manager.Instance;
        uiManager.TileManager = this;

        SC_Fight_Manager.Instance.TileManager = this;

        FindObjectOfType<SC_Camera>().Setup(xSize, ySize);

        tiles = new SC_Tile[xSize, ySize];

		foreach (SC_Tile t in FindObjectsOfType<SC_Tile>())
			tiles [Mathf.RoundToInt(t.transform.position.x), Mathf.RoundToInt(t.transform.position.y)] = t;

		gameManager.FinishSetup ();

		if (gameManager.Player)
			gameManager.Player.SetTileManager (this);

        OpenList = new List<SC_Tile>();
        ClosedList = new List<SC_Tile>();

    }

    #region Utility Functions
    public List<SC_Tile> GetTilesAtDistance(SC_Tile center, int distance) {

        return GetTilesAtDistance(center.transform.position, distance);

    }

    public List<SC_Tile> GetTilesAtDistance(Vector3 center, int distance) {

        List<SC_Tile> returnValue = new List<SC_Tile>();

        foreach(SC_Tile tile in tiles) {

            if (TileDistance(center, tile) == distance)
                returnValue.Add(tile);

        }

        return returnValue;

    }

    public List<SC_Tile> GetRange(Vector3 center, int distance) {

        List<SC_Tile> returnValue = new List<SC_Tile>();

        foreach (SC_Tile tile in tiles) {

            if (TileDistance(center, tile) <= distance)
                returnValue.Add(tile);

        }

        return returnValue;

    }

    public int TileDistance(Vector3 a, SC_Tile b) {

        return TileDistance(a, b.transform.position);

    }

    public int TileDistance (Vector3 a, Vector3 b) {

        return Mathf.Abs(Mathf.RoundToInt(a.x) - Mathf.RoundToInt(b.x)) + Mathf.Abs(Mathf.RoundToInt(a.y) - Mathf.RoundToInt(b.y));

    }

    public SC_Tile GetUnoccupiedNeighbor (SC_Character target) {

        SC_Tile t = null;

        foreach (SC_Tile tile in GetTilesAtDistance(target.transform.position, 1))
            if (tile.Empty)
                t = tile;

        return t;

    }

    public SC_Tile GetTileAt (GameObject g) {

        return GetTileAt(g.transform.position);

    }

    public SC_Tile GetTileAt (int x, int y) {

        return tiles[x, y];

    }

    public SC_Tile GetTileAt (Vector3 pos) {

        return tiles[Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y)];

    }

    public void RemoveAllFilters () {

        foreach (SC_Tile tile in tiles)
            tile.RemoveFilter();

    }
    #endregion

    #region Attack
    public List<SC_Tile> GetAttackTiles () {

        return GetAttackTiles(SC_Character.attackingCharacter, SC_Character.attackingCharacter.transform.position);

    }

    public List<SC_Tile> GetAttackTiles(SC_Character attacker, Vector3 center) {

        List<SC_Tile> attackableTiles = new List<SC_Tile>();

        if (attacker.HasRange) {

            if (attacker.Soldier && attacker.GetActiveWeapon().IsBow)
                attackableTiles = GetTilesAtDistance(center, 2);
            else
                attackableTiles = GetRange(center, 2);

        } else {

            attackableTiles = GetTilesAtDistance(center, 1);

        }

        List<SC_Tile> a = new List<SC_Tile> (attackableTiles);

        foreach (SC_Tile t in a)
            if (!t.Attackable)
                attackableTiles.Remove(t);

        return attackableTiles;

    }

    public void CheckAttack () {

        RemoveAllFilters();

        foreach (SC_Tile tile in GetAttackTiles()) {

            tile.ChangeDisplay(TDisplay.Attack);

            if (tile.CursorOn)
                tile.Hero?.PreviewFight();

        }

    }

    public void PreviewAttack() {

        foreach (SC_Tile t in GetAttackTiles())
            t.SetFilter(TDisplay.PreviewAttack);

    }

    public List<SC_Hero> HeroesInRange (SC_Hero target) {

        List<SC_Hero> heroesInRange = new List<SC_Hero>();

        foreach (SC_Tile tile in GetRange(target.transform.position, 3)) {

            if (tile.Character) {

                if (tile.Character.Hero && !tile.Character.Qin) {

                    if (!tile.Character.characterName.Equals(target.characterName) && !heroesInRange.Contains(tile.Character.Hero))
                        heroesInRange.Add(tile.Character.Hero);

                }

            }

        }

        return heroesInRange;

    }    
    #endregion

    #region Display Movements
    public void CheckMovements (SC_Character target) {

        SC_Character.characterToMove = target;

        RemoveAllFilters();

        SC_Tile tileTarget = GetTileAt(target.gameObject);

        CalcRange(tileTarget, target);

        foreach (SC_Tile tile in new List<SC_Tile>(ClosedList) { tileTarget }) {

            if (tile.CanSetOn || tile == tileTarget) {

                tile.ChangeDisplay(TDisplay.Movement);

                foreach (SC_Tile t in GetAttackTiles(target, tile.transform.position))
                    if (t.CurrentDisplay == TDisplay.None && !ClosedList.Contains(t))
                        t.SetFilter(TDisplay.PreviewAttack);

            }

        }

    }

    void CalcRange (SC_Tile aStartingTile, SC_Character target) {

        OpenList.Clear();
        ClosedList.Clear();

        movementPoints[aStartingTile] = target.Movement;

        ExpandTile(aStartingTile, target.Hero?.Berserk ?? false);

        while (OpenList.Count > 0) {

            OpenList.Sort((a, b) => movementPoints[a].CompareTo(movementPoints[b]));

            SC_Tile tile = OpenList[OpenList.Count - 1];

            OpenList.RemoveAt(OpenList.Count - 1);

            ExpandTile(tile, target.Hero?.Berserk ?? false);

        }

    }

    void ExpandTile (SC_Tile aTile, bool berserk) {

        int parentPoints = movementPoints[aTile];

        ClosedList.Add(aTile);

        foreach (SC_Tile tile in GetTilesAtDistance(aTile, 1)) {

            if (ClosedList.Contains(tile) || OpenList.Contains(tile) || !tile.CanGoThrough)
                continue;

            int points = parentPoints - (berserk ? 1 : tile.cost);

            if (points >= 0) {

                OpenList.Add(tile);

                movementPoints[tile] = points;

            }

        }

    }

    public List<SC_Tile> PathFinder (SC_Tile start, SC_Tile end) {

        List<SC_Tile> openList = new List<SC_Tile>();
        List<SC_Tile> tempList = new List<SC_Tile>();
        List<SC_Tile> closedList = new List<SC_Tile>();

        start.Parent = null;
        openList.Add(start);

        while (!openList.Contains(end)) {

            foreach (SC_Tile tile in openList) {

                foreach (SC_Tile neighbor in GetTilesAtDistance(tile, 1)) {

                    if (!closedList.Contains(neighbor) && ClosedList.Contains(neighbor) && !tempList.Contains(neighbor)) {

                        tempList.Add(neighbor);
                        neighbor.Parent = tile;

                    }

                }

                closedList.Add(tile);

            }

            openList = new List<SC_Tile>(tempList);
            tempList.Clear();

        }

        List<SC_Tile> path = new List<SC_Tile>();
        SC_Tile currentParent = end;

        while (!path.Contains(start)) {

            path.Add(currentParent);
            currentParent = currentParent.Parent;

        }

        foreach (SC_Tile tile in tiles)
            tile.Parent = null;

        path.Reverse();

        return (path.Count > 1) ? path : null;

    }
    #endregion

    #region Construction
    public List<SC_Tile> GetConstructableTiles(bool wall) {

        List<SC_Tile> constructableTiles = new List<SC_Tile>();

        if (wall) {

            foreach (SC_Construction construction in FindObjectsOfType<SC_Construction>()) {

                if (construction.GreatWall) {

                    foreach (SC_Tile neighbor in GetTilesAtDistance(construction.transform.position, 1))
                        if (neighbor.Constructable && !constructableTiles.Contains(neighbor))
                            constructableTiles.Add(neighbor);

                }

            }

        } else {

            foreach (SC_Tile tile in tiles)
                if (tile.Constructable)
                    constructableTiles.Add(tile);

        }

        return constructableTiles;

    }

    public void DisplayConstructableTiles (bool wall) {

        foreach (SC_Tile tile in GetConstructableTiles(wall))
            tile.GetComponent<SC_Tile>().ChangeDisplay(TDisplay.Construct);

    }

    public void UpdateNeighborWallGraph (SC_Tile center) {

        foreach (SC_Tile tile in GetTilesAtDistance(center, 1))
            if (tile.Bastion)
                UpdateWallGraph(tile.Bastion.gameObject);

    }

    public void UpdateWallGraph (GameObject go) {

        SC_Construction construction = go.GetComponent<SC_Construction>();

        SC_Tile under = GetTileAt(go);

        bool left = false;
        bool right = false;
        bool top = false;
        int count = 0;

        foreach (SC_Tile tile in GetTilesAtDistance(under, 1)) {

            if (tile.Bastion) {

                if (tile.transform.position.x < under.transform.position.x)
                    left = true;
                else if (tile.transform.position.x > under.transform.position.x)
                    right = true;
                else if (tile.transform.position.y > under.transform.position.y)
                    top = true;

                count++;

            }

        }

        string rotation = "";

        if (count == 1)
            rotation = right ? "Right" : left ? "Left" : top ? "Top" : "Bottom";
        else if (count == 2)
            rotation = right ? (left ? "RightLeft" : top ? "RightTop" : "RightBottom") : left ? (top ? "LeftTop" : "LeftBottom") : "TopBottom";
        else if (count == 3)
            rotation = !right ? "Left" : (!left ? "Right" : (!top ? "Bottom" : "Top"));

        if (!rotation.Equals(""))
            rotation = "_" + rotation;

        construction.GetComponentInChildren<SpriteRenderer>().sprite = Resources.Load<Sprite>("Sprites/Constructions/" + (construction.GetType().Equals(typeof(SC_Bastion)) ? "Bastion/" : "Wall/") + count.ToString() + rotation);

    }
    #endregion

    #region Display Actions
    public void DisplaySacrifices () {        

        foreach (SC_Soldier soldier in FindObjectsOfType<SC_Soldier>())
            GetTileAt(soldier.gameObject).ChangeDisplay(TDisplay.Sacrifice);

    }

    /*public void DisplayResurrection () {

        foreach (SC_Tile tile in tiles)
            if (tile.Empty)
                tile.ChangeDisplay(TDisplay.Resurrection);

    }*/
    #endregion

    public void HidePumpRange () {

        if (DisplayedPump) {

            foreach (SC_Tile t in GetRange(DisplayedPump.transform.position, DisplayedPump.range))
                t.RemoveFilter();

            DisplayedPump = null;

        }

    }

}
