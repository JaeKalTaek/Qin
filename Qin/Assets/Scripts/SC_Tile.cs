using UnityEngine;
using UnityEngine.Networking;
using static SC_Enums;

[System.Serializable]
public class SC_Tile : NetworkBehaviour {

    public TDisplay CurrentDisplay { get; set; }

    public int cost;

    bool MovingCharaQin { get { return !SC_Character.characterToMove.coalition; } }

    public bool CanGoThrough {

        get {

            if (Character)
                return MovingCharaQin != Character.coalition;
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
                return MovingCharaQin == Character.coalition;
            else if (Construction)
                return !MovingCharaQin && (Bastion || Pump);
            else if (Qin)
                return !MovingCharaQin;
            else
                return true;

        }
    }

    public bool Constructable { get { return !name.Contains("Palace") && (!Character || (gameManager.Bastion && Soldier)) && !Construction && !Locked; } }

    public bool Locked { get; set; }

    public SC_Construction Construction { get; set; }

    public SC_Village Village { get { return Construction as SC_Village; } }

    public SC_Bastion Bastion { get { return Construction as SC_Bastion; } }

    public SC_Wall Wall { get { return Construction as SC_Wall; } }

    public SC_Workshop Workshop { get { return Construction as SC_Workshop; } }

    public SC_Pump Pump { get { return Construction as SC_Pump; } }

    public SC_Character Character { get; set; }

    public SC_Soldier Soldier { get { return Character as SC_Soldier; } }

    public bool Qin { get; set; }

    public bool Empty { get { return !Construction && !Character && !Qin; } }

    public bool Palace { get { return name.Contains("Palace"); } }

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

        if ((int)transform.position.x == (gameManager.baseMapPrefab.GetComponent<SC_MapPrefab>().xSize - 1) && (int)transform.position.y == (gameManager.baseMapPrefab.GetComponent<SC_MapPrefab>().ySize - 1) && !isServer) {

            if (gameManager.Player)
                gameManager.Player.CmdFinishLoading();
            else if (SC_Player.localPlayer)
                SC_Player.localPlayer.CmdFinishLoading();
            else
                FindObjectOfType<SC_Player>().CmdFinishLoading();

        }

    }

    public void CursorClick() {

        if (SC_UI_Manager.CanInteract) {

            if (CurrentDisplay == TDisplay.Construct) {                

                gameManager.ConstructAt(this);

            } else if (CurrentDisplay == TDisplay.Movement) {

                uiManager.cancelMovementButton.SetActive(false);

                SC_Player.localPlayer.Busy = true;

                SC_Player.localPlayer.CmdMoveCharacterTo((int)transform.position.x, (int)transform.position.y);

            } else if (CurrentDisplay == TDisplay.Attack) {

                fightManager.RangedAttack = tileManager.TileDistance(SC_Character.attackingCharacter.transform.position, this) > 1;

                SC_Player.localPlayer.CmdPrepareForAttack(fightManager.RangedAttack, gameObject, !SC_Player.localPlayer.qin);

                if (SC_Character.attackingCharacter.IsHero)
                    SC_Character.attackingCharacter.Hero.ChooseWeapon();
                else
                    SC_Player.localPlayer.CmdAttack();

            } else if (CurrentDisplay == TDisplay.Sacrifice) {

                SC_Player.localPlayer.CmdChangeQinEnergy(SC_Qin.Qin.sacrificeValue);

                RemoveFilter();

                Character.CanMove = false;

                SC_Player.localPlayer.CmdDestroyCharacter(Character.gameObject);

            } else if (CurrentDisplay == TDisplay.Resurrection) {

                uiManager.EndQinAction("qinPower");

                SC_Qin.UsePower(transform.position);

            }

        }

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
