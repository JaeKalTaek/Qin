using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;
using UnityEngine.Networking.Types;

public class SC_Matchmaker : MonoBehaviour {

	public void QuickMatchmaking(bool qin) {

		NetworkManager.singleton.StartMatchMaker ();
		NetworkManager.singleton.matchMaker.ListMatches (0, 10, qin ? "Heroes" : "Qin", true, 0, 0, OnMatchList); 

	}

	public void OnMatchList(bool success, string extendedInfo, List<MatchInfoSnapshot> matches) {

		if (matches.Count == 0)
			NetworkManager.singleton.matchMaker.CreateMatch ("Qin", 2, true, "", "", "", 0, 0, OnMatchCreate);
		else
			NetworkManager.singleton.matchMaker.JoinMatch (matches [0].networkId, "", "", "", 0, 0, OnMatchJoined);

	}

	public void OnMatchCreate(bool success, string extendedInfo, MatchInfo matchInfo) { 

		NetworkManager.singleton.StartHost (matchInfo);

	}

	public void OnMatchJoined(bool success, string extendedInfo, MatchInfo matchInfo) {	

		NetworkManager.singleton.StartClient (matchInfo);
	
	}

	public void CancelMatchmaking() {

		NetworkManager.singleton.StopHost ();
		NetworkManager.singleton.StopMatchMaker ();

	}

}
