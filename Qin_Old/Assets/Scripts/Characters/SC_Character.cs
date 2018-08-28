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

	[HideInInspector]
	public SC_Tile lastPos;

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

        if(gameManager == null) {

            gameManager = FindObjectOfType<SC_GameManager>();

            tileManager = FindObjectOfType<SC_Tile_Manager>();

            uiManager = FindObjectOfType<SC_UI_Manager>();

        }

		lifebar = Instantiate(Resources.Load<GameObject>("Prefabs/P_Lifebar"), transform).GetComponent<SC_Lifebar>();
		lifebar.transform.position += new Vector3 (0, -.44f, 0);

		health = maxHealth;
		criticalHit = technique;
		dodgeHit = speed;

		lastPos = tileManager.GetTileAt(gameObject);

		canMove = coalition;

	}

	protected virtual void OnMouseDown() {

		if (!UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject ()) {

			if (gameManager.player.Turn()) {

				SC_Tile under = tileManager.GetTileAt (gameObject);

                if (under.CurrentDisplay == TDisplay.Movement) {

                    SC_Player.localPlayer.CmdMoveCharacterTo((int)under.transform.position.x, (int)under.transform.position.y);

				} else if (under.CurrentDisplay == TDisplay.Attack) {

					SC_Tile attackingCharacterTile = tileManager.GetTileAt (attackingCharacter.gameObject);
					gameManager.rangedAttack = !tileManager.IsNeighbor (attackingCharacterTile, under);

					attackingCharacter.attackTarget = under;

					if (attackingCharacter.IsHero ()) {

						((SC_Hero)attackingCharacter).ChooseWeapon ();

					} else {

						foreach (SC_Tile tile in tileManager.tiles)
							tile.RemoveFilter ();

						gameManager.Attack ();

					}

				} else if ((under.CurrentDisplay == TDisplay.Construct) && ((SC_Qin.Energy > SC_Qin.Qin.wallCost) || gameManager.Bastion) && !IsHero ()) {

					gameManager.ConstructAt (under);

					canMove = false;

				} else if (under.CurrentDisplay == TDisplay.Sacrifice) {

					SC_Player.localPlayer.CmdChangeQinEnergy (SC_Qin.Qin.sacrificeValue);

					canMove = false;

					under.RemoveFilter ();

					DestroyCharacter ();

				} else {

                    PrintMovements ();

				}

			}

		}

	}

	protected virtual void PrintMovements() { }

	protected void OnMouseOver() {

		if(Input.GetMouseButtonDown(1))
			uiManager.ShowHideInfos (gameObject, GetType());

	}

	public virtual void MoveTo(SC_Tile target) {

        SC_Player.localPlayer.CmdRemoveAllFilters();

        lastPos = tileManager.GetTileAt (gameObject);
		lastPos.movementCost = lastPos.baseCost;
		lastPos.canSetOn = true;

        path = PathFinder(lastPos, target, gameManager.GetClosedList ());

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

        SC_Tile leavingTile = path[0];
        SC_Tile target = path[path.Count - 1];

        if(moved) {

            if(tileManager.GetAt<SC_Construction>(leavingTile) == null)
                leavingTile.attackable = true;

            target.movementCost = 5000;
            target.canSetOn = false;
            target.attackable = (coalition != gameManager.CoalitionTurn());

        }

        canMove = false;

        attackingCharacter = this;

        if(IsHero()) {

            canMove = (((SC_Hero)this).berserk && !((SC_Hero)this).berserkTurn);

            if(moved && tileManager.GetAt<SC_Construction>(target)) {

                if(tileManager.GetAt<SC_Village>(target) && SC_Player.localPlayer.Turn()) {

                    uiManager.villagePanel.SetActive(true);

                } else {

                    gameManager.cantCancelMovement = true;

                    gameManager.CheckAttack(this);

                }

            } else {

                if(SC_Player.localPlayer.Turn())
                    uiManager.cancelAttackButton.SetActive(true);

                gameManager.CheckAttack(this);

            }

            if(moved) {

                leavingTile.constructable = !leavingTile.IsPalace();
                target.constructable = false;

            }

        } else {

            if(moved && tileManager.GetAt<SC_Convoy>(target)) {

                tileManager.GetAt<SC_Convoy>(target).DestroyConvoy();
                gameManager.cantCancelMovement = true;

            } else if(SC_Player.localPlayer.Turn()) {

                uiManager.cancelAttackButton.SetActive(true);

            }

            gameManager.CheckAttack(this);

        }

    }

    protected List<SC_Tile> PathFinder(SC_Tile start, SC_Tile end, List<SC_Tile> range) {

        List<SC_Tile> openList = new List<SC_Tile>();
        List<SC_Tile> tempList = new List<SC_Tile>();
        List<SC_Tile> closedList = new List<SC_Tile>();

        start.parent = null;
        openList.Add(start);

        while(!openList.Contains(end)) {

            foreach(SC_Tile tile in openList) {

                foreach(SC_Tile neighbor in tileManager.GetNeighbors(tile)) {

                    if(!closedList.Contains(neighbor) && range.Contains(neighbor) && !tempList.Contains(neighbor)) {

                        tempList.Add(neighbor);
                        neighbor.parent = tile;

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
            currentParent = currentParent.parent;

        }

        foreach(SC_Tile tile in tileManager.tiles)
            tile.parent = null;

        path.Reverse();

        return (path.Count > 1) ? path : null;

	}

	public static void ResetAttacker() {

        if(attackingCharacter) {

            attackingCharacter.Tire();

            if(attackingCharacter.IsHero())
                ((SC_Hero)attackingCharacter).berserkTurn = ((SC_Hero)attackingCharacter).berserk;

            attackingCharacter = null;

        }

    }

	public virtual void DestroyCharacter() {

		uiManager.HideInfos (gameObject);

		SC_Tile under = tileManager.GetTileAt (gameObject);
		under.movementCost = under.baseCost;
		under.canSetOn = true;
		if (tileManager.GetAt<SC_Construction>(under) == null)
			under.attackable = true;

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
