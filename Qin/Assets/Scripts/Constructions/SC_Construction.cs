using UnityEngine;
using UnityEngine.Networking;

public class SC_Construction : NetworkBehaviour {

	public bool test;

	public string buildingName;

	public int maxHealth;
	public int Health { get; set; }

	public SC_Lifebar Lifebar { get; set; }

	protected static SC_Game_Manager gameManager;

	protected static SC_Tile_Manager tileManager;

	protected static SC_UI_Manager uiManager;

	protected virtual void Start () {

		if (!gameManager)
			gameManager = FindObjectOfType<SC_Game_Manager> ();

		if (!tileManager)
			tileManager = FindObjectOfType<SC_Tile_Manager> ();

		if (!uiManager)
			uiManager = FindObjectOfType<SC_UI_Manager> ();

		Health = maxHealth;

        tileManager.GetTileAt(gameObject).Construction = this;

    }

	/*protected virtual void OnMouseDown() {

		SC_Tile under = tileManager.GetTileAt (gameObject);

		if (under.CurrentDisplay == TDisplay.Attack) {

			SC_Tile attackingCharacterTile = tileManager.GetTileAt (SC_Character.attackingCharacter.gameObject);
			gameManager.rangedAttack = !tileManager.IsNeighbor (attackingCharacterTile, under);

            SC_Character.attackingCharacter.attackTarget = under;

			((SC_Hero)SC_Character.attackingCharacter).ChooseWeapon ();

		}

	}*/

	protected void OnMouseOver() {

		if(Input.GetMouseButtonDown(1))
			uiManager.ShowHideInfos (gameObject, typeof(SC_Construction));

	}

	public virtual void DestroyConstruction() {

        if(uiManager.CurrentGameObject == gameObject)
		    uiManager.HideInfos (gameObject);

		tileManager.GetTileAt (gameObject).Construction = null;

        Destroy(gameObject);

        //SC_Player.localPlayer.CmdDestroyGameObject(gameObject);

		/*if(isServer)
			Network.Destroy (gameObject);*/

	}

	public bool Attackable() {

		return (!GetType ().Equals (typeof(SC_Village)) && !GetType ().Equals (typeof(SC_Workshop)));

	}

}
