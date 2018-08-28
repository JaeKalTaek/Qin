using UnityEngine;
using UnityEngine.Networking;
using static SC_Enums;

public class SC_Construction : NetworkBehaviour {

	public bool test;

	public string buildingName;

	public int maxHealth;
	[HideInInspector]
	public int health;

	[HideInInspector]
	public SC_Lifebar lifebar;

	protected static SC_GameManager gameManager;

	protected static SC_Tile_Manager tileManager;

	protected static SC_UI_Manager uiManager;

	protected virtual void Start () {

		if (gameManager == null)
			gameManager = FindObjectOfType<SC_GameManager> ();

		if (tileManager == null)
			tileManager = FindObjectOfType<SC_Tile_Manager> ();

		if (uiManager == null)
			uiManager = FindObjectOfType<SC_UI_Manager> ();

		health = maxHealth;

		tileManager.SetConstruction (this);

	}

	protected virtual void OnMouseDown() {

		SC_Tile under = tileManager.GetTileAt (gameObject);

		if (under.CurrentDisplay == TDisplay.Attack) {

			SC_Tile attackingCharacterTile = tileManager.GetTileAt (SC_Character.attackingCharacter.gameObject);
			gameManager.rangedAttack = !tileManager.IsNeighbor (attackingCharacterTile, under);

            SC_Character.attackingCharacter.attackTarget = under;

			((SC_Hero)SC_Character.attackingCharacter).ChooseWeapon ();

		}

	}

	protected void OnMouseOver() {

		if(Input.GetMouseButtonDown(1))
			uiManager.ShowHideInfos (gameObject, typeof(SC_Construction));

	}

	public virtual void DestroyConstruction() {

		uiManager.HideInfos (gameObject);

		SC_Tile under = tileManager.GetTileAt (gameObject);

		under.movementCost = under.baseCost;
		under.constructable = !under.IsPalace();
		under.attackable = true;

        SC_Player.localPlayer.CmdDestroyGameObject(gameObject);

		/*if(isServer)
			Network.Destroy (gameObject);*/

	}

	public bool Attackable() {

		return (!GetType ().Equals (typeof(SC_Village)) && !GetType ().Equals (typeof(SC_Workshop)));

	}

}
