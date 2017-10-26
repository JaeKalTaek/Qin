using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class SC_Character : NetworkBehaviour {

	//Alignment
	public bool coalition;

	//Actions
	public int movement = 5;
	protected bool canMove, readyToMove;

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

	protected Color baseColor, tiredColor;

    [HideInInspector]
    public SC_Lifebar lifebar;

    [HideInInspector]
    public bool selfPanel;

	protected static SC_Tile_Manager tileManager;

	protected static SC_GameManager gameManager;

	protected static SC_UI_Manager uiManager;

    protected virtual void Awake() {

		baseColor = GetComponent<SpriteRenderer> ().color;
		tiredColor = new Color (.15f, .15f, .15f);

    }

    protected virtual void Start() {

		if (gameManager == null)
			gameManager = GameObject.FindObjectOfType<SC_GameManager> ();

		if (tileManager == null)
			tileManager = GameObject.FindObjectOfType<SC_Tile_Manager> ();

		if (uiManager == null)
			uiManager = GameObject.FindObjectOfType<SC_UI_Manager> ();

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

			SC_Tile under = tileManager.GetTileAt (gameObject);
        
			if (under.displayMovement && readyToMove) {
			
				MoveTo ((int)transform.position.x, (int)transform.position.y);

			} else if (under.GetDisplayAttack ()) {

				SC_Character attackingCharacter = GetAttackingCharacter ();
				SC_Tile attackingCharacterTile = tileManager.GetTileAt (attackingCharacter.gameObject);
				SC_GameManager.GetInstance ().rangedAttack = !SC_GameManager.GetInstance ().IsNeighbor (attackingCharacterTile, under);

				attackingCharacter.attackTarget = under;

				if (attackingCharacter.isHero ()) {
				
					((SC_Hero)attackingCharacter).ChooseWeapon ();

				} else {

					foreach (SC_Tile tile in SC_GameManager.GetInstance().tiles)
						tile.RemoveFilter ();

					SC_GameManager.GetInstance ().Attack ();

				}

			} else if (under.displayConstructable && (((SC_Qin.GetEnergy () - 50) > 0) || SC_GameManager.GetInstance ().IsBastion ()) && !isHero ()) {

				SC_GameManager.GetInstance ().ConstructAt (under);

				SC_GameManager.GetInstance ().StopConstruction ();

				canMove = false;

			} else if (under.displaySacrifice) {

				SC_Qin.ChangeEnergy (25);

				canMove = false;

				under.RemoveFilter ();

				DestroyCharacter ();

			} else {

				PrintMovements ();

			}

		}

	}

	protected virtual void PrintMovements() { }

    protected void OnMouseOver() {

        if(Input.GetMouseButtonDown(1))
			uiManager.ShowHideInfos (gameObject, GetType());

    }

    public virtual void MoveTo(int x, int y) {

        foreach (SC_Tile tile in SC_GameManager.GetInstance().tiles)
            tile.RemoveFilter();

		lastPos = tileManager.GetTileAt (gameObject);
		lastPos.movementCost = lastPos.baseCost;
		lastPos.canSetOn = true;

		SC_Tile target = tileManager.GetTileAt (x, y);

		List<SC_Tile> path = PathFinder(lastPos, target, SC_GameManager.GetInstance ().GetClosedList ());

        for(int i = 0; i < path.Count; i++)
			StartCoroutine(MoveOneTile(lastPos, path[i], (i == (path.Count - 1)), i * 0.1f));

    }

    IEnumerator MoveOneTile(SC_Tile leavingTile, SC_Tile target, bool last, float delay) {

        yield return new WaitForSeconds(delay);

        float startTime = Time.time;

        while (Time.time < startTime + 0.15f) {

			transform.SetPos(Vector3.Lerp(transform.position, target.transform.position, (Time.time - startTime) / 0.2f));
            yield return null;

        }

		transform.SetPos(target.transform);
        
        if(last) {

			if (tileManager.GetAt<SC_Construction>(leavingTile) == null)
				leavingTile.attackable = true;

            target.movementCost = 5000;
            target.canSetOn = false;
			target.attackable = (coalition != SC_GameManager.GetInstance ().CoalitionTurn ());

            canMove = false;

            attacking = true;

			if (isHero ()) {

				canMove = (((SC_Hero)this).berserk && !((SC_Hero)this).berserkTurn);

				if (tileManager.GetAt<SC_Construction>(target) != null) {
			
					if (tileManager.GetAt<SC_Village>(target) != null) {

						uiManager.villagePanel.SetActive (true);

					} else { 

						SC_GameManager.GetInstance ().cantCancelMovement = true;
						SC_GameManager.GetInstance ().CheckAttack (this);

					}

				} else {

					uiManager.cancelAttackButton.SetActive (true);
					SC_GameManager.GetInstance ().CheckAttack (this);

				}

				leavingTile.constructable = !leavingTile.isPalace ();
                target.constructable = false;

            } else {

				if (tileManager.GetAt<SC_Convoy>(target) != null) {
			
					tileManager.GetAt<SC_Convoy>(target).DestroyConvoy ();
					SC_GameManager.GetInstance ().cantCancelMovement = true;

				} else {

					uiManager.cancelAttackButton.SetActive (true);

				}

				SC_GameManager.GetInstance().CheckAttack(this);

			}

        }

    }

	protected List<SC_Tile> PathFinder(SC_Tile start, SC_Tile end, List<SC_Tile> range) {

		List<SC_Tile> openList = new List<SC_Tile>();
		List<SC_Tile> tempList = new List<SC_Tile>();
		List<SC_Tile> closedList = new List<SC_Tile>();

		start.parent = null;
		openList.Add (start);

		while (!openList.Contains (end)) {
			
			foreach (SC_Tile tile in openList) {
				
				foreach (SC_Tile neighbor in SC_GameManager.GetInstance ().GetNeighbors (tile)) {
					
					if (!closedList.Contains (neighbor) && range.Contains (neighbor) && !tempList.Contains (neighbor)) {

						tempList.Add (neighbor);
						neighbor.parent = tile;

					}

				}
				
				closedList.Add (tile);

			}
				
			openList = new List<SC_Tile>(tempList);
			tempList.Clear ();

		}

		List<SC_Tile> path = new List<SC_Tile>();
		SC_Tile currentParent = end;

		while (!path.Contains (start)) {

			path.Add (currentParent);
			currentParent = currentParent.parent;

		}

		foreach (SC_Tile tile in SC_GameManager.GetInstance().tiles)
			tile.parent = null;

		path.Reverse ();
        if(path.Count > 1) path.RemoveAt (0);
		return path;

	}
		
    public static SC_Character GetCharacterToMove() {

		SC_Character characterToMove = null;

		foreach (SC_Character character in FindObjectsOfType<SC_Character>())
			if(character.readyToMove) characterToMove = character;

		return characterToMove;

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

    public bool IsReadyToMove() {

        return readyToMove;

    }

    public void SetReadyToMove(bool r) {

        readyToMove = r;

    }

	public void SetBaseColor(Color c) {

		baseColor = c;

	}

}
