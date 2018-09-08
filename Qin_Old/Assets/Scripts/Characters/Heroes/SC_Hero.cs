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

		tileManager.SetHero (this);

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

		if (canMove || (berserk && !berserkTurn)) {

            SC_Player.localPlayer.CmdCheckMovements((int)transform.position.x, (int)transform.position.y);

            uiManager.ShowHeroPower (powerUsed, name);

		}

	}

	void OnMouseEnter() {

		SC_Tile under = tileManager.GetTileAt (gameObject);

		if ((under.CurrentDisplay == TDisplay.Attack) && !attackingCharacter.IsHero()) {

            attackingCharacter.attackTarget = under;

			gameManager.PreviewFight(false);

            attackingCharacter.attackTarget = null;

		}

	}

	void OnMouseExit() {

		uiManager.previewFightPanel.SetActive (false);

	}

	public void ChooseWeapon() {

		uiManager.cancelMovementButton.SetActive (false);
		uiManager.cancelAttackButton.SetActive (true);

		if ((gameManager.rangedAttack && weapon1.ranged) || (!gameManager.rangedAttack && !weapon1.IsBow ()))
			uiManager.ShowWeapon (GetWeapon (true), true);

		if ((gameManager.rangedAttack && weapon2.ranged) || (!gameManager.rangedAttack && !weapon2.IsBow ()))
			uiManager.ShowWeapon (GetWeapon (false), false);

	}

    public static void Attack(bool usedActiveWeapon) {

        ((SC_Hero)attackingCharacter).SetWeapon(usedActiveWeapon);

        gameManager.Attack();

    }

    public void ActionVillage(bool destroy) {

		if (destroy) {

			gameManager.cantCancelMovement = true;
            tileManager.GetTileAt(gameObject).construction.DestroyConstruction();

		} else {

			uiManager.cancelMovementButton.SetActive (true);

		}

		uiManager.villagePanel.SetActive (false);

		CheckAttack();

	}

	public void Regen() {

		if (tileManager.GetTileAt(gameObject).village != null) {

			health = Mathf.Max (health + 10, maxHealth);
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

				SC_Hero saver = gameManager.CheckHeroSaved (this, saved);

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

			canMove = (gameManager.CoalitionTurn());

			berserkTurn = true;

			if(!berserk)
				GetComponent<Renderer> ().material.color = Color.cyan;

			berserk = true;

		}

		if (!dead)
			lifebar.UpdateGraph (health, maxHealth);

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

		gameManager.lastHeroDead = this;

		tileManager.GetTileAt (gameObject).constructable = !tileManager.GetTileAt (gameObject).palace;

		foreach (SC_Hero hero in FindObjectsOfType<SC_Hero>()) {

			int value = 0;
			relationships.TryGetValue (hero.characterName, out value);

			if (value >= 200) {

				hero.berserk = true;
				hero.berserkTurn = true;
				hero.canMove = (gameManager.CoalitionTurn());

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
