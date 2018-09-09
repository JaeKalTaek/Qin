using System.Collections.Generic;
using UnityEngine;
using static SC_Enums;

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

	static int heroesAlive;

	protected override void Awake() {

		base.Awake ();

		berserkColor = new Color (0, .82f, 1);

		coalition = true;

	}

	protected override void Start() {

		base.Start();

		relationships = new Dictionary<string, int> ();

		foreach (SC_Hero hero in FindObjectsOfType<SC_Hero>()) {

			if (!ReferenceEquals (hero, this)) {

				relationships.Add (hero.characterName, 0);
				relationshipKeys.Add (hero.characterName);

			}

		}

		heroesAlive++;

	}

	/*protected override void OnMouseDown () {

		if(!gameManager.player.IsQin())
			base.OnMouseDown ();

	}*/

	protected override void PrintMovements () {

		if (CanMove || (berserk && !berserkTurn)) {

            SC_Player.localPlayer.CmdCheckMovements((int)transform.position.x, (int)transform.position.y);

            uiManager.ShowHeroPower (powerUsed, name);

		}

	}

	void OnMouseEnter() {

		SC_Tile under = tileManager.GetTileAt (gameObject);

		if ((under.CurrentDisplay == TDisplay.Attack) && !attackingCharacter.IsHero) {

            attackingCharacter.AttackTarget = under;

			uiManager.PreviewFight(false);

            attackingCharacter.AttackTarget = null;

		}

	}

	void OnMouseExit() {

		uiManager.previewFightPanel.SetActive (false);

	}

	public void ChooseWeapon() {

		uiManager.cancelMovementButton.SetActive (false);
		uiManager.cancelAttackButton.SetActive (true);

		if ((fightManager.RangedAttack && weapon1.ranged) || (!fightManager.RangedAttack && !weapon1.IsBow ()))
			uiManager.ShowWeapon (GetWeapon (true), true);

		if ((fightManager.RangedAttack && weapon2.ranged) || (!fightManager.RangedAttack && !weapon2.IsBow ()))
			uiManager.ShowWeapon (GetWeapon (false), false);

	}

    public static void Attack(bool usedActiveWeapon) {

        ((SC_Hero)attackingCharacter).SetWeapon(usedActiveWeapon);

        fightManager.Attack();

    }    

	public void Regen() {

		if (tileManager.GetTileAt(gameObject).Village) {

			Health = Mathf.Min (Health + gameManager.commonCharactersVariables.villageRegen, maxHealth);
            UpdateHealth();

        }

	}

	public override bool Hit(int damages, bool saving) {

		bool dead = false;

		base.Hit(damages, saving);

		if (Health <= 0) {

			if (saving) {

                Health = gameManager.commonCharactersVariables.savedHealthAmount;
				berserk = true;
				berserkTurn = true;

				GetComponent<Renderer> ().material.color = Color.cyan;

			} else {

				SC_Hero saver = fightManager.CheckHeroSaved (this, saved);

				if (saver) {

					saver.Hit (damages, true);
					saved = true;
					Health += damages;

				} else {

					DestroyCharacter();
					dead = true;

				}

			}

		} else if (Health <= gameManager.commonCharactersVariables.berserkTriggerHealth) {

			CanMove = gameManager.CoalitionTurn;

			berserkTurn = true;

			if(!berserk)
				GetComponent<Renderer> ().material.color = Color.cyan;

			berserk = true;

		}

        if (!dead)
            UpdateHealth();

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

		SC_Qin.ChangeEnergy (SC_Qin.Qin.energyWhenHeroDies);

		gameManager.LastHeroDead = this;        

		foreach (SC_Hero hero in FindObjectsOfType<SC_Hero>()) {

			int value = 0;
			relationships.TryGetValue (hero.characterName, out value);

			if (value >= gameManager.commonCharactersVariables.berserkTriggerRelation) {

				hero.berserk = true;
				hero.berserkTurn = true;
				hero.CanMove = gameManager.CoalitionTurn;

				hero.GetComponent<Renderer> ().material.color = Color.cyan;

			}

		}

		gameObject.SetActive (false);

		heroesAlive--;

		if (heroesAlive <= 0)
			uiManager.ShowVictory (true);

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
