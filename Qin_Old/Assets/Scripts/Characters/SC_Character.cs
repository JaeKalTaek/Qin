using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using static SC_Enums;

public class SC_Character : NetworkBehaviour {

	//Alignment
	public bool coalition;

	//Actions
	public int movement = 5;
	protected bool canMove;

	public SC_Tile attackTarget;

	protected bool finishMovement;

	public SC_Tile LastPos { get; set; }

	//Stats
	public string characterName;
	public int maxHealth;
	[HideInInspector]
	public int health;
	public int strength, armor;
	public int qi, resistance;
	public int technique, speed;
	[HideInInspector]
	public int criticalHit, dodgeHit;
    public float moveSpeed = .25f;

	protected Color baseColor, tiredColor;

	[HideInInspector]
	public SC_Lifebar lifebar;

	[HideInInspector]
	public bool selfPanel;

	protected static SC_Tile_Manager tileManager;

	protected static SC_GameManager gameManager;

	protected static SC_UI_Manager uiManager;

    List<SC_Tile> path;

    public static SC_Character attackingCharacter;

    protected virtual void Awake() {

		baseColor = GetComponent<SpriteRenderer> ().color;
		tiredColor = new Color (.15f, .15f, .15f);

	}

	protected virtual void Start() {

        if(!gameManager)
            gameManager = SC_GameManager.Instance;

        if(!tileManager)
            tileManager = SC_Tile_Manager.Instance;

        if(!uiManager)
            uiManager = SC_UI_Manager.Instance;

		lifebar = Instantiate(Resources.Load<GameObject>("Prefabs/P_Lifebar"), transform).GetComponent<SC_Lifebar>();
		lifebar.transform.position += new Vector3 (0, -.44f, 0);

		health = maxHealth;
		criticalHit = technique;
		dodgeHit = speed;

		LastPos = tileManager.GetTileAt(gameObject);

		canMove = coalition;

	}

	protected virtual void OnMouseDown() {

		if ((coalition != SC_Player.localPlayer.IsQin()) && !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject ()) {

			if (gameManager.player.Turn() && (tileManager.GetTileAt(gameObject).CurrentDisplay == TDisplay.None))
                PrintMovements();               

				/*SC_Tile under = tileManager.GetTileAt (gameObject);

                if (under.CurrentDisplay == TDisplay.Movement) {

                    SC_Player.localPlayer.CmdMoveCharacterTo((int)under.transform.position.x, (int)under.transform.position.y);

				} else if (under.CurrentDisplay == TDisplay.Attack) {

					SC_Tile attackingCharacterTile = tileManager.GetTileAt (attackingCharacter.gameObject);
					gameManager.rangedAttack = !tileManager.IsNeighbor (attackingCharacterTile, under);

					attackingCharacter.attackTarget = under;

					if (attackingCharacter.IsHero ()) {

						((SC_Hero)attackingCharacter).ChooseWeapon ();

					} else {

                        tileManager.RemoveAllFilters();

						gameManager.Attack ();

					}

				} /*else if ((under.CurrentDisplay == TDisplay.Construct) && ((SC_Qin.Energy > SC_Qin.Qin.wallCost) || gameManager.Bastion) && !IsHero ()) {

					gameManager.ConstructAt (under);

					canMove = false;

				} else if (under.CurrentDisplay == TDisplay.Sacrifice) {

					SC_Player.localPlayer.CmdChangeQinEnergy (SC_Qin.Qin.sacrificeValue);

					canMove = false;

					under.RemoveFilter ();

                    SC_Player.localPlayer.CmdDestroyCharacter(gameObject);

                } else if (under.CurrentDisplay == TDisplay.None) {                    

                    PrintMovements ();

				}


            }*/

		}

	}

	protected virtual void PrintMovements() { }

	protected void OnMouseOver() {

		if(Input.GetMouseButtonDown(1))
			uiManager.ShowHideInfos (gameObject, GetType());

	}

	public virtual void MoveTo(SC_Tile target) {

        tileManager.RemoveAllFilters();

        LastPos = tileManager.GetTileAt(gameObject);

        path = PathFinder(LastPos, target, gameManager.GetClosedList ());

        if(path == null)
            FinishMovement(false);
        else
            StartCoroutine("Move");

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

        SC_Tile target = moved ? path[path.Count - 1] : null;

        if(moved) {

            transform.SetPos(target.transform);

            LastPos.MovementCost = LastPos.baseCost;
            LastPos.CanSetOn = true;
            LastPos.Attackable = (!LastPos.Construction || LastPos.Bastion && coalition);
            LastPos.Character = null;

            target.MovementCost = 5000;
            target.CanSetOn = false;
            target.Attackable = (coalition != gameManager.CoalitionTurn());
            target.Character = this;

        }

        canMove = false;

        attackingCharacter = this;

        if(IsHero()) {

            canMove = (((SC_Hero)this).berserk && !((SC_Hero)this).berserkTurn);

            if(moved && target.Construction) {

                if(target.Village && SC_Player.localPlayer.Turn()) {

                    uiManager.villagePanel.SetActive(true);

                } else {

                    gameManager.cantCancelMovement = true;

                    CheckAttack();

                }

            } else {

                if(SC_Player.localPlayer.Turn())
                    uiManager.cancelMovementButton.SetActive(true);

                CheckAttack();

            }

            if(moved) {

                LastPos.Constructable = !LastPos.Palace;
                target.Constructable = false;

            }

        } else {

            /*if(moved && tileManager.GetAt<SC_Convoy>(target)) {

                tileManager.GetAt<SC_Convoy>(target).DestroyConvoy();
                gameManager.cantCancelMovement = true;

            } else*/ if(SC_Player.localPlayer.Turn()) {

                uiManager.cancelMovementButton.SetActive(true);

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

    public void CheckAttack() {

        tileManager.RemoveAllFilters();

        //uiManager.HideWeapons();

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

            if(attackingCharacter.IsHero())
                ((SC_Hero)attackingCharacter).berserkTurn = ((SC_Hero)attackingCharacter).berserk;

            attackingCharacter = null;

        }

    }   

	public virtual void DestroyCharacter() {

        if(uiManager.currentGameObject == gameObject)
            uiManager.HideInfos (gameObject);

		SC_Tile under = tileManager.GetTileAt (gameObject);

		under.MovementCost = under.baseCost;

		under.CanSetOn = true;

		under.Attackable = (!under.Construction || under.Bastion && coalition);

	}

	public SC_Weapon GetActiveWeapon() {

		return IsHero() ? ((SC_Hero)this).GetWeapon(true) : ((SC_Soldier)this).weapon;

	}

	public bool HasRange() {

		if (IsHero())
			return (((SC_Hero)this).weapon1.ranged || ((SC_Hero)this).weapon2.ranged);
		else
			return ((SC_Soldier)this).weapon.ranged;

	}

	public virtual bool Hit(int damages, bool saving) {

		health -= damages;

		return false;

	}

	public bool IsHero() {

		return (GetType().Equals(typeof(SC_Hero)) || GetType().IsSubclassOf(typeof(SC_Hero)));

	}

	public virtual void Tire() {

		GetComponent<SpriteRenderer> ().color = tiredColor;

	}

	public virtual void UnTired() {

		GetComponent<SpriteRenderer> ().color = baseColor;

	}

	public bool GetCanMove() {

		return canMove;

	}

	public void SetCanMove(bool c) {

		canMove = c;

	}

	public void SetBaseColor(Color c) {

		baseColor = c;

	}

}
