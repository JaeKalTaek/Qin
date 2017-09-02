using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SC_Qin : MonoBehaviour {

	static Text energyText;
	public int startEnergy;
	static int energy;
	static GameObject qinPanel;
    [HideInInspector]
    public static bool selfPanel;

    void Awake() {

        energyText = GameObject.Find("EnergyQin").GetComponent<Text>();

		energy = startEnergy;

		energyText.text = UpdateText(energy);

	}

	void Start() {

		qinPanel = GameObject.Find ("QinPanel");

		qinPanel.SetActive (false);

		SC_GameManager.GetInstance().GetTileAt((int)transform.position.x, (int)transform.position.y).constructable = false;

		SC_GameManager.GetInstance ().GetTileAt ((int)transform.position.x, (int)transform.position.y).movementCost = 10000;

	}

	void OnMouseDown() {

		SC_Tile under = SC_GameManager.GetInstance().GetTileAt((int)transform.position.x, (int)transform.position.y);

		if (under.GetDisplayAttack ()) {

			SC_Character attackingCharacter = SC_Character.GetAttackingCharacter();
			SC_Tile attackingCharacterTile = SC_GameManager.GetInstance().GetTileAt((int)attackingCharacter.transform.position.x, (int)attackingCharacter.transform.position.y);
			SC_GameManager.GetInstance().rangedAttack = !SC_GameManager.GetInstance().IsNeighbor(attackingCharacterTile, under);

			attackingCharacter.attackTarget = under;

			if (attackingCharacter.isHero ()) {

				((SC_Hero)attackingCharacter).ChooseWeapon ();

			} else {

				foreach (SC_Tile tile in SC_GameManager.GetInstance().tiles)
					tile.RemoveFilter ();

				SC_GameManager.GetInstance ().Attack ();

			}

		}

	}

    void OnMouseOver() {

        if(Input.GetMouseButtonDown(1)) {

            if (selfPanel) {

                HideQinPanel();
                selfPanel = false;

             } else {

                SC_Character.HideStatPanel();
                SC_Construction.HideBuildingPanel();

                ShowQinPanel();

                foreach (SC_Character character in FindObjectsOfType<SC_Character>())
                    character.selfPanel = false;

                foreach (SC_Construction construction in FindObjectsOfType<SC_Construction>())
                    construction.selfPanel = false;

                selfPanel = true;

            }

        }

    }

	public static void ShowQinPanel() {

		qinPanel.SetActive (true);
		GameObject.Find ("QinEnergy").GetComponent<Text> ().text = energy.ToString();

	}

	public static void HideQinPanel() {

		qinPanel.SetActive (false);

	}

	public static void UsePower(Vector3 pos) {

        SC_Hero hero = SC_GameManager.GetInstance ().lastHeroDead;

		hero.transform.SetPos(pos);
		hero.gameObject.SetActive (true);
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

        DecreaseEnergy(2000);

		SC_GameManager.GetInstance ().lastHeroDead = null;

	}

	public static int GetEnergy() {

		return energy;

	}

	public static void IncreaseEnergy(int amount) {

		energy += amount;
		energyText.text = UpdateText(energy);

	}

	public static void DecreaseEnergy(int amount) {

		energy -= amount;
		energyText.text = UpdateText(energy);

	}

	static string UpdateText(int energy) {

		return "Qin's Energy : " + energy;

	}

}
