using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;
using UnityEngine.Networking.Types;

public class SC_Matchmaker : MonoBehaviour {

	public SC_Menu menuManager;
	bool selectedSide;
	ulong? createdMatchID;

	void Start() {

		selectedSide = false;
		createdMatchID = null;

	}

	public void QuickMatchmaking(bool qin) {



		NetworkManager.singleton.StartMatchMaker ();
		NetworkManager.singleton.matchMaker.ListMatches (0, 10, qin ? "Heroes" : "Qin", true, 0, 0, OnMatchList); 
		selectedSide = qin;

	}

	public void OnMatchList(bool success, string extendedInfo, List<MatchInfoSnapshot> matches) {

		if (matches.Count == 0) {

			if (createdMatchID == null) {

				createdMatchID = 0;
				NetworkManager.singleton.matchMaker.CreateMatch ("Qin", 2, true, "", "", "", 0, 0, OnMatchCreate);

			}

			QuickMatchmaking (selectedSide);

		} else {

			NetworkManager.singleton.matchMaker.JoinMatch (matches [0].networkId, "", "", "", 0, 0, OnMatchJoined);

		}

	}

	public void OnMatchCreate(bool success, string extendedInfo, MatchInfo matchInfo) {

		createdMatchID = (ulong?) matchInfo.networkId;

	}

	public void OnMatchJoined(bool success, string extendedInfo, MatchInfo matchInfo) {

		if (((ulong?)matchInfo.networkId) != createdMatchID)
			NetworkManager.singleton.matchMaker.DestroyMatch ((NetworkID)createdMatchID, 0, OnMatchDestroy);

	}

	public void OnMatchDestroy(bool success, string extendedInfo) { }

}
