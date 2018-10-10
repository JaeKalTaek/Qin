using System;
using UnityEngine;
using UnityEngine.Networking;
using static SC_Global;

[System.Serializable]
public class SC_Tile : NetworkBehaviour {

    public TDisplay CurrentDisplay { get; set; }

    [Header("Tile Variables")]
    [Tooltip("Movement cost to walk on this tile")]
    public int cost;

    [Tooltip("Colors for the different filters of this Tile")]
    public FilterColor[] filtersColors;

    [Tooltip("Combat modifiers for this tile")]
    public CombatModifiers combatModifers;

    [Serializable]
    public struct FilterColor {

        public TDisplay filter;

        public Color color;

    }

    [HideInInspector]
    [SyncVar]
    public bool river;

    [HideInInspector]
    [SyncVar]
    public int riverSprite;

    public bool CanCharacterGoThrough (SC_Character c) {

        if (Character)
            return c.Qin == Character.Qin;
        else if (Construction)
            return (c.Qin || !Bastion) && !Pump;
        else if (Qin)
            return c.Qin;
        else
            return true;

    }

    public bool CanCharacterSetOn (SC_Character c) {

        if (Character || Qin)
            return false;
        else if (Construction)
            return (c.Qin || !Bastion) && !Pump;
        else
            return true;

    }

	public bool CanCharacterAttack(SC_Character c) {

        if (Character)
            return c.Qin != Character.Qin;
        else if (Construction)
            return !c.Qin && (Bastion || Pump);
        else if (Qin)
            return !c.Qin;
        else
            return true;

    }

    public bool Constructable { get { return !name.Contains("Palace") && (!Character || (gameManager.QinTurnStarting && Soldier)) && !Construction && !Ruin; } }

    public SC_Construction Construction { get; set; }

    public SC_Village Village { get { return Construction as SC_Village; } }

    public SC_Bastion Bastion { get { return Construction as SC_Bastion; } }

    public SC_Wall Wall { get { return Construction as SC_Wall; } }

    public SC_Workshop Workshop { get { return Construction as SC_Workshop; } }

    public SC_Pump Pump { get { return Construction as SC_Pump; } }

    public bool ProductionBuilding { get { return Construction?.production ?? false; } }

    public SC_Ruin Ruin { get; set; }

    public SC_Character Character { get; set; }

    public SC_Hero Hero { get { return Character as SC_Hero; } }

    public SC_Soldier Soldier { get { return Character as SC_Soldier; } }

    public SC_Qin Qin { get; set; }

    public bool Empty { get { return !Construction && !Character && !Qin; } }

    public bool Palace { get { return name.Contains("Palace"); } }

    public bool CursorOn { get; set; }

    // Used for PathFinder
    public SC_Tile Parent { get; set; }

	static SC_Game_Manager gameManager;

	static SC_Tile_Manager tileManager;

    static SC_UI_Manager uiManager;

    static SC_Fight_Manager fightManager;

    SpriteRenderer filter;

    public override void OnStartClient () {

        base.OnStartClient();

        if (river)
            GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Sprites/Tiles/River/" + (SC_EditorTile.RiverSprite)riverSprite);

    }

    void Start() {

        if(!gameManager)
            gameManager = SC_Game_Manager.Instance;

        if(!tileManager)
            tileManager = SC_Tile_Manager.Instance;

        if(!uiManager)
            uiManager = SC_UI_Manager.Instance;

        if (!fightManager)
            fightManager = SC_Fight_Manager.Instance;

        if (Mathf.RoundToInt(transform.position.x) == (gameManager.baseMapPrefab.SizeMapX - 1) && Mathf.RoundToInt(transform.position.y) == (gameManager.baseMapPrefab.SizeMapY - 1) && !isServer)
            gameManager.StartCoroutine("FinishLoading");

        filter = transform.GetChild(0).GetComponent<SpriteRenderer>();

    }

    public void CursorClick() {

        if (SC_UI_Manager.CanInteract) {

            if (CurrentDisplay == TDisplay.Construct) {

                SC_Player.localPlayer.CmdConstructAt(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y));

            } else if (CurrentDisplay == TDisplay.Movement) {

                SC_Cursor.Instance.Locked = true;

                uiManager.cancelButton.gameObject.SetActive(false);                

                SC_Player.localPlayer.Busy = true;

                SC_Player.localPlayer.CmdMoveCharacterTo(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y));

            } else if (CurrentDisplay == TDisplay.Attack) {

                fightManager.RangedAttack = tileManager.TileDistance(SC_Character.attackingCharacter.transform.position, this) > 1;

                SC_Player.localPlayer.CmdPrepareForAttack(fightManager.RangedAttack, gameObject, !SC_Player.localPlayer.Qin);

                if (SC_Character.attackingCharacter.Hero)
                    SC_Character.attackingCharacter.Hero.ChooseWeapon();
                else
                    SC_Player.localPlayer.CmdAttack();

            } else if (CurrentDisplay == TDisplay.Sacrifice) {

                SC_Player.localPlayer.CmdChangeQinEnergy(Soldier.sacrificeValue);

                RemoveFilter();

                Character.CanMove = false;

                SC_Player.localPlayer.CmdDestroyCharacter(Character.gameObject);

            } /*else if (CurrentDisplay == TDisplay.Resurrection) {

                uiManager.EndQinAction("qinPower");

                SC_Qin.UsePower(transform.position);

            }*/ else if (CurrentDisplay == TDisplay.None && SC_UI_Manager.CanInteract && !SC_Player.localPlayer.Busy) {

                if (Character && (Character.Qin == SC_Player.localPlayer.Qin))
                    Character.TryCheckMovements();
                else if (Workshop && SC_Player.localPlayer.Qin)
                    Workshop.SelectWorkshop();
                else
                    uiManager.playerActionsPanel.SetActive(true);

            }

        }

	}

    public void CursorSecondaryClick() {

        if (Character)
            Character.ShowHideInfos();
        else if (Construction)
            Construction.ShowHideInfos();
        else
            Qin?.ShowHideInfos();

    }

    public void OnCursorEnter() {

        CursorOn = true;

        if (CurrentDisplay == TDisplay.Attack)
            Hero?.PreviewFight();
        else if (CurrentDisplay == TDisplay.Sacrifice)
            Soldier.ToggleDisplaySacrificeValue();
        else if (Character && !SC_Player.localPlayer.Busy && (!SC_Character.characterToMove || (SC_Character.characterToMove.Qin != SC_Player.localPlayer.Qin)))
            tileManager.PreviewMovementAndAttack(Character, this);

    }

    public void OnCursorExit() {

        CursorOn = false;

        if (CurrentDisplay == TDisplay.Attack && Hero)
            uiManager.HidePreviewFight();
        else if (CurrentDisplay == TDisplay.Sacrifice)
            Soldier.ToggleDisplaySacrificeValue();
        else if (Character && Character == SC_Tile_Manager.focusedCharacter)
            tileManager.TryStopPreview(Character);

    }

    public void SetFilter(TDisplay filterName) {

        Color c = new Color();

        foreach (FilterColor fC in filtersColors)
            if (fC.filter == filterName)
                c = fC.color;

        filter.color = c;
        filter.enabled = true;

	}

	public void RemoveFilter() {

        CurrentDisplay = TDisplay.None;

        filter.enabled = false;

	}

    public void ChangeDisplay(TDisplay d) {

        CurrentDisplay = d;

        SetFilter(d);

    }

}
