using System.Collections.Generic;
using UnityEngine;
using static SC_Enums;

public class SC_Hero : SC_Character {

	//Relationships	
	public Dictionary<string, int> Relationships { get; set; }
	public List<string> RelationshipKeys { get; set; }

	bool saved;

	//Berserk	
	public bool Berserk { get; set; }
    public bool BerserkTurn { get; set; }

	//Weapons
	public SC_Weapon weapon1, weapon2;

	//power	
	public bool PowerUsed { get; set; }
	public int PowerBacklash { get; set; }

	Color berserkColor;

	static int heroesAlive;

    public bool ReadyToRegen { get; set; }

	protected override void Awake() {

		base.Awake ();

		berserkColor = new Color (0, .82f, 1);

		coalition = true;

	}

	protected override void Start() {

		base.Start();

		Relationships = new Dictionary<string, int> ();
        RelationshipKeys = new List<string>();

        foreach (SC_Hero hero in FindObjectsOfType<SC_Hero>()) {

			if (!ReferenceEquals (hero, this)) {

				Relationships.Add (hero.characterName, 0);
				RelationshipKeys.Add (hero.characterName);

			}

		}

		heroesAlive++;

	}

	protected override void PrintMovements () {

		if (CanMove || (Berserk && !BerserkTurn)) {

            base.PrintMovements();

            uiManager.ShowHeroPower (PowerUsed, name);

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

		uiManager.resetMovementButton.SetActive (false);
		uiManager.resetAttackChoiceButton.SetActive (true);

		if ((fightManager.RangedAttack && weapon1.ranged) || (!fightManager.RangedAttack && !weapon1.IsBow))
			uiManager.ShowWeapon (GetWeapon (true), true);

		if ((fightManager.RangedAttack && weapon2.ranged) || (!fightManager.RangedAttack && !weapon2.IsBow))
			uiManager.ShowWeapon (GetWeapon (false), false);

	}

    public static void Attack(bool usedActiveWeapon) {

        ((SC_Hero)attackingCharacter).SetWeapon(usedActiveWeapon);

        fightManager.Attack();

    }    

	public void Regen() {

		if (tileManager.GetTileAt(gameObject).Village) {

            if (ReadyToRegen) {

                Health = Mathf.Min(Health + gameManager.commonCharactersVariables.villageRegen, maxHealth);
                UpdateHealth();

            } else {

                ReadyToRegen = true;

            }

        }

	}

	public override bool Hit(int damages, bool saving) {

		bool dead = false;

		base.Hit(damages, saving);

		if (Health <= 0) {

			if (saving) {

                Health = gameManager.commonCharactersVariables.savedHealthAmount;
				Berserk = true;
				BerserkTurn = true;

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

			BerserkTurn = true;

			if(!Berserk)
				GetComponent<Renderer> ().material.color = Color.cyan;

			Berserk = true;

		}

        if (!dead)
            UpdateHealth();

        return dead;

	}

	public override void Tire() {

		if (!Berserk || BerserkTurn) base.Tire ();

	}

	public override void UnTired() {

		if (Berserk)
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
			Relationships.TryGetValue (hero.characterName, out value);

			if (value >= gameManager.commonCharactersVariables.berserkTriggerRelation) {

				hero.Berserk = true;
				hero.BerserkTurn = true;
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
