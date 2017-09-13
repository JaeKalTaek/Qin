using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.Networking.Match;

public class SC_Network_Manager : NetworkLobbyManager {

	bool isQinHost;
	bool side, quitting, stoppedMatchmaking;
	MatchInfo createdMatch = null;

	void OnEnable() {

		SceneManager.activeSceneChanged += OnSceneChanged;

	}

	public override void OnStopClient () {

		if (quitting)
			Application.Quit ();

	}

	public override bool OnLobbyServerSceneLoadedForPlayer (GameObject lobbyPlayer, GameObject gamePlayer) {

		gamePlayer.GetComponent<SC_Player> ().SetSide (lobbyPlayer.GetComponent<SC_Lobby_Player> ().GetSide());

		return true;

	}

	public override void OnClientDisconnect (NetworkConnection conn) {

		CancelMatchmaking ();

	}

	public override void OnServerDisconnect (NetworkConnection conn) {
		
		CancelMatchmaking ();

	}

	void OnSceneChanged(Scene previousScene, Scene newScene) {

		if (newScene.name.Equals(offlineScene) && stoppedMatchmaking) {

			SC_Menu menu = GameObject.FindObjectOfType<SC_Menu> ();
			
			menu.ShowPanel (menu.qmPanel);

			stoppedMatchmaking = false;

		}

	}

	public void QuickMatchmaking(bool qin) {

		side = qin;

		StartMatchMaker ();

		matchMaker.ListMatches (0, 10, side ? "Heroes" : "Qin", true, 0, 0, OnMatchList); 

	}

	public override void OnMatchList(bool success, string extendedInfo, List<MatchInfoSnapshot> matches) {

		if (matches.Count == 0)
			matchMaker.CreateMatch (side ? "Qin" : "Heroes", 2, true, "", "", "", 0, 0, OnMatchCreate);
		else
			matchMaker.JoinMatch (matches [0].networkId, "", "", "", 0, 0, OnMatchJoined);

	}

	public override void OnMatchCreate(bool success, string extendedInfo, MatchInfo matchInfo) { 

		isQinHost = side;

		createdMatch = matchInfo;

		StartHost (matchInfo);

		SC_Menu menu = GameObject.FindObjectOfType<SC_Menu> ();

		menu.ShowPanel (menu.searchGamePanel);

	}

	public override void OnMatchJoined(bool success, string extendedInfo, MatchInfo matchInfo) {	

		isQinHost = !side;

		StartClient (matchInfo);

	}

	public void CancelMatchmaking() {

		stoppedMatchmaking = true;

		if (createdMatch != null) {
			
			matchMaker.SetMatchAttributes (createdMatch.networkId, false, 0, OnSetMatchAttributes);

		} else {
				
			StopClient ();

			StopMatchMaker ();

		}

	}

	public override void OnSetMatchAttributes(bool success, string extendedInfo) {

		StopHost ();

		StopMatchMaker ();

		createdMatch = null;

	}

	void OnApplicationQuit() {

		if ((!quitting) && isNetworkActive) {

			quitting = true;

			Application.CancelQuit ();

			CancelMatchmaking ();

		}

	}

	public bool IsQinHost() {

		return isQinHost;

	}

}
