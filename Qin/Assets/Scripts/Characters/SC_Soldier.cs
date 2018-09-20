using UnityEngine;

public class SC_Soldier : SC_Character {

    public SC_Weapon weapon;

    [Header("Variables for Soldiers")]
    [Tooltip("Cost to create this soldier in a Workshop")]
    public int cost;

    protected override void Awake() {

		base.Awake ();
		
        qin = true;

    }

    public void SetupNew() {

        CanMove = false;

        Tire();

        SC_Qin.ChangeEnergy(cost);

    }

	protected override void PrintMovements () {

		if (CanMove) {

            /*uiManager.SetButtonActivated ("construct", true);
            uiManager.SetButtonActivated("sacrifice", true);
            uiManager.SetButtonActivated("qinPower", true);
            uiManager.workshopPanel.gameObject.SetActive (false);*/

            base.PrintMovements();

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
