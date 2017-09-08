using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class SC_Network_Manager : NetworkLobbyManager {

	public override void OnLobbyServerPlayersReady () {
		
		base.OnLobbyServerPlayersReady ();

		print ("Players are ready !");

	}

}
