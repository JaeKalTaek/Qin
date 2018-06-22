using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class SC_Lobby_Player : NetworkLobbyPlayer {

	bool side;

	public override void OnStartLocalPlayer () {
		
		if (isLocalPlayer) {

			side = (GameObject.FindObjectOfType<SC_Network_Manager> ().IsQinHost () == isServer);

			SendReadyToBeginMessage ();

		}

	}

	public bool GetSide() {

		return (isLocalPlayer) ? side : false;

	}

}
