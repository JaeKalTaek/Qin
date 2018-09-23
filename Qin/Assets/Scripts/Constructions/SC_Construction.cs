﻿using UnityEngine;
using UnityEngine.Networking;

public class SC_Construction : NetworkBehaviour {

    [Header("Constructions Variables")]
    [Tooltip("Name of the construction")]
	public string Name;

    [Tooltip("Base maximum health of the construction, put 0 for a construction who doesn't have health")]
	public int maxHealth;

	public int Health { get; set; }

    [Tooltip("Cost for Qin to build this construction")]
    public int cost;

	public SC_Lifebar Lifebar { get; set; }

    public bool GreatWall { get { return (this as SC_Bastion != null) || (this as SC_Wall != null); } }

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

        lastConstru.DestroyConstruction();

        if (lastConstruSoldier) {

            SC_Qin.ChangeEnergy(-lastConstruSoldier.sacrificeValue);

            lastConstruSoldier.gameObject.SetActive(true);

        }

        if (SC_Player.localPlayer.Qin) {

            if (!gameManager.QinTurnBeginning) {

                SC_Qin.ChangeEnergy(SC_Qin.GetConstruCost(lastConstru.Name));

                SC_Player.localPlayer.CmdChangeQinEnergyOnClient(SC_Qin.GetConstruCost(lastConstru.Name), false);

            }

            uiManager.UpdateConstructPanel();

            SC_Player.localPlayer.Busy = true;

            tileManager.DisplayConstructableTiles(lastConstru.Name == "Wall");

            uiManager.cancelLastConstructButton.SetActive(false);

        }

        lastConstru = null;

        lastConstruSoldier = null;        

    }

}
