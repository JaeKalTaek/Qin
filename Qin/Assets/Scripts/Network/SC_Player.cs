using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class SC_Player : NetworkBehaviour {

	[SyncVar]
	bool qin;

	SC_GameManager gameManager;

	public override void OnStartLocalPlayer () {
		
		tag = "Player";

		gameManager = FindObjectOfType<SC_GameManager> ();

		if(gameManager)
			gameManager.player = this;

		//<SC_UI_Manager> ().SetupUI (this, qin);
		
	}

	#region Commands
	[Command]
	public void CmdDisplayMovement(GameObject tile) {

		RpcDisplayMovement (tile);

	}

	[ClientRpc]
	void RpcDisplayMovement(GameObject tile) {

		tile.GetComponent<SC_Tile> ().DisplayMovement (true);

	}

	[Command]
	public void CmdRemoveFilters(GameObject tile) {

		RpcRemoveFilters (tile);

	}

	[ClientRpc]
	void RpcRemoveFilters(GameObject tile) {

		tile.GetComponent<SC_Tile> ().RemoveFilters ();

	}
	#endregion

	public void SetGameManager(SC_GameManager gm) {

		gameManager = gm; 

	}

	public bool Turn() {

		return (qin == (!gameManager.CoalitionTurn()));

	}

	public bool IsQin() {

		return qin;

	}

	public void SetSide(bool side) {

		qin = side;

	}

}
