using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class SC_Player : NetworkBehaviour {

	[SyncVar]
	bool qin;

	SC_GameManager gameManager;

	SC_Tile_Manager tileManager;

	public override void OnStartLocalPlayer () {

		print ("Start Local Player, " + (FindObjectOfType<SC_Tile_Manager> () != null));

		tag = "Player";

		gameManager = FindObjectOfType<SC_GameManager> ();

		if(gameManager)
			gameManager.player = this;

		if(FindObjectOfType<SC_Tile_Manager> () != null)
			tileManager = FindObjectOfType<SC_Tile_Manager> ();

		//<SC_UI_Manager> ().SetupUI (this, qin);
		
	}

	#region Commands
	[Command]
	public void CmdDisplayMovement(int[] xArray, int[] yArray) {

		RpcDisplayMovement (xArray, yArray);

	}

	[ClientRpc]
	void RpcDisplayMovement(int[] xArray, int[] yArray) {

		print (tag);

		if (isLocalPlayer) {

			print (tileManager);

			for (int i = 0; i < xArray.Length; i++)
				tileManager.GetTileAt (xArray [i], yArray [i]).DisplayMovement ();

		}

	}

	[Command]
	public void CmdDisplayAttack(GameObject tile) {

		RpcDisplayAttack (tile);

	}

	[ClientRpc]
	void RpcDisplayAttack(GameObject tile) {

		tile.GetComponent<SC_Tile> ().DisplayAttack ();

	}

	[Command]
	public void CmdDisplayConstructable(GameObject tile) {

		RpcDisplayConstructable (tile);

	}

	[ClientRpc]
	void RpcDisplayConstructable(GameObject tile) {

		tile.GetComponent<SC_Tile> ().DisplayConstructable ();

	}

	[Command]
	public void CmdRemoveFilters(GameObject tile) {

		RpcRemoveFilters (tile);

	}

	[ClientRpc]
	void RpcRemoveFilters(GameObject tile) {

		tile.GetComponent<SC_Tile> ().RemoveFilters ();

	}

	[Command]
	public void CmdMove(GameObject toMove, Vector3 pos) {

		toMove.transform.SetPos (pos);

	}
	#endregion

	public void SetGameManager(SC_GameManager gm) {

		gameManager = gm; 

	}

	public void SetTileManager(SC_Tile_Manager tm) {

		print ("Set Tile Manager to : " + tm);

		tileManager = tm;

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

	void Update() {

		print ("Tile Manager Is : " + tileManager);

	}

}
