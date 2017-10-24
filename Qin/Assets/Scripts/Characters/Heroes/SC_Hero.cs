using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SC_Hero : SC_Character {

	//Relationships
	[HideInInspector]
	public Dictionary<string, int> relationships;
	[HideInInspector]
	public List<string> relationshipKeys;
	bool saved;

	//Berserk
	[HideInInspector]
	public bool berserk, berserkTurn;

	//Weapons
	public SC_Weapon weapon1, weapon2;

	//power
	[HideInInspector]
	public bool powerUsed;
	[HideInInspector]
	public int powerBacklash;

	Color berserkColor;

	//UI
	/*[HideInInspector]
	public static GameObject villagePanel;
	static GameObject weaponChoice1, weaponChoice2, usePower, cancelAttackButton;*/

	protected override void Awake() {

		base.Awake ();

		berserkColor = new Color (0, .82f, 1);

		coalition = true;

	}

	protected override void Start() {

		base.Start();

		/*if (villagePanel == null)
			villagePanel = GameObject.Find ("VillagePanel");

		if (weaponChoice1 == null)
			weaponChoice1 = GameObject.Find("Weapon Choice 1");

		if (weaponChoice2 == null)
			weaponChoice2 = GameObject.Find("Weapon Choice 2");

		if (usePower == null)
			usePower = GameObject.Find("PowerHero");

		if (cancelAttackButton == null)
			cancelAttackButton = GameObject.Find("CancelAttack");

		villagePanel.SetActive(false);
		weaponChoice1.SetActive(false);
		weaponChoice2.SetActive(false);
		usePower.SetActive(false);
		cancelAttackButton.SetActive(false);*/

		tileManager.SetHero (this);

		//SC_GameManager.GetInstance().GetTileAt((int)transform.position.x, (int)transform.position.y).constructable = false;

		relationships = new Dictionary<string, int> ();

		foreach (SC_Hero hero in FindObjectsOfType<SC_Hero>()) {

			if (!ReferenceEquals (hero, this)) {

				relationships.Add (hero.characterName, 0);
				relationshipKeys.Add (hero.characterName);

			}

		}

	}

	protected override void PrintMovements () {
		
		if ((canMove || (berserk && !berserkTurn)) && (coalition == SC_GameManager.GetInstance().CoalitionTurn())) {

			SC_GameManager.GetInstance ().CheckMovements (this);

			uiManager.usePower.SetActive (!powerUsed);
			if (powerUsed)
				uiManager.usePower.GetComponentInChildren<Text> ().name = name;
			//usePower.SetActive (!powerUsed);
			//if(!powerUsed) usePower.GetComponentInChildren<Text> ().name = name;

		}

	}

	protected override void ShowStatPanel() {

		base.ShowStatPanel();

		uiManager.relationshipPanel.SetActive (true);

		SC_Functions.SetText("WeaponsTitle", " Weapons :");
		SC_Functions.SetText("Weapon 1", "  - " + GetWeapon(true).weaponName + " (E)");
		SC_Functions.SetText("Weapon 2", "  - " + GetWeapon(false).weaponName);

		for (int i = 0; i < relationshipKeys.Count; i++) {

			int value;
			relationships.TryGetValue(relationshipKeys [i], out value);
			GameObject.Find ("Relation_" + (i + 1)).GetComponent<Text> ().text = "  " + relationshipKeys [i] + " : " + value;

		}

		SC_Tile under = tileManager.GetTileAt (gameObject); //SC_GameManager.GetInstance().GetTileAt((int)transform.position.x, (int)transform.position.y);

		if (under.GetDisplayAttack ()) {

			GetAttackingCharacter ().attackTarget = under;

			SC_Tile attackingCharacterTile = tileManager.GetTileAt (GetAttackingCharacter ().gameObject); //SC_GameManager.GetInstance().GetTileAt((int)GetAttackingCharacter ().transform.position.x, (int)GetAttackingCharacter ().transform.position.y);

			SC_GameManager.GetInstance().rangedAttack = !SC_GameManager.GetInstance().IsNeighbor(attackingCharacterTile, under);

			SC_GameManager.GetInstance ().PreviewFight (true);

		}

	}

	void OnMouseEnter() {

		SC_Tile under = tileManager.GetTileAt (gameObject); //SC_GameManager.GetInstance().GetTileAt((int)transform.position.x, (int)transform.position.y);

		if (under.GetDisplayAttack() && !GetAttackingCharacter().isHero()) {

			GetAttackingCharacter().attackTarget = under;

			SC_GameManager.GetInstance().PreviewFight(false);

			GetAttackingCharacter().attackTarget = null;

		}

	}

	void OnMouseExit() {

		SC_GameManager.GetInstance().HidePreviewFight();

	}

	public void ChooseWeapon() {

		uiManager.cancelMovementButton.SetActive (false);
		uiManager.cancelAttackButton.SetActive (true);
		//cancelMovementButton.SetActive (false);
		//cancelAttackButton.SetActive (true);

		foreach (SC_Tile tile in SC_GameManager.GetInstance().tiles)
			tile.RemoveFilter();

		if(SC_GameManager.GetInstance().rangedAttack) {

			if(weapon1.ranged) {

				uiManager.weaponChoice1.SetActive (true);
				//weaponChoice1.SetActive(true);
				SC_Functions.SetText("Weapon Choice 1 Text", GetWeapon(true).weaponName);

			}

			if(weapon2.ranged) {

				uiManager.weaponChoice2.SetActive (true);
				//weaponChoice2.SetActive(true);
				SC_Functions.SetText("Weapon Choice 2 Text", GetWeapon(false).weaponName);

			}

		} else {

			if(!weapon1.IsBow()) {

				uiManager.weaponChoice1.SetActive (true);
				//weaponChoice1.SetActive(true);
				SC_Functions.SetText("Weapon Choice 1 Text", GetWeapon(true).weaponName);

			}

			if(!weapon2.IsBow()) {

				uiManager.weaponChoice2.SetActive (true);
				//weaponChoice2.SetActive(true);
				SC_Functions.SetText("Weapon Choice 2 Text", GetWeapon(false).weaponName);

			}

		}

	}

	public static void HideWeapons() {

		uiManager.weaponChoice1.SetActive (false);
		uiManager.weaponChoice2.SetActive (false);
		/*weaponChoice1.SetActive (false);
		weaponChoice2.SetActive (false);*/
		SC_GameManager.GetInstance ().HidePreviewFight ();

	}

	public static void HidePower() {

		uiManager.usePower.SetActive (false);
		//usePower.SetActive (false);

	}

	public void ActionVillage(bool destroy) {

		//SC_GameManager.GetInstance ().GetTileAt ((int)transform.position.x, (int)transform.position.y);

		if (destroy) {

			SC_GameManager.GetInstance ().cantCancelMovement = true;
			tileManager.GetAt<SC_Village> (gameObject).DestroyConstruction ();
			//((SC_Village)SC_GameManager.GetInstance ().GetConstructionAt (pos)).DestroyConstruction ();

		} else {

			uiManager.cancelMovementButton.SetActive (true);
			//cancelMovementButton.SetActive (true);

		}

		uiManager.villagePanel.SetActive (false);
		//villagePanel.SetActive (false);

		SC_GameManager.GetInstance().CheckAttack(this);

	}

	public void Regen() {

		//SC_Tile pos = SC_GameManager.GetInstance ().GetTileAt ((int)transform.position.x, (int)transform.position.y);

		if (/*SC_GameManager.GetInstance ().GetConstructionAt (pos)*/ tileManager.GetAt<SC_Village>(gameObject) != null) {

			//if (SC_GameManager.GetInstance ().GetConstructionAt (pos).GetType ().Equals (typeof(SC_Village))) {

				health = ((health + 10) > maxHealth) ? maxHealth : (health + 10);
				lifebar.UpdateGraph(health, maxHealth);

			//}

		}

	}

	public override bool Hit(int damages, bool saving) {

		bool dead = false;

		base.Hit(damages, saving);

		if (health <= 0) {

			if (saving) {

				health = 1;
				berserk = true;
				berserkTurn = true;

				GetComponent<Renderer> ().material.color = Color.cyan;

			} else {

				SC_Hero saver = SC_GameManager.GetInstance ().CheckHeroSaved (this, saved);

				if (saver != null) {

					saver.Hit (damages, true);
					saved = true;
					health += damages;

				} else {

					DestroyCharacter();
					dead = true;

				}

			}

		} else if (health <= Mathf.CeilToInt ((float)(maxHealth * 0.2))) {

			canMove = (SC_GameManager.GetInstance().CoalitionTurn());

			berserkTurn = true;

			if(!berserk)
				GetComponent<Renderer> ().material.color = Color.cyan;

			berserk = true;

		}

		if (!dead) {

			lifebar.UpdateGraph (health, maxHealth);
			if (selfPanel) ShowStatPanel ();

		}

		return dead;

	}

	public override void Tire() {

		if (!berserk || berserkTurn) base.Tire ();

	}

	public override void UnTired() {

		if (berserk)
			GetComponent<SpriteRenderer> ().color = berserkColor;
		else
			base.UnTired ();

	}

	public override void DestroyCharacter() {

		base.DestroyCharacter();

		SC_Qin.ChangeEnergy (500);

		SC_GameManager.GetInstance ().lastHeroDead = this;

		tileManager.GetTileAt (gameObject).constructable = !tileManager.GetTileAt (gameObject).isPalace ();

		/*int x = (int)transform.position.x;
		int y = (int)transform.position.y;

		SC_GameManager.GetInstance ().GetTileAt (x, y).constructable = !SC_GameManager.GetInstance ().GetTileAt (x, y).isPalace();*/

		foreach (SC_Hero hero in FindObjectsOfType<SC_Hero>()) {

			int value = 0;
			relationships.TryGetValue (hero.characterName, out value);

			if (value >= 200) {

				hero.berserk = true;
				hero.berserkTurn = true;
				hero.canMove = (SC_GameManager.GetInstance().CoalitionTurn());

				hero.GetComponent<Renderer> ().material.color = Color.cyan;

			}

		}

		gameObject.SetActive (false);

	}

	public SC_Weapon GetWeapon(bool active) {

		return active ? weapon1 : weapon2;

	}

	public void SetWeapon(bool activeWeaponUsed) {

		if(!activeWeaponUsed) {

			SC_Weapon temp = GetWeapon(true);
			weapon1 = weapon2;
			weapon2 = temp;

		}

	}

	public static void HideCancelAttack() {

		uiManager.cancelAttackButton.SetActive (false);
		//cancelAttackButton.SetActive (false);

	}

}
