using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using static SC_Global;
using static SC_Game_Manager;

public class SC_Tile_Manager : NetworkBehaviour {

	[SyncVar]
	public int xSize, ySize;

	public SC_Tile[,] tiles;

	static SC_Game_Manager gameManager;

    static SC_UI_Manager uiManager;

    public static SC_Tile_Manager Instance { get; set; }

    //Variables used to determine the movements possible
    List<SC_Tile> OpenList { get; set; }
    List<SC_Tile> MovementRange { get; set; }
    Dictionary<SC_Tile, int> movementPoints = new Dictionary<SC_Tile, int>();

    public SC_Pump DisplayedPump { get; set; }

    //public static SC_Character focusedCharacter;

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
			tiles [t.transform.position.x.I(), t.transform.position.y.I()] = t;

		gameManager.FinishSetup ();

		if (gameManager.Player)
			gameManager.Player.SetTileManager (this);

        OpenList = new List<SC_Tile>();
        MovementRange = new List<SC_Tile>();

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

        return Mathf.Abs((a.x - b.x).I()) + Mathf.Abs((a.y - b.y).I());

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

        return tiles[pos.x.I(), pos.y.I()];

    }

    public void RemoveAllFilters () {

        //focusedCharacter = null;

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
            if (!t.CanCharacterAttack(attacker))
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

        MovementRange = DisplayMovementAndAttack(target, false);

    }

    public List<SC_Tile> DisplayMovementAndAttack (SC_Character target, bool preview) {        

        OpenList.Clear();
        List<SC_Tile> movementRange = new List<SC_Tile>();

        movementPoints[target.Tile] = target.Movement;

        ExpandTile(ref movementRange, target.Tile, target);

        while (OpenList.Count > 0) {

            OpenList.Sort((a, b) => movementPoints[a].CompareTo(movementPoints[b]));

            SC_Tile tile = OpenList[OpenList.Count - 1];

            OpenList.RemoveAt(OpenList.Count - 1);

            ExpandTile(ref movementRange, tile, target);

        }

        if (SC_Player.localPlayer.Turn || preview) {

            foreach (SC_Tile tile in new List<SC_Tile>(movementRange) { target.Tile }) {

                if (tile.CanCharacterSetOn(target)) {

                    if (preview)
                        tile.SetFilter(TDisplay.PreviewMovement);
                    else
                        tile.ChangeDisplay(TDisplay.Movement);

                    foreach (SC_Tile t in GetAttackTiles(target, tile.transform.position))
                        if (t.CurrentDisplay == TDisplay.None && !movementRange.Contains(t))
                            t.SetFilter(TDisplay.PreviewAttack);

                }

            }
        }

        return movementRange;

    }

    /*public void PreviewMovementAndAttack(SC_Character target, SC_Tile tileTarget) {        

        RemoveAllFilters();

        //focusedCharacter = target;

        DisplayMovementAndAttack(target, tileTarget, true);

    }

    public void TryStopPreview(SC_Character c) {

        //if (c == focusedCharacter)
            RemoveAllFilters();

    }*/

    void ExpandTile (ref List<SC_Tile> list, SC_Tile aTile, SC_Character target) {

        int parentPoints = movementPoints[aTile];

        list.Add(aTile);

        foreach (SC_Tile tile in GetTilesAtDistance(aTile, 1)) {

            if (list.Contains(tile) || OpenList.Contains(tile) || !tile.CanCharacterGoThrough(target))
                continue;

            int points = parentPoints - ((target.Hero?.Berserk ?? false) ? 1 : tile.cost);

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

                    if (!closedList.Contains(neighbor) && MovementRange.Contains(neighbor) && !tempList.Contains(neighbor)) {

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
            soldier.Tile.ChangeDisplay(TDisplay.Sacrifice);

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
