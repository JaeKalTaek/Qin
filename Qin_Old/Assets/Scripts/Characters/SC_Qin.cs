using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class SC_Qin : NetworkBehaviour {

	[Header("Qin variables")]
	[Tooltip("Energy of Qin at the start of the game")]
	public int startEnergy;
	static int energy;

	[Tooltip("Energy necessary for Qin to win the game")]
	public int energyToWin;

	[Tooltip("Cost of the power of Qin")]
	public int powerCost;

	[Tooltip("Energy won when a hero dis")]
	public int energyWhenHeroDies;

    [Tooltip("Cost in energy for Qin to build a wall")]
    public int wallCost;

    [Tooltip("Energy won when Qin sacrifices a soldier")]
    public int sacrificeValue;

	[HideInInspector]
	public static bool selfPanel;

	static SC_GameManager gameManager;

	static SC_Tile_Manager tileManager;

	static SC_UI_Manager uiManager;

	public static SC_Qin Qin;

	void Start() {

		Qin = this;

		gameManager = GameObject.FindObjectOfType<SC_GameManager> ();

		tileManager = GameObject.FindObjectOfType<SC_Tile_Manager> ();

		uiManager = GameObject.FindObjectOfType<SC_UI_Manager> ();

		energy = startEnergy;

		uiManager.energyText.text = "Qin's Energy : " + energy;

		/*qinPanel = GameObject.Find ("QinPanel");

		qinPanel.SetActive (false);*/

		//SC_GameManager.GetInstance().GetTileAt((int)transform.position.x, (int)transform.position.y).constructable = false;

		//SC_GameManager.GetInstance ().GetTileAt ((int)transform.position.x, (int)transform.position.y).movementCost = 10000;

		tileManager.SetQin (this);

	}

	void OnMouseDown() {

		SC_Tile under = tileManager.GetTileAt (gameObject); //SC_GameManager.GetInstance().GetTileAt((int)transform.position.x, (int)transform.position.y);

		if (under.GetDisplayAttack ()) {

			SC_Character attackingCharacter = SC_Character.GetAttackingCharacter();
			SC_Tile attackingCharacterTile = tileManager.GetTileAt (attackingCharacter.gameObject); //SC_GameManager.GetInstance().GetTileAt((int)attackingCharacter.transform.position.x, (int)attackingCharacter.transform.position.y);
			gameManager.rangedAttack = !gameManager.IsNeighbor(attackingCharacterTile, under);

			attackingCharacter.attackTarget = under;

			if (attackingCharacter.isHero ()) {

				((SC_Hero)attackingCharacter).ChooseWeapon ();

			} else {

				foreach (SC_Tile tile in tileManager.tiles)
					tile.RemoveFilters ();

				gameManager.Attack ();

			}

		}

	}

	void OnMouseOver() {

		if(Input.GetMouseButtonDown(1))
			uiManager.ShowHideInfos(gameObject, GetType());

	}

	public static void UsePower(Vector3 pos) {

		SC_Hero hero = gameManager.lastHeroDead;

		hero.transform.SetPos(pos);
		hero.coalition = false;
		hero.powerUsed = false;
		hero.powerBacklash = 0;
		hero.SetBaseColor (new Color (255, 0, 205));
		hero.health = hero.maxHealth;
		hero.lifebar.UpdateGraph(hero.health, hero.maxHealth);
		hero.SetCanMove (true);
		hero.berserk = false;
		hero.berserkTurn = false;
		hero.UnTired ();

		Quaternion rotation = Quaternion.identity;
		rotation.eulerAngles = new Vector3(0, 0, 180);

		Quaternion lifebarRotation = Quaternion.identity;
		lifebarRotation.eulerAngles = hero.lifebar.transform.parent.rotation.eulerAngles;

		hero.transform.rotation = rotation;
		hero.lifebar.transform.parent.rotation = lifebarRotation;

		hero.gameObject.SetActive (true);

		ChangeEnergy(-Qin.powerCost);

		gameManager.lastHeroDead = null;

	}

	public static int GetEnergy() {

		return energy;

	}

	public static void ChangeEnergy(int amount) {

		energy += amount;

		if (energy > Qin.energyToWin)
			uiManager.ShowVictory (true);
		else if (energy > 0)
			uiManager.energyText.text = "Qin's Energy : " + energy;
		else
			uiManager.ShowVictory (false);

	}

}
