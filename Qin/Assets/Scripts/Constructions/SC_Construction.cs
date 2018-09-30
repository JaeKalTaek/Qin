using UnityEngine;
using UnityEngine.Networking;
using static SC_Global;

public class SC_Construction : NetworkBehaviour {

    [Header("Constructions Variables")]
    [Tooltip("Name of the construction")]
    public string Name;

    [Tooltip("Base maximum health of the construction, put 0 for a construction who doesn't have health")]
    public int maxHealth;

    public int Health { get; set; }

    [Tooltip("Cost for Qin to build this construction")]
    public int cost;

    [Tooltip("Is this a Production Construction")]
    public bool production;

    [Tooltip("Combat modifiers for this construction")]
    public CombatModifiers combatModifers;

    public SC_Lifebar Lifebar { get; set; }

    public bool GreatWall { get { return (this as SC_Bastion != null) || (this as SC_Wall != null); } }

    public SC_Pump Pump { get { return this as SC_Pump; } }

	protected static SC_Game_Manager gameManager;

	protected static SC_Tile_Manager tileManager;

	protected static SC_UI_Manager uiManager;

    public static SC_Construction lastConstru;

    public static SC_Soldier lastConstruSoldier;

    protected void Awake () {

        if (!tileManager)
            tileManager = FindObjectOfType<SC_Tile_Manager>();

        if (tileManager && (tileManager.tiles != null))
            tileManager.GetTileAt(gameObject).Construction = this;

    }

    protected virtual void Start () {

		if (!gameManager)
			gameManager = FindObjectOfType<SC_Game_Manager> ();		

		if (!uiManager)
			uiManager = FindObjectOfType<SC_UI_Manager> ();

		Health = maxHealth;

        tileManager.GetTileAt(gameObject).Construction = this;

    }

	public virtual void DestroyConstruction() {

        uiManager.HideInfosIfActive(gameObject);

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

        tileManager.RemoveAllFilters();

        lastConstru.gameObject.SetActive(false);

        lastConstru.DestroyConstruction();

        if (lastConstruSoldier) {

            SC_Qin.ChangeEnergy(-lastConstruSoldier.sacrificeValue);

            lastConstruSoldier.gameObject.SetActive(true);

            tileManager.GetTileAt(lastConstruSoldier.gameObject).Character = lastConstruSoldier;

        }                

        if (SC_Player.localPlayer.Qin) {

            if (!gameManager.QinTurnStarting) {

                SC_Qin.ChangeEnergy(SC_Qin.GetConstruCost(lastConstru.Name));

                SC_Player.localPlayer.CmdChangeQinEnergyOnClient(SC_Qin.GetConstruCost(lastConstru.Name), false);

            }

            uiManager.UpdateQinConstructPanel();

            SC_Player.localPlayer.Busy = true;

            tileManager.DisplayConstructableTiles(lastConstru.Name == "Wall");

        }

        lastConstru = null;

        lastConstruSoldier = null;        

    }

}
