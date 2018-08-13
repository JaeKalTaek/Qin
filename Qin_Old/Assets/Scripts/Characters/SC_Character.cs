﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class SC_Character : NetworkBehaviour {

	//Alignment
	public bool coalition;

	//Actions
	public int movement = 5;
	protected bool canMove;

	public bool attacking;

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

                if (under.displayMovement) {

                    SC_Player.localPlayer.CmdMoveCharacterTo((int)under.transform.position.x, (int)under.transform.position.y);

				} else if (under.GetDisplayAttack ()) {

					SC_Character attackingCharacter = GetAttackingCharacter ();
					SC_Tile attackingCharacterTile = tileManager.GetTileAt (attackingCharacter.gameObject);
					gameManager.rangedAttack = !gameManager.IsNeighbor (attackingCharacterTile, under);

					attackingCharacter.attackTarget = under;

					if (attackingCharacter.isHero ()) {

						((SC_Hero)attackingCharacter).ChooseWeapon ();

					} else {

						foreach (SC_Tile tile in tileManager.tiles)
							tile.RemoveFilters ();

						gameManager.Attack ();

					}

				} else if (under.displayConstructable && (((SC_Qin.GetEnergy () - 50) > 0) || gameManager.IsBastion ()) && !isHero ()) {

					gameManager.ConstructAt (under);

					canMove = false;

				} else if (under.displaySacrifice) {

					SC_Player.localPlayer.CmdChangeQinEnergy (SC_Qin.Qin.sacrificeValue);

					canMove = false;

					under.RemoveFilters ();

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

        //lastPos, path[i], (i == (path.Count - 1)), i * 0.1f

        if(path == null)
            FinishMovement(false);
        else
            StartCoroutine(Move());

    }    

	IEnumerator Move(/*SC_Tile leavingTile, SC_Tile target, bool last, float delay*/) {

        int pathIndex = 0;

        float movementTimer = 0;

        Vector3 currentStart = transform.position;

        Vector3 currentEnd = path[0].transform.position;

        while ((pathIndex < path.Count) && (movementTimer < moveSpeed)) {

            movementTimer = Mathf.Min(movementTimer + Time.deltaTime, 1);

            transform.SetPos(Vector3.Lerp(currentStart, currentEnd, movementTimer/moveSpeed));

            if(movementTimer == 1) {

                pathIndex++;

                movementTimer = 0;

                currentStart = transform.position;

                currentEnd = path[pathIndex].transform.position;

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

        attacking = true;

        if(isHero()) {

            canMove = (((SC_Hero)this).berserk && !((SC_Hero)this).berserkTurn);

            if(tileManager.GetAt<SC_Construction>(target) && moved) {

                if(tileManager.GetAt<SC_Village>(target)) {

                    uiManager.villagePanel.SetActive(true);

                } else {

                    gameManager.cantCancelMovement = true;

                    gameManager.CheckAttack(this);

                }

            } else {

                uiManager.cancelAttackButton.SetActive(true);
                gameManager.CheckAttack(this);

            }

            if(moved) {

                leavingTile.constructable = !leavingTile.IsPalace();
                target.constructable = false;

            }

        } else {

            if(tileManager.GetAt<SC_Convoy>(target) && moved) {

                tileManager.GetAt<SC_Convoy>(target).DestroyConvoy();
                gameManager.cantCancelMovement = true;

            } else {

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

        if(path.Count > 1) {            

            path.RemoveAt(0);

            return path;

        } else {

            return null;

        }

	}

	public static SC_Character GetAttackingCharacter() {

		SC_Character attackingCharacter = null;

		foreach (SC_Character character in FindObjectsOfType<SC_Character>())
			if(character.attacking) attackingCharacter = character;

		return attackingCharacter;

	}

	public static void ResetAttacker() {

		foreach (SC_Character character in FindObjectsOfType<SC_Character>()) {
			if (character.attacking) character.Tire ();
			character.attacking = false;
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

		return isHero() ? ((SC_Hero)this).GetWeapon(true) : ((SC_Soldier)this).weapon;

	}

	public bool HasRange() {

		if (isHero())
			return (((SC_Hero)this).weapon1.ranged || ((SC_Hero)this).weapon2.ranged);
		else
			return ((SC_Soldier)this).weapon.ranged;

	}

	public virtual bool Hit(int damages, bool saving) {

		health -= damages;

		return false;

	}

	public bool isHero() {

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
