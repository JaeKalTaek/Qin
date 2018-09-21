using TMPro;
using UnityEngine;

public class SC_Soldier : SC_Character {    

    [Header("Soldiers Variables")]
    [Tooltip("Weapon of this soldier")]
    public SC_Weapon weapon;

    [Tooltip("Cost to create this soldier in a Workshop")]
    public int cost;

    [Tooltip("Energy gained by Qin when he sacrifices this unit")]
    public int sacrificeValue;

    GameObject sacrificeValueText;

    protected override void Start () {

        base.Start();

        sacrificeValueText = Instantiate(Resources.Load<GameObject>("Prefabs/Characters/Components/SacrificeValueText"), transform);

        sacrificeValueText.GetComponent<TextMeshPro>().text = sacrificeValue.ToString();

    }

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

    public void ToggleDisplaySacrificeValue() {

        sacrificeValueText.SetActive(!sacrificeValueText.activeSelf);

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
