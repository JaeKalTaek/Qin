using UnityEngine;

public class SC_Soldier : SC_Character {

    public SC_Weapon weapon;

    protected override void Awake() {

		base.Awake ();
		
        coalition = false;

    }

	protected override void PrintMovements () {

		if (CanMove) {

			uiManager.SetButtonActivated ("construct", true);
            uiManager.SetButtonActivated("sacrifice", true);
            uiManager.SetButtonActivated("qinPower", true);
            uiManager.workshopPanel.gameObject.SetActive (false);

            SC_Player.localPlayer.CmdCheckMovements((int)transform.position.x, (int)transform.position.y);

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
