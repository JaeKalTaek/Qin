using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class SC_Qin : NetworkBehaviour {

	//static Text energyText;
	public int startEnergy;
	static int energy;
	//static GameObject qinPanel;
    [HideInInspector]
    public static bool selfPanel;
	SC_Tile_Manager tileManager;
	static SC_UI_Manager uiManager;

    void Awake() {

        //energyText = GameObject.Find("EnergyQin").GetComponent<Text>();

		//energyText.text = "Qin's Energy : " + energy;

	}

	void Start() {

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

			uiManager.ShowHideInfos(gameObject, GetType());

            /*if (selfPanel) {

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

            }*/

        }

    }

	/*public static void ShowQinPanel() {

		uiManager.qinPanel.SetActive (true);
		qinPanel.transform.GetChild(1).GetComponent<Text> ().text = energy.ToString();
		qinPanel.SetActive (true);

	}

	public static void HideQinPanel() {

		uiManager.qinPanel.SetActive (false);
		//qinPanel.SetActive (false);

	}*/

	public static void UsePower(Vector3 pos) {

        SC_Hero hero = SC_GameManager.GetInstance ().lastHeroDead;

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

		ChangeEnergy(-2000);

		SC_GameManager.GetInstance ().lastHeroDead = null;

	}

	public static int GetEnergy() {

		return energy;

	}

	public static void ChangeEnergy(int amount) {

		energy += amount;
		uiManager.energyText.text = "Qin's Energy : " + energy;
		//energyText.text = "Qin's Energy : " + energy;

	}

}
