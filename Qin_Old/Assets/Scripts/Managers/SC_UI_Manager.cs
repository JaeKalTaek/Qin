using UnityEngine;
using UnityEngine.UI;
using System;

public class SC_UI_Manager : MonoBehaviour {

	[Header("Game")]
	public GameObject loadingPanel;
	public Text turns;
	public GameObject health;
	public GameObject previewFightPanel;
	public GameObject endTurn;
	public GameObject victoryPanel;

	[Header("Characters")]
	public GameObject statsPanel;
	public GameObject cancelMovementButton;
	public GameObject weaponChoice1;
	public GameObject cancelAttackButton;

	[Header("Heroes")]
	public GameObject relationshipPanel;
	public GameObject villagePanel;
	public GameObject weaponChoice2;
	public GameObject usePower;

	[Header("Constructions")]
	public GameObject buildingInfosPanel;

	[Header("Qin")]
	public Text energyText;
	public GameObject qinPanel;
	public Transform construct;
	public Transform qinPower;
	public Transform sacrifice;
	public GameObject workshopPanel;

	public GameObject CurrentGameObject { get; set; }

	static SC_Game_Manager gameManager;

    public SC_Tile_Manager TileManager { get; set; }

    static SC_Fight_Manager fightManager;

    public static SC_UI_Manager Instance { get; set; }

    private void Awake() {

        Instance = this;

    }

    public void SetupUI(bool qin) {       

        gameManager = SC_Game_Manager.Instance;

        TileManager = SC_Tile_Manager.Instance;

        fightManager = SC_Fight_Manager.Instance;


        if (!qin) {

			usePower.SetActive (true);
			endTurn.SetActive (true);

		}

	}

	public void NextTurn(bool coalition, int turn) {

		HideWeapons();

		villagePanel.SetActive (false);
		usePower.SetActive (coalition && !gameManager.Player.IsQin());
		cancelMovementButton.SetActive (false);
		cancelAttackButton.SetActive (false);

        if(!coalition) {

            SetButtonActivated("construct", true);
            SetButtonActivated("sacrifice", true);
            SetButtonActivated("qinPower", true);

        } else {

            construct.gameObject.SetActive(false);
            qinPower.gameObject.SetActive(false);
            sacrifice.gameObject.SetActive(false);

        }

		turns.text = (((turn - 1) % 3) == 0) ? "1st Turn - Coalition" : (((turn - 2) % 3) == 0) ? "2nd Turn - Coalition" : "Turn Qin";

        bool pNotQin = !SC_Player.localPlayer.IsQin();

        endTurn.SetActive(pNotQin == coalition && pNotQin);

	}

	/*public void ToggleButton(string id) {

        foreach(Transform t in (Transform)typeof(SC_UI_Manager).GetField(id).GetValue(this))
            t.gameObject.SetActive(t.gameObject.activeSelf);

	}*/

    public void SetButtonActivated(string id, bool active) {

        ((Transform)typeof(SC_UI_Manager).GetField(id).GetValue(this)).GetChild(0).gameObject.SetActive(active);
        ((Transform)typeof(SC_UI_Manager).GetField(id).GetValue(this)).GetChild(1).gameObject.SetActive(!active);

    }

    public void SetButtonActivated(string b, string id) {

        SetButtonActivated(b, b != id);

    }

    public void ShowHideInfos(GameObject g, Type t) {

		if(HideInfos (g)) {

			if (t == typeof(SC_Hero))
				ShowHeroInfos (g.GetComponent<SC_Hero> ());
			else if (t == typeof(SC_Soldier))
				ShowSoldierInfos (g.GetComponent<SC_Soldier> ());
			else if (t == typeof(SC_Construction))
				ShowConstructionsInfos (g.GetComponent<SC_Construction> ());
			else if (t == typeof(SC_Qin))
				ShowQinInfos (g.GetComponent<SC_Qin> ());
			else
				print ("ERRROR");

		}

	}

	public bool HideInfos(GameObject g) {

		statsPanel.SetActive (false);
		relationshipPanel.SetActive (false);
		buildingInfosPanel.SetActive (false);
		qinPanel.SetActive (false);

        CurrentGameObject = (CurrentGameObject == g) ? null : g;

        return CurrentGameObject;

	}

    public void TryRefreshInfos(GameObject g, Type t) {

        if(CurrentGameObject == g) {

            CurrentGameObject = null;

            ShowHideInfos(g, t);

        }

    }

	void ShowCharacterInfos(SC_Character character) {

		statsPanel.SetActive (true);

		SetText("Name", character.characterName);
		SetText("Health", "Health : " + character.Health + " / " + character.maxHealth);
		SetText("Strength", " Strength : " + character.strength);
		SetText("Armor", " Armor : " + character.armor);
		SetText("Qi", " Qi : " + character.qi);
		SetText("Resistance", " Resistance : " + character.resistance);
		SetText("Technique", " Technique : " + character.technique + " (" + character.CriticalHit + ")");
		SetText("Speed", " Speed : " + character.speed + " (" + character.DodgeHit + ")");
		SetText("Movement", " Movement : " + character.movement);
		SetText("WeaponsTitle", " Weapons :");

	}

	void ShowHeroInfos(SC_Hero hero) {

		ShowCharacterInfos (hero);

		relationshipPanel.SetActive (true);

		SetText("Weapon 1", "  - " + hero.GetWeapon(true).weaponName + " (E)");
		SetText("Weapon 2", "  - " + hero.GetWeapon(false).weaponName);

		for (int i = 0; i < hero.relationshipKeys.Count; i++) {

			int value;
			hero.relationships.TryGetValue(hero.relationshipKeys [i], out value);
			GameObject.Find ("Relation_" + (i + 1)).GetComponent<Text> ().text = "  " + hero.relationshipKeys [i] + " : " + value;

		}


	}

	void ShowSoldierInfos(SC_Soldier soldier) {

		ShowCharacterInfos (soldier);

		SetText("Weapon 1", "  - " + soldier.weapon.weaponName);
		SetText("Weapon 2", "");

	}

	public void ShowWeapon(SC_Weapon weapon, bool first) {

		if (first)
			weaponChoice1.SetActive (true);
		else
			weaponChoice2.SetActive (true);

		SetText("Weapon Choice " + (first ? "1" : "2") + " Text", weapon.weaponName);

	}

	void ShowConstructionsInfos(SC_Construction construction) {

		buildingInfosPanel.SetActive (true);

		SetText("BuildingName", construction.buildingName);
		SetText("BuildingHealth", (construction.GetType ().Equals (typeof(SC_Village))) ? "" : "Health : " + construction.health + " / " + construction.maxHealth);

	}

	void ShowQinInfos(SC_Qin qin) {

		qinPanel.SetActive (true);

		SetText("QinEnergy", SC_Qin.Energy + "");

	}

	public void PreviewFight(SC_Character attacker, bool rangedAttack) {

		previewFightPanel.SetActive (true);

		SetText ("AttackerName", attacker.characterName);

		SetText ("AttackerWeapon", attacker.GetActiveWeapon ().weaponName);

		SetText ("AttackerCrit", attacker.CriticalHit.ToString ());

		SetText ("AttackerDodge", attacker.DodgeHit.ToString ());

		int attackedDamages = 0;

		string attackerDamagesString = "";

		int attackerDamages = attacker.GetActiveWeapon().weaponOrQi ? attacker.strength : attacker.qi;

		string attackedDamagesString = "";

		string attackedName = "";

		string attackedHP = "";

		string attackedWeapon = "";

		string attackedCrit = "";

		string attackedDodge = "";

		if (attacker.AttackTarget.Character) {

			SC_Character attacked = attacker.AttackTarget.Character;

			attackedName = attacked.characterName;

			attackedWeapon = attacked.GetActiveWeapon ().weaponName;

			attackerDamages = fightManager.CalcDamages (attacker, attacked, false);
			attackedDamages = fightManager.CalcDamages (attacked, attacker, true);
			if (!((rangedAttack && attacked.GetActiveWeapon ().ranged) || (!rangedAttack && !attacked.GetActiveWeapon ().IsBow ())))
				attackedDamages = 0;

			attackedHP = (attacked.Health - attackerDamages).ToString ();

			attackerDamagesString = attackerDamages.ToString ();

			attackedDamagesString = attackedDamages.ToString ();

			attackedCrit = attacked.CriticalHit.ToString ();

			attackedDodge = attacked.DodgeHit.ToString ();

		} else {

            SC_Construction attackedConstruction = attacker.AttackTarget.Construction;

            int attackedType = attackedConstruction ? 0 : attacker.AttackTarget.Qin ? 1 : 2;

			attackedName = (attackedType == 0) ? attackedConstruction.buildingName : (attackedType == 1) ? "Qin" : "";			

			int attackedHealth = (attackedType == 0) ? attackedConstruction.health : (attackedType == 1) ? SC_Qin.Energy : 0;

			if (attackedType != 2) attackedHP = (attackedHealth - attackerDamages).ToString ();

		}

		SetText("AttackerHP", (attacker.Health - attackedDamages).ToString());

		SetText("AttackedName", attackedName);

		SetText("AttackedHP", attackedHP);

		SetText("AttackerDamages", attackerDamagesString);
		SetText("AttackedDamages", attackedDamagesString);

		SetText("AttackedWeapon", attackedWeapon);

		SetText("AttackedCrit", attackedCrit);
		SetText("AttackedDodge", attackedDodge);

	}

	public void HideWeapons() {

		weaponChoice1.SetActive (false);
		weaponChoice2.SetActive (false);
		previewFightPanel.SetActive (false);

	}

	public void ShowHeroPower(bool show, string heroName) {

		usePower.SetActive (!show);

		if (show)
			usePower.GetComponentInChildren<Text> ().name = heroName;

	}

	void SetText(string id, string text) {

		GameObject.Find (id).GetComponent<Text> ().text = text;

	}

	public void ShowVictory(bool qinWon) {

        victoryPanel.GetComponentInChildren<Text>().text = (qinWon ? "Qin" : "The Heroes") + " won the war !";

        victoryPanel.SetActive(true);

	}

    public void StartQinAction(string action) {

        SetButtonActivated("construct", action);
        SetButtonActivated("sacrifice", action);
        SetButtonActivated("qinPower", action);

        workshopPanel.SetActive(action == "workshop");

        cancelMovementButton.SetActive(false);

        SC_Tile_Manager.Instance.RemoveAllFilters();

        SC_Player.localPlayer.CmdRemoveAllFiltersOnClient(false);

        SC_Character.CancelAttack();

    }

    public void EndQinAction(string action) {

        SC_Tile_Manager.Instance.RemoveAllFilters();

        SetButtonActivated(action, true);

    }

    public void ToggleHealth() {

        foreach(SC_Lifebar lifebar in FindObjectsOfType<SC_Lifebar>())
            lifebar.Toggle();

    }

    public void ResetAttackChoice () {

        HideWeapons();

        SC_Character.attackingCharacter.CheckAttack();

        cancelMovementButton.SetActive(!gameManager.CantCancelMovement);

        cancelAttackButton.SetActive(false);

    }

    #region Preview Fight
    public void PreviewFight (bool activeWeapon) {

        if (SC_Character.attackingCharacter.IsHero)
            SC_Character.attackingCharacter.Hero.SetWeapon(activeWeapon);

        PreviewFight(SC_Character.attackingCharacter, fightManager.RangedAttack);

        if (SC_Character.attackingCharacter.IsHero)
            SC_Character.attackingCharacter.Hero.SetWeapon(activeWeapon);

    }

    public void HidePreviewFight() {

        previewFightPanel.SetActive(false);

    }
    #endregion

    public void DisplayWorkshopPanel () {

        if (!gameManager.CoalitionTurn && !gameManager.Bastion && !TileManager.GetTileAt(gameManager.CurrentWorkshop.gameObject).Character)
            StartQinAction("workshop");

    }

    public void HideWorkshopPanel () {

        workshopPanel.SetActive(false);

    }

}
