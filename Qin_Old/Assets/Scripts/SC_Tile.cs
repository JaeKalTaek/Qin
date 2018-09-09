using UnityEngine;
using UnityEngine.Networking;
using static SC_Enums;

[System.Serializable]
public class SC_Tile : NetworkBehaviour {

    public TDisplay CurrentDisplay { get; set; }

    public int cost;

    public bool CanGoThrough {

        get {

            if (Character)
                return SC_Character.characterToMove.coalition == Character.coalition;
            else if (Construction)
                return !SC_Character.characterToMove.coalition || !Bastion;
            else if (Qin)
                return !SC_Character.characterToMove.coalition;
            else
                return true;

        }

    }

    public bool CanSetOn {

        get {

            if (Character || Qin)
                return false;
            else if (Construction)
                return SC_Character.characterToMove.coalition ? Village : Village || Bastion;
            else
                return true;

        }

    }

	public bool Attackable {

        get {

            if (Character)
                return SC_Character.attackingCharacter.coalition != Character.coalition;
            else if (Construction)
                return SC_Character.attackingCharacter.coalition && Bastion;
            else if (Qin)
                return SC_Character.attackingCharacter.coalition;
            else
                return true;

        }
    }

    public bool Constructable { get { return !name.Contains("Palace") && (!Character || Soldier) && (!Construction || gameManager.Bastion && Wall); } }

    public SC_Construction Construction { get; set; }

    public SC_Village Village { get { return Construction as SC_Village; } }

    public SC_Bastion Bastion { get { return Construction as SC_Bastion; } }

    public SC_Wall Wall { get { return Construction as SC_Wall; } }

    public SC_Character Character { get; set; }

    public SC_Soldier Soldier { get { return Character as SC_Soldier; } }

    public bool Qin { get; set; }

    public bool Empty { get { return !Construction && !Character && !Qin; } }

    public bool Palace { get { return name.Contains("Palace"); } }

    // Used for PathFinder
    public SC_Tile Parent { get; set; }

	static SC_GameManager gameManager;

	static SC_Tile_Manager tileManager;

    static SC_UI_Manager uiManager;

	void Start() {

        if(!gameManager)
            gameManager = SC_GameManager.Instance;

        if(!tileManager)
            tileManager = SC_Tile_Manager.Instance;

        if(!uiManager)
            uiManager = SC_UI_Manager.Instance;

	}

    void OnMouseDown() {

        if ((CurrentDisplay == TDisplay.Construct) && ((SC_Qin.Energy > SC_Qin.Qin.wallCost) || gameManager.Bastion)) {

            gameManager.ConstructAt(this);

        } else if (CurrentDisplay == TDisplay.Movement) {

            SC_Player.localPlayer.CmdMoveCharacterTo((int)transform.position.x, (int)transform.position.y);

        } else if (CurrentDisplay == TDisplay.Attack) {

            SC_Player.localPlayer.CmdPrepareForAttack(!tileManager.IsNeighbor(tileManager.GetTileAt(SC_Character.attackingCharacter.gameObject), this), gameObject);

            if(SC_Character.attackingCharacter.IsHero())
                ((SC_Hero)SC_Character.attackingCharacter).ChooseWeapon();
            else
                SC_Player.localPlayer.CmdAttack();

        } else if (CurrentDisplay == TDisplay.Sacrifice) {

            SC_Player.localPlayer.CmdChangeQinEnergy(SC_Qin.Qin.sacrificeValue);

            RemoveFilter();

            Character.SetCanMove(false);

            SC_Player.localPlayer.CmdDestroyCharacter(Character.gameObject);

        } else if (CurrentDisplay == TDisplay.Resurrection) {

            uiManager.EndQinAction("qinPower");

            SC_Qin.UsePower(transform.position);

        }

	}

	public void SetFilter(string filterName) {

		foreach(SpriteRenderer sprite in GetComponentsInChildren<SpriteRenderer>())
			sprite.enabled = sprite.name.Equals(filterName);

	}

	public void RemoveFilter() {

        CurrentDisplay = TDisplay.None;

		foreach (SpriteRenderer sprite in GetComponentsInChildren<SpriteRenderer>())
			sprite.enabled = false;

	}

    public void ChangeDisplay(TDisplay d) {

        CurrentDisplay = d;

        SetFilter("T_Display" + d);

    }

}
