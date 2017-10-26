﻿using System;
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

	protected override void Awake() {

		base.Awake ();

		berserkColor = new Color (0, .82f, 1);

		coalition = true;

	}

	protected override void Start() {

		base.Start();

		tileManager.SetHero (this);

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

		}

	}

	void OnMouseEnter() {

		SC_Tile under = tileManager.GetTileAt (gameObject);

		if (under.GetDisplayAttack() && !GetAttackingCharacter().isHero()) {

			GetAttackingCharacter().attackTarget = under;

			SC_GameManager.GetInstance().PreviewFight(false);

			GetAttackingCharacter().attackTarget = null;

		}

	}

	void OnMouseExit() {

		uiManager.previewFightPanel.SetActive (false);

	}

	public void ChooseWeapon() {

		uiManager.cancelMovementButton.SetActive (false);
		uiManager.cancelAttackButton.SetActive (true);

		foreach (SC_Tile tile in SC_GameManager.GetInstance().tiles)
			tile.RemoveFilter();

		if(SC_GameManager.GetInstance().rangedAttack) {

			if(weapon1.ranged)
				uiManager.ShowWeapon (GetWeapon (true), true);

			if(weapon2.ranged)
				uiManager.ShowWeapon (GetWeapon (false), false);

		} else {

			if(!weapon1.IsBow())
				uiManager.ShowWeapon (GetWeapon (true), true);

			if(!weapon2.IsBow())
				uiManager.ShowWeapon (GetWeapon (false), false);

		}

	}

	public static void HideWeapons() {

		uiManager.weaponChoice1.SetActive (false);
		uiManager.weaponChoice2.SetActive (false);
		uiManager.previewFightPanel.SetActive (false);

	}

	public void ActionVillage(bool destroy) {

		if (destroy) {

			SC_GameManager.GetInstance ().cantCancelMovement = true;
			tileManager.GetAt<SC_Village> (gameObject).DestroyConstruction ();

		} else {

			uiManager.cancelMovementButton.SetActive (true);

		}

		uiManager.villagePanel.SetActive (false);

		SC_GameManager.GetInstance().CheckAttack(this);

	}

	public void Regen() {

		if (tileManager.GetAt<SC_Village>(gameObject) != null) {

			health = ((health + 10) > maxHealth) ? maxHealth : (health + 10);
			lifebar.UpdateGraph(health, maxHealth);

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
			uiManager.UpdateCharacterHealth (gameObject);

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

}
