﻿using UnityEngine;
using UnityEngine.Networking;

public class SC_Player : NetworkBehaviour {

	[SyncVar]
	bool qin;

	SC_GameManager gameManager;

	SC_Tile_Manager tileManager;

	public static SC_Player localPlayer;

	public override void OnStartLocalPlayer () {

        SetSide();

        tag = "Player";

		gameManager = FindObjectOfType<SC_GameManager> ();

		if(gameManager)
			gameManager.player = this;

		if(FindObjectOfType<SC_Tile_Manager> () != null)
			tileManager = FindObjectOfType<SC_Tile_Manager> ();

		localPlayer = this;
		
	}

	#region Commands
    [Command]
    public void CmdCheckMovements(int x, int y) {

        RpcCheckMovements(x, y);

    }

    [ClientRpc]
    void RpcCheckMovements(int x, int y) {

        localPlayer.gameManager.CheckMovements(localPlayer.tileManager.GetAt<SC_Character>(x, y));

    }

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
    public void CmdSetCharacterToMove(int x, int y) {

        RpcSetCharacterToMove(x, y);

    }

    [ClientRpc]
    void RpcSetCharacterToMove(int x, int y) {

        localPlayer.gameManager.SetCharacterToMove(localPlayer.tileManager.GetAt<SC_Character>(x, y));

    }

    [Command]
    public void CmdMoveCharacterTo(int x, int y) {

        RpcMoveCharacterTo(x, y);

    }

    [ClientRpc]
    void RpcMoveCharacterTo(int x, int y) {

        localPlayer.gameManager.GetCharacterToMove().MoveTo(localPlayer.tileManager.GetTileAt(x, y));

    }

    [Command]
    public void CmdCancelMovement() {

        RpcCancelMovement();

    }

    [ClientRpc]
    void RpcCancelMovement() {

        localPlayer.gameManager.CancelMovementFunction();

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
	public void CmdRemoveAllFilters() {

		RpcRemoveAllFilters ();

	}

	[ClientRpc]
	void RpcRemoveAllFilters() {

        foreach(SC_Tile tile in localPlayer.tileManager.tiles)
		    tile.RemoveFilters ();

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
    public void CmdFinishConstruction() {

        RpcFinishConstruction();

    }

    [ClientRpc]
    void RpcFinishConstruction() {        

        if(localPlayer.IsQin())
            localPlayer.gameManager.FinishConstruction();

        localPlayer.gameManager.Bastion = false;

    }

    [Command]
    public void CmdUpdateWallGraph(GameObject go) {

        RpcUpdateWallGraph(go);

    }

    [ClientRpc]
    void RpcUpdateWallGraph(GameObject go) {

        localPlayer.gameManager.UpdateWallGraph(go);

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

	public void SetSide() {

        qin = FindObjectOfType<SC_Network_Manager>().IsQinHost() == isServer;

    }

}
