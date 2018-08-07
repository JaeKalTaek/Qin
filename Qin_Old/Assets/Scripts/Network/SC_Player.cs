using UnityEngine;
using UnityEngine.Networking;

public class SC_Player : NetworkBehaviour {

	[SyncVar]
	bool qin;

	SC_GameManager gameManager;

	SC_Tile_Manager tileManager;

	public static SC_Player localPlayer;

	public override void OnStartLocalPlayer () {

		tag = "Player";

		gameManager = FindObjectOfType<SC_GameManager> ();

		if(gameManager)
			gameManager.player = this;

		if(FindObjectOfType<SC_Tile_Manager> () != null)
			tileManager = FindObjectOfType<SC_Tile_Manager> ();

		localPlayer = this;

		//<SC_UI_Manager> ().SetupUI (this, qin);
		
	}

	#region Commands
	[Command]
	public void CmdDisplayMovement(int[] xArray, int[] yArray) {

		RpcDisplayMovement (xArray, yArray);

	}

	[ClientRpc]
	void RpcDisplayMovement(int[] xArray, int[] yArray) {

		localPlayer.DisplayMovement (xArray, yArray);

	}

	void DisplayMovement(int[] xArray, int[] yArray) {

		for (int i = 0; i < xArray.Length; i++)
			tileManager.GetTileAt (xArray [i], yArray [i]).DisplayMovement ();

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
    public void CmdDisplaySacrifice(int[] xArray, int[] yArray) {

        RpcDisplaySacrifice(xArray, yArray);

    }

    [ClientRpc]
    void RpcDisplaySacrifice(int[] xArray, int[] yArray) {

        if(localPlayer.IsQin())
            localPlayer.DisplaySacrifice(xArray, yArray);

    }

    void DisplaySacrifice(int[] xArray, int[] yArray) {

        for (int i = 0; i < xArray.Length; i++)
            tileManager.GetTileAt(xArray[i], yArray[i]).DisplaySacrifice();

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

	[Command]
	public void CmdNextTurn() {
		
		RpcNextTurn ();

	}

	[ClientRpc]
	void RpcNextTurn() {  

		localPlayer.gameManager.NextTurnFunction ();

	}
		
	[Command]
	public void CmdConstructAt(int x, int y) {

        localPlayer.gameManager.ConstructAt(x, y);

    }

    [Command]
    public void CmdUpdateWallGraph(int x, int y) {

        RpcUpdateWallGraph(x, y);

    }

    [ClientRpc]
    void RpcUpdateWallGraph(int x, int y) {

        localPlayer.gameManager.UpdateWallGraph(localPlayer.tileManager.GetTileAt(x, y));

    }

    [Command]
    public void CmdUpdateNeighborWallsGraph(int x, int y) {

        RpcUpdateNeighborWallsGraph(x, y);

    }

    [ClientRpc]
    void RpcUpdateNeighborWallsGraph(int x, int y) {

        localPlayer.gameManager.UpdateNeighborWallGraph(localPlayer.tileManager.GetTileAt(x, y));

    }

    [Command]
	public void CmdChangeQinEnergy(int amount) {

		RpcChangeQinEnergy (amount);

	}

	[ClientRpc]
	void RpcChangeQinEnergy(int amount) {

		SC_Qin.ChangeEnergy (amount);

	}

	[Command]
	public void CmdDestroyGameObject(GameObject go) {

		if(go && (go.name != "dead")) {

			go.name = "dead";

			NetworkServer.Destroy (go);

		}

	}
	#endregion

	public void SetGameManager(SC_GameManager gm) {

		gameManager = gm; 

	}

	public void SetTileManager(SC_Tile_Manager tm) {

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

}
