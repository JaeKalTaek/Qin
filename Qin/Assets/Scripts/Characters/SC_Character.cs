﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using static SC_Global;

public abstract class SC_Character : NetworkBehaviour {

    #region Stats
    [Header("Character variables")]
    [Tooltip("Name of this character")]
    public string characterName;

    [Tooltip("Base Maximum Health of this character")]
    public int maxHealth;
    public int Health { get; set; }
    public SC_Lifebar Lifebar { get; set; }

    [Tooltip("Strength of this character")]
    public int baseStrength;
    public int StrengthModifiers { get; set; }
    public int Strength { get { return Mathf.Max(0, baseStrength + StrengthModifiers + CombatModifiers().strength + DemonsModifier("strength")); } }

    [Tooltip("Chi of this character")]
    public int baseChi;
    public int ChiModifiers { get; set; }
    public int Chi { get { return Mathf.Max(0, baseChi + ChiModifiers + CombatModifiers().chi + DemonsModifier("chi")); } }

    [Tooltip("Armor of this character")]
    public int baseArmor;
    public int ArmorModifiers { get; set; }
    public int Armor { get { return baseArmor + ArmorModifiers + CombatModifiers().armor + DemonsModifier("armor"); } }

    [Tooltip("Resistance of this character")]
    public int baseResistance;
    public int ResistanceModifiers { get; set; }
    public int Resistance { get { return baseResistance + ResistanceModifiers + CombatModifiers().resistance + DemonsModifier("resistance"); } }

    [Tooltip("Technique of this character, amount of Crits Jauge gained after attacking")]
    public int baseTechnique;
    public int TechniqueModifiers { get; set; }
    public int Technique { get { return Mathf.Max(0, baseTechnique + TechniqueModifiers + CombatModifiers().technique + DemonsModifier("technique")); } }

    public int CriticalAmount { get; set; }

    [Tooltip("Reflexes of this character, amount of Dodge Jauge gained after being attacked")]
    public int baseReflexes;
    public int ReflexesModifiers { get; set; }
    public int Reflexes { get { return Mathf.Max(0, baseReflexes + ReflexesModifiers + CombatModifiers().reflexes + DemonsModifier("reflexes")); } }

    public int DodgeAmount { get; set; }

    [Tooltip("Base movement distance of this character")]
    public int baseMovement;
    public int MovementModifiers { get; set; }
    public int Movement { get { return Mathf.Max(0, baseMovement + MovementModifiers + CombatModifiers().movement + DemonsModifier("movement")); } }    

    public bool CanMove { get; set; }

    [Tooltip("Time for a character to walk one tile of distance")]
    public float moveDuration;

    public int RangeModifiers { get; set; }      

    public int BaseDamage { get { return GetActiveWeapon().physical ? Strength : Chi; } }
    #endregion

    [Tooltip("Color applied when the character is tired")]
    public Color tiredColor = new Color(.15f, .15f, .15f);

    public Color BaseColor { get; set; }	

    public bool Qin { get; set; }

    public SC_Hero Hero { get { return this as SC_Hero; } }

    public SC_BaseQinChara BaseQinChara { get { return this as SC_BaseQinChara; } }

    public SC_Soldier Soldier { get { return this as SC_Soldier; } }

    public SC_Demon Demon { get { return this as SC_Demon; } }

    public SC_Tile AttackTarget { get; set; }

    public SC_Tile LastPos { get; set; }

    public List<Actions> possiblePlayerActions;

    public List<Actions> possibleCharacterActions;

    #region Managers
    protected static SC_Tile_Manager tileManager;

	protected static SC_Game_Manager gameManager;

	protected static SC_UI_Manager uiManager;

    protected static SC_Fight_Manager fightManager;
    #endregion

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
        Health = maxHealth;

        Lifebar = Instantiate(Resources.Load<GameObject>("Prefabs/Characters/Components/P_Lifebar"), transform).GetComponent<SC_Lifebar>();
        Lifebar.transform.position += new Vector3(0, -.44f, 0);        

        baseStrength = loadedCharacter.baseStrength;

        baseArmor = loadedCharacter.baseArmor;

        baseChi = loadedCharacter.baseChi;

        baseResistance = loadedCharacter.baseResistance;

        baseTechnique = loadedCharacter.baseTechnique;

        baseReflexes = loadedCharacter.baseReflexes;

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

		LastPos = Tile;

        LastPos.Character = this;

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

        }

        CanMove = false;

        attackingCharacter = this;

        bool canAttack = false;

        foreach (SC_Tile tile in tileManager.GetAttackTiles())
            if (!tile.Empty && tile.CanCharacterAttack(this))
                canAttack = true;

        uiManager.attackButton.SetActive(canAttack);

        if (Hero) {

            CanMove = (Hero.Berserk && !Hero.BerserkTurn);

            uiManager.destroyConstruButton.SetActive(!SC_Player.localPlayer.Qin && (target.ProductionBuilding || target.Ruin));

            if (moved) {

                SC_Pump.UpdateHeroSlow(Hero);

                Hero.ReadyToRegen = false;

            }

        } else if (Soldier) {

            uiManager.buildConstruButton.SetActive(SC_Player.localPlayer.Qin && (target.Ruin || (Soldier.Builder && target.Constructable(true))));

        } else if (Demon) {

            if (moved) {

                uiManager.buildConstruButton.SetActive(false);

                Demon.RemoveAura(false, LastPos);

                Demon.AddAura(false);

            }

        }

        if(moved)
            uiManager.TryRefreshInfos(gameObject, GetType());

        if (SC_Player.localPlayer.Turn) {            

            tileManager.PreviewAttack();

            uiManager.MenuManager.MenuPos(SC_Menu_Manager.actionMenu.Character);

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

        uiManager.HideInfosIfActive(gameObject);

        Tile.Character = null;

	}

	public SC_Weapon GetActiveWeapon() {

		return Hero?.GetWeapon(true) ?? BaseQinChara.weapon;

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

    public abstract Vector2 GetRange (SC_Tile t = null);

    public int Range (SC_Tile t) {

        return CombatModifiers(t).range + RangeModifiers + DemonsModifier("range", t);

    } 

    public SC_CombatModifiers CombatModifiers(SC_Tile t = null) {

        if (!t)
            t = Tile;

        return t.Construction?.combatModifers ?? (t.Ruin?.combatModifers ?? t.combatModifers);

    }

    int DemonsModifier(string id, SC_Tile t = null) {

        if (!t)
            t = Tile;

        if (Demon)
            return 0;

        int modif = 0;

        foreach (DemonAura dA in t.DemonAuras)
            modif += (int)dA.aura.GetType().GetField(id).GetValue(dA.aura) * (Qin ? 1 : -1);

        return modif;

    }

    public virtual bool CanCharacterGoThrough (SC_Tile t) {

        if (t.Character)
            return Qin == t.Character.Qin;
        else if (t.Construction)
            return (Qin || !t.Construction.GreatWall) && !t.Pump;
        else if (t.Qin)
            return Qin;
        else
            return true;

    }

    public virtual bool CanCharacterSetOn (SC_Tile t) {

        if ((t.Character && (t.Character != this)) || t.Qin)
            return false;
        else if (t.Construction)
            return (Qin || !t.Construction.GreatWall) && !t.Pump;
        else
            return true;

    }

}
