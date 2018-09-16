using UnityEngine;
using UnityEngine.Networking;

public class SC_Construction : NetworkBehaviour {

	public string Name;

	public int maxHealth;
	public int Health { get; set; }

	public SC_Lifebar Lifebar { get; set; }

    public bool GreatWall { get { return (this as SC_Bastion != null) || (this as SC_Wall != null); } }

	protected static SC_Game_Manager gameManager;

	protected static SC_Tile_Manager tileManager;

	protected static SC_UI_Manager uiManager;

    public static SC_Construction lastConstru;

    public static SC_Soldier lastConstruSoldier;

	protected virtual void Start () {

		if (!gameManager)
			gameManager = FindObjectOfType<SC_Game_Manager> ();

		if (!tileManager)
			tileManager = FindObjectOfType<SC_Tile_Manager> ();

		if (!uiManager)
			uiManager = FindObjectOfType<SC_UI_Manager> ();

		Health = maxHealth;

        SC_Tile under = tileManager.GetTileAt(gameObject);

        under.Construction = this;

        under.Locked = false;

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
			uiManager?.ShowHideInfos (gameObject, typeof(SC_Construction));

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

    public static void CancelLastConstruction () {        

        lastConstru.DestroyConstruction();

        if (lastConstruSoldier) {

            SC_Qin.ChangeEnergy(-SC_Qin.Qin.sacrificeValue);

            lastConstruSoldier.gameObject.SetActive(true);

        }

        if (!gameManager.Bastion) {

            SC_Qin.ChangeEnergy(SC_Qin.GetConstruCost(lastConstru.Name));

        } else if (SC_Player.localPlayer.qin) {

            SC_Player.localPlayer.Busy = true;

            tileManager.DisplayConstructableTiles(false);

            uiManager.StopCancelConstruct();

        }

        lastConstru = null;

        lastConstruSoldier = null;        

    }

}
