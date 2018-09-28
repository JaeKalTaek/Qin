using UnityEngine;
using UnityEngine.Networking;
using static SC_Enums;

[System.Serializable]
public class SC_Tile : NetworkBehaviour {

    public TDisplay CurrentDisplay { get; set; }

    public int cost;

    bool MovingCharaQin { get { return SC_Character.characterToMove.Qin; } }

    public bool CanGoThrough {

        get {

            if (Character)
                return MovingCharaQin == Character.Qin;
            else if (Construction)
                return (MovingCharaQin || !Bastion) && !Pump;
            else if (Qin)
                return MovingCharaQin;
            else
                return true;

        }

    }

    public bool CanSetOn {

        get {

            if (Character || Qin)
                return false;
            else if (Construction)
                return (MovingCharaQin || !Bastion) && !Pump;
            else
                return true;

        }

    }

	public bool Attackable {

        get {

            if (Character)
                return MovingCharaQin != Character.Qin;
            else if (Construction)
                return !MovingCharaQin && (Bastion || Pump);
            else if (Qin)
                return !MovingCharaQin;
            else
                return true;

        }
    }

    public bool Constructable { get { return !name.Contains("Palace") && (!Character || (gameManager.QinTurnBeginning && Soldier)) && !Construction; } }

    public SC_Construction Construction { get; set; }

    public SC_Village Village { get { return Construction as SC_Village; } }

    public SC_Bastion Bastion { get { return Construction as SC_Bastion; } }

    public SC_Wall Wall { get { return Construction as SC_Wall; } }

    public SC_Workshop Workshop { get { return Construction as SC_Workshop; } }

    public SC_Pump Pump { get { return Construction as SC_Pump; } }

    public bool ProductionBuilding { get { return Village || Workshop; } }

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

	void Start() {

        if(!gameManager)
            gameManager = SC_Game_Manager.Instance;

        if(!tileManager)
            tileManager = SC_Tile_Manager.Instance;

        if(!uiManager)
            uiManager = SC_UI_Manager.Instance;

        if (!fightManager)
            fightManager = SC_Fight_Manager.Instance;

        if (Mathf.RoundToInt(transform.position.x) == (gameManager.baseMapPrefab.GetComponent<SC_MapPrefab>().xSize - 1) && Mathf.RoundToInt(transform.position.y) == (gameManager.baseMapPrefab.GetComponent<SC_MapPrefab>().ySize - 1) && !isServer)
            gameManager.StartCoroutine("FinishLoading");

    }

    public void CursorClick() {

        if (SC_UI_Manager.CanInteract) {

            if (CurrentDisplay == TDisplay.Construct) {                

                gameManager.ConstructAt(this);

            } else if (CurrentDisplay == TDisplay.Movement) {

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

            } else if (CurrentDisplay == TDisplay.Resurrection) {

                uiManager.EndQinAction("qinPower");

                SC_Qin.UsePower(transform.position);

            } else if (CurrentDisplay == TDisplay.None && SC_UI_Manager.CanInteract && !SC_Player.localPlayer.Busy) {

                if (Character && (Character.Qin == SC_Player.localPlayer.Qin))
                    Character.TryCheckMovements();
                else if (Workshop && SC_Player.localPlayer.Qin)
                    Workshop.SelectWorkshop();

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

    }

    public void OnCursorExit() {

        CursorOn = false;

        if (CurrentDisplay == TDisplay.Attack && Hero)
            uiManager.HidePreviewFight();

        if (CurrentDisplay == TDisplay.Sacrifice)
            Soldier.ToggleDisplaySacrificeValue();

    }

    public void SetFilter(TDisplay filterName) {

		foreach(SpriteRenderer sprite in GetComponentsInChildren<SpriteRenderer>())
			sprite.enabled = sprite.name.Equals("T_Display" + filterName);

	}

	public void RemoveFilter() {

        CurrentDisplay = TDisplay.None;

		foreach (SpriteRenderer sprite in GetComponentsInChildren<SpriteRenderer>())
			sprite.enabled = false;

	}

    public void ChangeDisplay(TDisplay d) {

        CurrentDisplay = d;

        SetFilter(d);

    }

}
