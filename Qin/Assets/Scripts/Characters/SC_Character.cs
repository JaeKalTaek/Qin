using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using static SC_Global;

public class SC_Character : NetworkBehaviour {	   

    // Public Variables
    [Header("Character variables")]
    [Tooltip("Name of this character")]
    public string characterName;

    [Tooltip("Base movement distance of this character")]
    public int baseMovement;

    public int Movement { get; set; }

    public bool CanMove { get; set; }

    [Tooltip("Time for a character to walk one tile of distance")]
    public float moveDuration;

    [Tooltip("Base Maximum Health of this character")]
	public int maxHealth;

    public int Health { get; set; }

    public SC_Lifebar Lifebar { get; set; }

    [Tooltip("Strength of this character")]
    public int strength;

    [Tooltip("Armor of this character")]
    public int armor;

    [Tooltip("Qi of this character")]
    public int qi;

    [Tooltip("Resistance of this character")]
    public int resistance;

    [Tooltip("Technique of this character, amount of Crits Jauge gained after attacking")]
    public int technique;

    public int CriticalAmount { get; set; }

    [Tooltip("Reflexes of this character, amount of Dodge Jauge gained after being attacked")]
    public int reflexes;	
	
    public int DodgeAmount { get; set; }

    public CombatModifiers Modifiers { get { return Tile.Construction?.combatModifers ?? (Tile.Ruin?.combatModifers ?? Tile.combatModifers); } }

    public int BaseDamage { get { return Mathf.Max(0, GetActiveWeapon().weaponOrQi ? strength + Modifiers.strength : qi + Modifiers.qi); } }

    [Tooltip("Color applied when the character is tired")]
    public Color tiredColor = new Color(.15f, .15f, .15f);

    public Color BaseColor { get; set; }	

    public bool Qin { get; set; }

    public SC_Hero Hero { get { return this as SC_Hero; } }

    public SC_Soldier Soldier { get { return this as SC_Soldier; } }

    public SC_Tile AttackTarget { get; set; }

    public bool HasRange { get { return Hero ? Hero.weapon1.ranged || Hero.weapon2.ranged : Soldier.weapon.ranged; } }

    public SC_Tile LastPos { get; set; }

    protected static SC_Tile_Manager tileManager;

	protected static SC_Game_Manager gameManager;

	protected static SC_UI_Manager uiManager;

    protected static SC_Fight_Manager fightManager;

    List<SC_Tile> path;

    public static SC_Character attackingCharacter, characterToMove;

    public SC_Tile Tile { get { return tileManager.GetTileAt(gameObject); } }

    [HideInInspector]
    [SyncVar]
    public string characterPath;

    protected SC_Character loadedCharacter;

    protected virtual void Awake() {

        if (!gameManager)
            gameManager = SC_Game_Manager.Instance;

        Qin = !Hero;

		BaseColor = GetComponent<SpriteRenderer> ().color;

        CanMove = Qin == gameManager.Qin;        

    }

    public override void OnStartClient () {

        loadedCharacter = Resources.Load<SC_Character>(characterPath);

        characterName = loadedCharacter.characterName;

        baseMovement = loadedCharacter.baseMovement;

        moveDuration = loadedCharacter.moveDuration;

        maxHealth = loadedCharacter.maxHealth;

        strength = loadedCharacter.strength;

        armor = loadedCharacter.armor;

        qi = loadedCharacter.qi;

        resistance = loadedCharacter.resistance;

        technique = loadedCharacter.technique;

        reflexes = loadedCharacter.reflexes;

        tiredColor = loadedCharacter.tiredColor;

        GetComponent<SpriteRenderer>().sprite = loadedCharacter.GetComponent<SpriteRenderer>().sprite;

    }

    protected virtual void Start() {

        if(!tileManager)
            tileManager = SC_Tile_Manager.Instance;

        if(!uiManager)
            uiManager = SC_UI_Manager.Instance;

        if (!fightManager)
            fightManager = SC_Fight_Manager.Instance;

        Health = maxHealth;

        Lifebar = Instantiate(Resources.Load<GameObject>("Prefabs/Characters/Components/P_Lifebar"), transform).GetComponent<SC_Lifebar>();
		Lifebar.transform.position += new Vector3 (0, -.44f, 0);		

		LastPos = Tile;

        LastPos.Character = this;

        Movement = baseMovement;

	}

    #region Movement
    public virtual void TryCheckMovements () {

        SC_Player.localPlayer.CmdCheckMovements(transform.position.x.I(), transform.position.y.I());

        uiManager.SetCancelButton(gameManager.UnselectCharacter);

    }

    public virtual void MoveTo(SC_Tile target) {

        tileManager.RemoveAllFilters();

        LastPos = Tile;

        path = tileManager.PathFinder(LastPos, target);

        if(path == null)
            FinishMovement(false);
        else
            StartCoroutine(Move());

    }    

	IEnumerator Move() {

        int pathIndex = 1;

        float movementTimer = 0;

        Vector3 currentStart = transform.position;

        Vector3 currentEnd = new Vector3(path[1].transform.position.x, path[1].transform.position.y, transform.position.z);

        while (pathIndex < path.Count) {

            movementTimer = Mathf.Min(movementTimer + Time.deltaTime, moveDuration);

            transform.position = Vector3.Lerp(currentStart, currentEnd, movementTimer/moveDuration);

            if (movementTimer == moveDuration) {

                pathIndex++;

                if(pathIndex < path.Count) {

                    movementTimer = 0;

                    currentStart = transform.position;

                    currentEnd = new Vector3(path[pathIndex].transform.position.x, path[pathIndex].transform.position.y, transform.position.z);

                }

            }

			yield return new WaitForEndOfFrame();

		}

        FinishMovement(true);

    }

    void FinishMovement(bool moved) {

        SC_Tile target = moved ? path[path.Count - 1] : LastPos;

        if(moved) {

            transform.SetPos(target.transform);

            LastPos.Character = null;

            target.Character = this;

            uiManager.TryRefreshInfos(gameObject, GetType());

        }

        CanMove = false;

        attackingCharacter = this;

        bool canAttack = false;

        foreach (SC_Tile tile in tileManager.GetAttackTiles())
            if (!tile.Empty)
                canAttack = true;

        uiManager.attackButton.SetActive(canAttack);

        if(Hero) {

            CanMove = (Hero.Berserk && !Hero.BerserkTurn);

            uiManager.destroyConstruButton.SetActive(!SC_Player.localPlayer.Qin && (target.ProductionBuilding || target.Ruin));

            if (moved) {

                SC_Pump.UpdateHeroSlow(Hero);

                Hero.ReadyToRegen = false;

            }

        } else {

            uiManager.buildConstruButton.SetActive(SC_Player.localPlayer.Qin && (target.Ruin || Soldier.Builder));

        }

        if (SC_Player.localPlayer.Turn) {            

            tileManager.PreviewAttack();

            uiManager.MenuPos(uiManager.characterActionsPanel);

            uiManager.SetCancelButton(gameManager.ResetMovement);

        }        

    }

    public void ResetMovementFunction () {

        uiManager.characterActionsPanel.SetActive(false);

        tileManager.RemoveAllFilters();

        Tile.Character = null;

        transform.SetPos(LastPos.transform);

        LastPos.Character = this;

        uiManager.TryRefreshInfos(gameObject, GetType());

        CanMove = true;

        tileManager.CheckMovements(this);

        UnTired();

        if(Hero)
            SC_Pump.UpdateHeroSlow(Hero);

        if (SC_Player.localPlayer.Turn) {

            SC_Cursor.Instance.Locked = false;

            uiManager.SetCancelButton(gameManager.UnselectCharacter);

        }

        SC_Player.localPlayer.Busy = false;

    }
    #endregion

    public static void Wait() {

        tileManager.RemoveAllFilters();

        characterToMove = null;

        if(attackingCharacter) {

            attackingCharacter.Tire();

            if(attackingCharacter.Hero)
                attackingCharacter.Hero.BerserkTurn = attackingCharacter.Hero.Berserk;

            attackingCharacter = null;

        }

    }   

	public virtual void DestroyCharacter() {

        //tileManager.TryStopPreview(this);

        uiManager.HideInfosIfActive(gameObject);

        Tile.Character = null;

	}

	public SC_Weapon GetActiveWeapon() {

		return Hero?.GetWeapon(true) ?? Soldier.weapon;

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

		GetComponent<SpriteRenderer> ().color = BaseColor;

	}

}
