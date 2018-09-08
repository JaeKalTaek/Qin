using UnityEngine;
using UnityEngine.Networking;
using static SC_Enums;

[System.Serializable]
public class SC_Tile : NetworkBehaviour {

    public TDisplay CurrentDisplay { get; set; }

    public int baseCost;

	public int movementCost { get; set; }

	public bool canSetOn { get; set; }

	public bool attackable { get; set; }

    public bool constructable { get; set; }

    public SC_Construction construction { get; set; }

    public SC_Village village { get { return construction as SC_Village; } }

    public SC_Bastion bastion { get { return construction as SC_Bastion; } }

    public SC_Wall wall { get { return bastion as SC_Wall; } }

    public SC_Character character { get; set; }

    public bool qin { get; set; }

    public bool empty { get { return !construction && !character && !qin; } }

    public bool palace { get { return name.Contains("Palace"); } }

    // Used for PathFinder
    public SC_Tile parent { get; set; }

	static SC_GameManager gameManager;

	static SC_Tile_Manager tileManager;

    static SC_UI_Manager uiManager;

	void Awake() {

		constructable = !name.Contains("Palace");

		movementCost = baseCost;

		canSetOn = true;

		attackable = true;

	}

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

            character.SetCanMove(false);

            SC_Player.localPlayer.CmdDestroyCharacter(character.gameObject);

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
