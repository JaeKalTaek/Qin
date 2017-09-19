using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[System.Serializable]
public class SC_Tile : NetworkBehaviour {

    [HideInInspector]
    public bool displayMovement;
	[HideInInspector]
	public bool /*relationshipRange,*/ constructable;
	bool displayAttack;
    [HideInInspector]
    public bool displayConstructable, displaySacrifice, displayResurrection;
    public int baseCost;
    [HideInInspector]
    public int movementCost;
    [HideInInspector]
    public bool canSetOn;
	[HideInInspector]
	public bool attackable;
	[HideInInspector]
	public SC_Tile parent;

    void Awake() {

        constructable = !(name.Contains("Palace"));

        movementCost = baseCost;

        canSetOn = true;

		attackable = true;

    }
		
    void OnMouseDown() {
        
		if ((displayConstructable) && (((SC_Qin.GetEnergy () - 50) > 0) || SC_GameManager.GetInstance ().IsBastion ())) {

			SC_GameManager.GetInstance ().ConstructAt (transform.position);

			SC_GameManager.GetInstance ().StopConstruction ();

		} else if (displayMovement) {

			SC_Character.GetCharacterToMove ().MoveTo ((int)transform.position.x, (int)transform.position.y);

		} else if (displayAttack) {

			SC_Character attackingCharacter = SC_Character.GetAttackingCharacter ();
			SC_Tile attackingCharacterTile = SC_GameManager.GetInstance ().GetTileAt ((int)attackingCharacter.transform.position.x, (int)attackingCharacter.transform.position.y);
			SC_GameManager.GetInstance ().rangedAttack = !SC_GameManager.GetInstance ().IsNeighbor (attackingCharacterTile, this);

			attackingCharacter.attackTarget = this;

			if (attackingCharacter.isHero ()) {
				
				((SC_Hero)attackingCharacter).ChooseWeapon ();

			} else {
			
				foreach (SC_Tile tile in SC_GameManager.GetInstance().tiles)
					tile.RemoveFilter ();
				
				SC_GameManager.GetInstance ().Attack ();

			}

		} else if (displaySacrifice) {

			SC_Qin.IncreaseEnergy (25);

			RemoveFilter ();

			Destroy (SC_GameManager.GetInstance ().GetCharacterAt (this).gameObject);

		} else if (displayResurrection) {

			SC_GameManager.GetInstance ().HideResurrectionTiles ();

			SC_Qin.UsePower (transform.position);

		}
        
    }

    public void SetFilter(string filterName) {
        
        foreach(SpriteRenderer sprite in GetComponentsInChildren<SpriteRenderer>())
            if (sprite.name.Equals(filterName)) sprite.enabled = true;
  
    }

    public void RemoveFilter() {

        displayMovement = false;
        displayConstructable = false;
        displayAttack = false;
		displaySacrifice = false;
		displayResurrection = false;

        foreach (SpriteRenderer sprite in GetComponentsInChildren<SpriteRenderer>())
            sprite.enabled = false;

    }

    public void DisplayMovement(bool valid) { 

        if(valid) {

            displayMovement = true;
            SetFilter("T_DisplayMovement");

        }

    }

	public bool Qin() {

		Vector3 qinPos = FindObjectOfType<SC_Qin> ().transform.position;
		return ((transform.position.x == qinPos.x) && (transform.position.y == qinPos.y));

	}

	public bool IsEmpty() {

		SC_GameManager gm = SC_GameManager.GetInstance ();
		return ((gm.GetConvoyAt (this) == null) && (gm.GetConstructionAt (this) == null) && (gm.GetCharacterAt (this) == null));

	}

    public bool isPalace() {

        return name.Contains("Palace");

    }

    public bool CanConstructOn() {

		return displayConstructable;

	}

	public void SetCanConstruct(bool c) {

		displayConstructable = c;

	}

	public bool GetDisplayAttack() {

		return displayAttack;

	}

	public void SetDisplayAttack(bool d) {

		displayAttack = d;

	}

}
