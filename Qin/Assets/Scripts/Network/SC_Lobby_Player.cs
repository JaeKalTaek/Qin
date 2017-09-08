using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class SC_Lobby_Player : NetworkLobbyPlayer {

	public override void OnStartLocalPlayer () {
		
		if (isLocalPlayer) SendReadyToBeginMessage ();

	}

}
