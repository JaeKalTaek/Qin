﻿using UnityEngine;
using UnityEngine.Networking;
using static SC_Enums;

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

    #region Characters movements
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

        for(int i = 0; i < xArray.Length; i++)
            tileManager.GetTileAt(xArray[i], yArray[i]).ChangeDisplay(TDisplay.Movement);

	}

    [Command]
    public void CmdMoveCharacterTo(int x, int y) {

        RpcMoveCharacterTo(x, y);

    }

    [ClientRpc]
    void RpcMoveCharacterTo(int x, int y) {

        localPlayer.gameManager.characterToMove.MoveTo(localPlayer.tileManager.GetTileAt(x, y));

    }

    [Command]
    public void CmdResetMovement() {

        RpcResetMovement();

    }

    [ClientRpc]
    void RpcResetMovement() {

        localPlayer.gameManager.ResetMovementFunction();

    }
    #endregion

    #region Attack
    [Command]
	public void CmdPrepareForAttack(bool rangedAttack, GameObject targetTileObject) {

        RpcPrepareForAttack(rangedAttack, targetTileObject);

    }

	[ClientRpc]
	void RpcPrepareForAttack(bool rangedAttack, GameObject targetTileObject) {

        localPlayer.tileManager.RemoveAllFilters();

        localPlayer.gameManager.rangedAttack = rangedAttack;

        SC_Character.attackingCharacter.attackTarget = targetTileObject.GetComponent<SC_Tile>();

	}

    [Command]
    public void CmdAttack() {

        RpcAttack();

    }

    [ClientRpc]
    void RpcAttack() {

        localPlayer.gameManager.Attack();

    }

    [Command]
    public void CmdHeroAttack(bool usedActiveWeapon) {

        RpcHeroAttack(usedActiveWeapon);

    }

    [ClientRpc]
    void RpcHeroAttack(bool usedActiveWeapon) {

        SC_Hero.Attack(usedActiveWeapon);

    }
    #endregion

    #region Remove filters
    [Command]
	public void CmdRemoveAllFilters() {

		RpcRemoveAllFilters ();

	}    

    [ClientRpc]
	void RpcRemoveAllFilters() {

        localPlayer.tileManager.RemoveAllFilters();

    }

    [Command]
    public void CmdRemoveAllFiltersOnClient(bool qin) {

        RpcRemoveAllFiltersForClient(qin);

    }

    [ClientRpc]
    void RpcRemoveAllFiltersForClient(bool qin) {

        if(localPlayer.qin == qin)
            localPlayer.tileManager.RemoveAllFilters();

    }
    #endregion

    #region Next Turn
    [Command]
	public void CmdNextTurn() {
		
		RpcNextTurn ();

	}
    
    [ClientRpc]
	void RpcNextTurn() {  

		localPlayer.gameManager.NextTurnFunction ();

	}
    #endregion

    #region Construction
    [Command]
	public void CmdConstructAt(int x, int y) {

        RpcConstructAt(x, y);

    }

    [ClientRpc]
    public void RpcConstructAt(int x, int y) {

        localPlayer.gameManager.ConstructAt(x, y);

    }
    #endregion

    #region Change Qin Energy
    [Command]
	public void CmdChangeQinEnergy(int amount) {

		RpcChangeQinEnergy (amount);

	}

	[ClientRpc]
	void RpcChangeQinEnergy(int amount) {

		SC_Qin.ChangeEnergy (amount);

	}
    #endregion

    #region Destroy Character
    [Command]
    public void CmdDestroyCharacter(GameObject c) {

        RpcDestroyCharacter(c);

    }

    [ClientRpc]
    void RpcDestroyCharacter(GameObject c) {

        c.GetComponent<SC_Character>().DestroyCharacter();

    }
    #endregion

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
