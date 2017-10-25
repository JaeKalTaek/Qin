using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class SC_UI_Manager : MonoBehaviour {

	[Header("Game")]
	public Text turns;
	public Transform health;
	public GameObject previewFightPanel;
	public GameObject endTurn;

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

	SC_Player player;

	GameObject currentGameObject;

	public void SetupUI(SC_Player p, bool qin) {

		player = p;

		if (!player.IsQin()) {

			usePower.SetActive (true);
			endTurn.SetActive (true);

		}

	}

	public void NextTurn() {

		/*constructWallButton.SetActive (false);
		endConstructionButton.SetActive (false);
		powerQinButton.SetActive (false);
		cancelPowerQinButton.SetActive (false);
		sacrificeUnitButton.SetActive (false);
		cancelSacrificeButton.SetActive (false);

		if (player.Turn ()) {

			endTurn.SetActive (true);

			if (!player.IsQin ()) {

				usePower.SetActive (true);

			}

		}*/

	}

	public void ToggleButton(string id) {

		Transform parent = (Transform)typeof(SC_UI_Manager).GetField (id).GetValue(this);
		bool turnedOn = !parent.GetChild (0).gameObject.activeSelf;
		parent.GetChild (0).gameObject.SetActive (turnedOn);
		parent.GetChild (1).gameObject.SetActive (!turnedOn);

	}

	public void HideButton(string id) {

		Transform parent = (Transform)typeof(SC_UI_Manager).GetField (id).GetValue(this);
		parent.GetChild (0).gameObject.SetActive (false);
		parent.GetChild (1).gameObject.SetActive (false);

	}

	public void ShowButton(string id) {

		Transform parent = (Transform)typeof(SC_UI_Manager).GetField (id).GetValue(this);
		parent.GetChild (0).gameObject.SetActive (true);

	}

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
		SetText("Health","Health : " + health + " / " + character.maxHealth);
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

	void ShowConstructionsInfos(SC_Construction construction) {

		buildingInfosPanel.SetActive (true);

		SetText("BuildingName", construction.buildingName);
		SetText("BuildingHealth", (construction.GetType ().Equals (typeof(SC_Village))) ? "" : "Health : " + health + " / " + construction.maxHealth);

	}

	public void UpdateBuildingHealth(GameObject g) {

		if (currentGameObject == g) {

			SC_Construction construction = g.GetComponent<SC_Construction> ();

			SetText ("BuildingHealth", (construction.GetType ().Equals (typeof(SC_Village))) ? "" : "Health : " + health + " / " + construction.maxHealth);

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

	public void SetTurnText(int turn) {

		turns.text = (((turn - 1) % 3) == 0) ? "1st Turn - Coalition" : (((turn - 2) % 3) == 0) ? "2nd Turn - Coalition" : "Turn Qin";

	}

	void SetText(string id, string text) {

		GameObject.Find (id).GetComponent<Text> ().text = text;

	}

}
