using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class SC_Construction : NetworkBehaviour {

	public bool test;

	public string buildingName;

	public int maxHealth;
	[HideInInspector]
	public int health;

	//static GameObject buildingInfosPanel;
    [HideInInspector]
    public bool selfPanel;
	[HideInInspector]
	public SC_Lifebar lifebar;

	protected static SC_Tile_Manager tileManager;

	protected static SC_UI_Manager uiManager;

    protected virtual void Start () {

		tileManager = GameObject.FindObjectOfType<SC_Tile_Manager> ();

		/*if(buildingInfosPanel == null)
			buildingInfosPanel = GameObject.Find ("BuildingInfos");

		buildingInfosPanel.SetActive(false);*/

		if (tileManager == null)
			tileManager = FindObjectOfType<SC_Tile_Manager> ();

		if (uiManager == null)
			uiManager = FindObjectOfType<SC_UI_Manager> ();

		health = maxHealth;

		/*SC_Tile under = SC_GameManager.GetInstance ().GetTileAt ((int)transform.position.x, (int)transform.position.y);

		if (!GetType ().Equals (typeof(SC_Wall)))
			under.constructable = false;

		if (GetType ().Equals (typeof(SC_Village)) || GetType ().Equals (typeof(SC_Workshop))) {
			
			under.attackable = false;

		} else {
		
			GameObject go = Instantiate(Resources.Load<GameObject>("Prefabs/P_Lifebar"), transform);
			go.transform.localPosition = go.transform.position;
			lifebar = GetComponentInChildren<SC_Lifebar>();

			under.movementCost = 10000;
			under.attackable = SC_GameManager.GetInstance ().CoalitionTurn ();

		}*/

		tileManager.SetConstruction (this);

    }

	protected virtual void OnMouseDown() {

		SC_Tile under = tileManager.GetTileAt (gameObject); //SC_GameManager.GetInstance().GetTileAt((int)transform.position.x, (int)transform.position.y);

		if (under.GetDisplayAttack ()) {

			SC_Character attackingCharacter = SC_Character.GetAttackingCharacter ();
			SC_Tile attackingCharacterTile = tileManager.GetTileAt (attackingCharacter.gameObject); //SC_GameManager.GetInstance ().GetTileAt ((int)attackingCharacter.transform.position.x, (int)attackingCharacter.transform.position.y);
			SC_GameManager.GetInstance ().rangedAttack = !SC_GameManager.GetInstance ().IsNeighbor (attackingCharacterTile, under);

			attackingCharacter.attackTarget = under;

			((SC_Hero)attackingCharacter).ChooseWeapon ();

		}

	}

    protected void OnMouseOver() {

        if(Input.GetMouseButtonDown(1)) {

			uiManager.ShowHideInfos (gameObject, typeof(SC_Construction));

            /*if (selfPanel) {

                HideBuildingPanel();
                selfPanel = false;

             } else {

                SC_Character.HideStatPanel();
                SC_Qin.HideQinPanel();

                ShowBuildingPanel();

                foreach (SC_Character character in FindObjectsOfType<SC_Character>())
                    character.selfPanel = false;

                foreach (SC_Construction construction in FindObjectsOfType<SC_Construction>())
                    construction.selfPanel = false;

                SC_Qin.selfPanel = false;

                selfPanel = true;

            }*/

        }

    }

	/*public void ShowBuildingPanel() {


		//buildingInfosPanel.SetActive (true);

		SetText("BuildingName", buildingName);
		SetText("BuildingHealth", (GetType ().Equals (typeof(SC_Village))) ? "" : "Health : " + health + " / " + maxHealth);

	}

    public static void HideBuildingPanel() {

		uiManager.buildingInfosPanel.SetActive (false);
		//buildingInfosPanel.gameObject.SetActive (false);

	}

	public static void SetText(string id, string text) {

		GameObject.Find (id).GetComponent<Text> ().text = text;

	}*/

	public virtual void DestroyConstruction() {

        selfPanel = false;
		uiManager.buildingInfosPanel.SetActive (false);
		//buildingInfosPanel.SetActive(false);

        //int x = (int)transform.position.x;
		//int y = (int)transform.position.y;

		SC_Tile under = tileManager.GetTileAt (gameObject); //SC_GameManager.GetInstance().GetTileAt (x, y);

		under.movementCost = under.baseCost;
		under.constructable = !under.isPalace();
		under.attackable = true;
		Destroy (gameObject);

	}

	public bool Attackable() {

		return (!GetType ().Equals (typeof(SC_Village)) && !GetType ().Equals (typeof(SC_Workshop)));

	}

}
