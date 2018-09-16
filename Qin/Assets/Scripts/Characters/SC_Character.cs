using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using static SC_Enums;

public class SC_Character : NetworkBehaviour {

	//Alignment
	public bool coalition;

    public bool IsHero { get { return (GetType().Equals(typeof(SC_Hero)) || GetType().IsSubclassOf(typeof(SC_Hero))); } }

    public SC_Hero Hero { get { return this as SC_Hero; } }

    public SC_Soldier Soldier { get { return this as SC_Soldier; } }

    //Actions
    public int movement = 5;
	public bool CanMove { get; set; }

	public SC_Tile AttackTarget { get; set; }

	public SC_Tile LastPos { get; set; }

	//Stats
	public string characterName;
	public int maxHealth;
	public int Health { get; set; }
    public int strength, armor;
	public int qi, resistance;
	public int technique, reflexes;
	public int CriticalAmount { get; set; }
    public int DodgeAmount { get; set; }
    public float moveSpeed = .25f;

	protected Color baseColor, tiredColor;

	public SC_Lifebar Lifebar { get; set; }

	protected static SC_Tile_Manager tileManager;

	protected static SC_Game_Manager gameManager;

	protected static SC_UI_Manager uiManager;

    protected static SC_Fight_Manager fightManager;

    List<SC_Tile> path;

    public static SC_Character attackingCharacter, characterToMove;

    protected virtual void Awake() {

		baseColor = GetComponent<SpriteRenderer> ().color;
		tiredColor = new Color (.15f, .15f, .15f);

	}

	protected virtual void Start() {

        if(!gameManager)
            gameManager = SC_Game_Manager.Instance;

        if(!tileManager)
            tileManager = SC_Tile_Manager.Instance;

        if(!uiManager)
            uiManager = SC_UI_Manager.Instance;

        if (!fightManager)
            fightManager = SC_Fight_Manager.Instance;

		Lifebar = Instantiate(Resources.Load<GameObject>("Prefabs/P_Lifebar"), transform).GetComponent<SC_Lifebar>();
		Lifebar.transform.position += new Vector3 (0, -.44f, 0);

		Health = maxHealth;

		LastPos = tileManager.GetTileAt(gameObject);

        LastPos.Character = this;

		CanMove = coalition == gameManager.CoalitionTurn;

	}

	protected virtual void OnMouseDown() {

        if (SC_UI_Manager.CanInteract && !SC_Player.localPlayer.Busy && (tileManager.GetTileAt(gameObject).CurrentDisplay == TDisplay.None))
            PrintMovements();

	}

	protected virtual void PrintMovements() {        

        SC_Player.localPlayer.CmdCheckMovements((int)transform.position.x, (int)transform.position.y);

        uiManager.cancelMovementButton.SetActive(true);

        uiManager.StopCancelConstruct();

    }

	protected void OnMouseOver() {

		if(Input.GetMouseButtonDown(1))
			uiManager.ShowHideInfos (gameObject, GetType());

	}

	public virtual void MoveTo(SC_Tile target) {

        tileManager.RemoveAllFilters();

        LastPos = tileManager.GetTileAt(gameObject);

        path = PathFinder(LastPos, target, tileManager.ClosedList);

        if(path == null)
            FinishMovement(false);
        else
            StartCoroutine(Move());

    }    

	IEnumerator Move() {

        int pathIndex = 1;

        float movementTimer = 0;

        Vector3 currentStart = transform.position;

        Vector3 currentEnd = path[1].transform.position;

        while (pathIndex < path.Count) {

            movementTimer = Mathf.Min(movementTimer + Time.deltaTime, moveSpeed);

            transform.SetPos(Vector3.Lerp(currentStart, currentEnd, movementTimer/moveSpeed));

            if(movementTimer == moveSpeed) {

                pathIndex++;

                if(pathIndex < path.Count) {

                    movementTimer = 0;

                    currentStart = transform.position;

                    currentEnd = path[pathIndex].transform.position;

                }

            }

			yield return null;

		}

        FinishMovement(true);

    }

    void FinishMovement(bool moved) {

        SC_Player.localPlayer.Busy = false;

        SC_Tile target = moved ? path[path.Count - 1] : null;

        if(moved) {

            transform.SetPos(target.transform);

            LastPos.Character = null;

            target.Character = this;

        }

        CanMove = false;

        attackingCharacter = this;

        if(IsHero) {

            CanMove = (Hero.Berserk && !Hero.BerserkTurn);

            if ((!moved && LastPos.Village) || (moved && target.Village)) {

                SC_Player.localPlayer.Busy = true;

                uiManager.villagePanel.SetActive(true);

            } else {

                if (moved) {

                    if (target.Workshop) {

                        target.Workshop.DestroyConstruction();

                        gameManager.CantCancelMovement = true;

                    }

                    Hero.ReadyToRegen = false;

                }              

                uiManager.resetMovementButton.SetActive(SC_Player.localPlayer.Turn && !gameManager.CantCancelMovement);

                CheckAttack();

            }

        } else {

            /*if(moved && tileManager.GetAt<SC_Convoy>(target)) {

                tileManager.GetAt<SC_Convoy>(target).DestroyConvoy();
                gameManager.cantCancelMovement = true;

            } else*/ if(SC_Player.localPlayer.Turn) {

                uiManager.resetMovementButton.SetActive(true);

            }

            CheckAttack();

        }        

    }

    protected List<SC_Tile> PathFinder(SC_Tile start, SC_Tile end, List<SC_Tile> range) {

        List<SC_Tile> openList = new List<SC_Tile>();
        List<SC_Tile> tempList = new List<SC_Tile>();
        List<SC_Tile> closedList = new List<SC_Tile>();

        start.Parent = null;
        openList.Add(start);

        while(!openList.Contains(end)) {

            foreach(SC_Tile tile in openList) {

                foreach(SC_Tile neighbor in tileManager.GetNeighbors(tile)) {

                    if(!closedList.Contains(neighbor) && range.Contains(neighbor) && !tempList.Contains(neighbor)) {

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

        while(!path.Contains(start)) {

            path.Add(currentParent);
            currentParent = currentParent.Parent;

        }

        foreach(SC_Tile tile in tileManager.tiles)
            tile.Parent = null;

        path.Reverse();

        return (path.Count > 1) ? path : null;

	}

    public void ResetMovementFunction () {

        tileManager.RemoveAllFilters();

        tileManager.GetTileAt(gameObject).Character = null;

        transform.SetPos(LastPos.transform);

        LastPos.Character = this;

        CanMove = true;

        tileManager.CheckMovements(this);

        UnTired();

        uiManager.resetMovementButton.SetActive(false);

        uiManager.cancelMovementButton.SetActive(true);

    }

    public void CheckAttack() {

        tileManager.RemoveAllFilters();

        List<SC_Tile> attackableTiles = new List<SC_Tile>(tileManager.GetNeighbors(tileManager.GetTileAt(gameObject)));

        if(HasRange()) {

            foreach(SC_Tile tile in tileManager.GetNeighbors(tileManager.GetTileAt(gameObject)))
                attackableTiles.AddRange(tileManager.GetNeighbors(tile));

        }

        foreach(SC_Tile tile in attackableTiles)
            if(tile.Attackable)
                tile.ChangeDisplay(TDisplay.Attack);

    }

    public static void CancelAttack() {

        if(attackingCharacter) {

            attackingCharacter.Tire();

            if(attackingCharacter.IsHero)
                attackingCharacter.Hero.BerserkTurn = attackingCharacter.Hero.Berserk;

            attackingCharacter = null;

        }

    }   

	public virtual void DestroyCharacter() {

        if(uiManager.CurrentGameObject == gameObject)
            uiManager.HideInfos (gameObject);

        tileManager.GetTileAt(gameObject).Character = null;

	}

	public SC_Weapon GetActiveWeapon() {

		return IsHero ? Hero.GetWeapon(true) : Soldier.weapon;

	}

	public bool HasRange() {

		if (IsHero)
			return Hero.weapon1.ranged || Hero.weapon2.ranged;
		else
			return Soldier.weapon.ranged;

	}

	public virtual bool Hit(int damages, bool saving) {

		Health -= damages;

		return false;

	}

    protected void UpdateHealth() {

        Lifebar.UpdateGraph(Health, maxHealth);
        uiManager.TryRefreshInfos(gameObject, GetType());

    }	

	public virtual void Tire() {

		GetComponent<SpriteRenderer> ().color = tiredColor;

	}

	public virtual void UnTired() {

		GetComponent<SpriteRenderer> ().color = baseColor;

	}

	public void SetBaseColor(Color c) {

		baseColor = c;

	}

}
