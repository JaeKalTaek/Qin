﻿using UnityEngine;
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

            SC_Character chara = tileManager.GetAt<SC_Character>(this);

            SC_Player.localPlayer.CmdChangeQinEnergy(SC_Qin.Qin.sacrificeValue);

            RemoveFilter();

            chara.SetCanMove(false);

            SC_Player.localPlayer.CmdDestroyCharacter(chara.gameObject);

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

	public bool Qin() {

        return tileManager.GetAt<SC_Qin>(this) != null;

	}

	public bool IsEmpty() {

		return tileManager.GetAt<MonoBehaviour> (this) == null;

	}

	public bool IsPalace() {

		return name.Contains("Palace");

	}

}
