using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.Networking;

public class SC_UI_Manager : MonoBehaviour {

	[Header("Game")]
	public GameObject loadingPanel;
	public Text turns;
	public Transform health;
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

	GameObject currentGameObject;

	static SC_GameManager gameManager;
	static SC_Tile_Manager tileManager;

	/*public void SetupUI(SC_Player p) {

		if (gameManager == null)
			gameManager = GetComponent<SC_GameManager> ();

		if (tileManager == null)
			tileManager = GetComponent<SC_Tile_Manager> ();

		player = p;

		if (!player.IsQin()) {

			usePower.SetActive (true);
			endTurn.SetActive (true);

		}

	}*/

	public void SetupUI(bool qin) {

		if (gameManager == null)
			gameManager = FindObjectOfType<SC_GameManager> ();

		if (tileManager == null)
			tileManager = FindObjectOfType<SC_Tile_Manager> ();

		if (!qin) {

			usePower.SetActive (true);
			endTurn.SetActive (true);

		}

	}

	public void NextTurn(bool coalition, int turn) {

		HideWeapons();

		villagePanel.SetActive (false);
		usePower.SetActive (coalition && !gameManager.player.IsQin());
		cancelMovementButton.SetActive (false);
		cancelAttackButton.SetActive (false);

		if (coalition) {

			construct.gameObject.SetActive (false);
			qinPower.gameObject.SetActive (false);
			sacrifice.gameObject.SetActive (false);

		}

		/*if(coalition)
			foreach (string s in new string[] { "construct", "qinPower", "sacrifice" })
				Hide (s);*/

		turns.text = (((turn - 1) % 3) == 0) ? "1st Turn - Coalition" : (((turn - 2) % 3) == 0) ? "2nd Turn - Coalition" : "Turn Qin";

	}

	public void ToggleButton(string id) {

		Transform parent = (Transform)typeof(SC_UI_Manager).GetField (id).GetValue(this);
		bool turnedOn = parent.GetChild (0).gameObject.activeSelf;
		parent.GetChild (0).gameObject.SetActive (!turnedOn);
		parent.GetChild (1).gameObject.SetActive (turnedOn);

	}

	/*public void Hide(string id) {

		((GameObject)typeof(SC_UI_Manager).GetField (id).GetValue (this)).SetActive (false);

		//Transform parent = (Transform)typeof(SC_UI_Manager).GetField (id).GetValue(this);		
		//parent.GetChild (0).gameObject.SetActive (false);
		//parent.GetChild (1).gameObject.SetActive (false);

	}

	public void Show(string id) {

		((GameObject)typeof(SC_UI_Manager).GetField (id).GetValue (this)).SetActive (true);

		//Transform parent = (Transform)typeof(SC_UI_Manager).GetField (id).GetValue(this);
		//parent.GetChild (0).gameObject.SetActive (true);

	}*/

	public void ShowHideInfos(GameObject g, Type t) {

		if(!HideInfos (g)) {

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

		if (currentGameObject == g) {

			currentGameObject = null;

			return true;

		} else {

			currentGameObject = g;

			return false;

		}

	}

	void ShowCharacterInfos(SC_Character character) {

		statsPanel.SetActive (true);

		SetText("Name", character.characterName);
		SetText("Health", "Health : " + character.health + " / " + character.maxHealth);
		SetText("Strength", " Strength : " + character.strength);
		SetText("Armor", " Armor : " + character.armor);
		SetText("Qi", " Qi : " + character.qi);
		SetText("Resistance", " Resistance : " + character.resistance);
		SetText("Technique", " Technique : " + character.technique + " (" + character.criticalHit + ")");
		SetText("Speed", " Speed : " + character.speed + " (" + character.dodgeHit + ")");
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

	public void UpdateCharacterHealth(GameObject g) {

		if(currentGameObject == g)
			SetText("Health","Health : " + health + " / " + g.GetComponent<SC_Character>().maxHealth);

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

	public void UpdateBuildingHealth(GameObject g) {

		if (currentGameObject == g) {

			SC_Construction construction = g.GetComponent<SC_Construction> ();

			SetText ("BuildingHealth", (construction.GetType ().Equals (typeof(SC_Village))) ? "" : "Health : " + construction.health + " / " + construction.maxHealth);

		}

	}

	void ShowQinInfos(SC_Qin qin) {

		qinPanel.SetActive (true);

		SetText("QinEnergy", SC_Qin.GetEnergy() + "");

	}

	void UpdateQinEnergy(GameObject g) {

		if(currentGameObject == g)
			SetText("QinEnergy", SC_Qin.GetEnergy() + "");

	}

	public void PreviewFight(SC_Character attacker, bool rangedAttack) {

		previewFightPanel.SetActive (true);

		SetText ("AttackerName", attacker.characterName);

		SetText ("AttackerWeapon", attacker.GetActiveWeapon ().weaponName);

		SetText ("AttackerCrit", attacker.criticalHit.ToString ());

		SetText ("AttackerDodge", attacker.dodgeHit.ToString ());

		int attackedDamages = 0;

		string attackerDamagesString = "";

		int attackerDamages = attacker.GetActiveWeapon().weaponOrQi ? attacker.strength : attacker.qi;

		string attackedDamagesString = "";

		string attackedName = "";

		string attackedHP = "";

		string attackedWeapon = "";

		string attackedCrit = "";

		string attackedDodge = "";

		if (tileManager.GetAt<SC_Character> (attacker.attackTarget) != null) {

			SC_Character attacked = tileManager.GetAt<SC_Character> (attacker.attackTarget);

			attackedName = attacked.characterName;

			attackedWeapon = attacked.GetActiveWeapon ().weaponName;

			attackerDamages = gameManager.CalcDamages (attacker, attacked, false);
			attackedDamages = gameManager.CalcDamages (attacked, attacker, true);
			if (!((rangedAttack && attacked.GetActiveWeapon ().ranged) || (!rangedAttack && !attacked.GetActiveWeapon ().IsBow ())))
				attackedDamages = 0;

			attackedHP = (attacked.health - attackerDamages).ToString ();

			attackerDamagesString = attackerDamages.ToString ();

			attackedDamagesString = attackedDamages.ToString ();

			attackedCrit = attacked.criticalHit.ToString ();

			attackedDodge = attacked.dodgeHit.ToString ();

		} else {

			int attackedType = (tileManager.GetAt<SC_Construction> (attacker.attackTarget) != null) ? 0 : attacker.attackTarget.Qin () ? 1 : 2;

			attackedName = (attackedType == 0) ? tileManager.GetAt<SC_Construction> (attacker.attackTarget).buildingName : (attackedType == 1) ? "Qin" : "";			

			int attackedHealth = (attackedType == 0) ? tileManager.GetAt<SC_Construction> (attacker.attackTarget).health : (attackedType == 1) ? SC_Qin.GetEnergy () : 0;

			if (attackedType != 2) attackedHP = (attackedHealth - attackerDamages).ToString ();

		}

		SetText("AttackerHP", (attacker.health - attackedDamages).ToString());

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

		SetText ("Victory_Text", (qinWon ? "Qin" : "The Heroes") + " won the war !");
		victoryPanel.SetActive (true);

	}

}
