﻿using UnityEngine;

public class SC_Soldier : SC_Character {    

    [Header("Soldiers Variables")]
    [Tooltip("Weapon of this soldier")]
    public SC_Weapon weapon;

    [Tooltip("Cost to create this soldier in a Workshop")]
    public int cost;

    public void SetupNew() {

        CanMove = false;

        Tire();

        SC_Qin.ChangeEnergy(cost);

    }

	protected override void TryCheckMovements () {

		if (CanMove) {

            uiManager.StopCancelConstruct();

            base.TryCheckMovements();

		}

	}

	public override bool Hit(int damages, bool saving) {

		base.Hit(damages, saving);

        if (Health <= 0)
            DestroyCharacter();
        else
            UpdateHealth();

        return (Health <= 0);

	}

    public override void DestroyCharacter() {

        base.DestroyCharacter();

        if(isServer)
		    SC_Player.localPlayer.CmdDestroyGameObject (gameObject);

    }

}
