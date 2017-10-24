using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class SC_Player : NetworkBehaviour {

	[SyncVar]
	bool qin;

	SC_GameManager gameManager;

	public override void OnStartLocalPlayer () {

		if (isLocalPlayer)			
			tag = "Player";

		FindObjectOfType<SC_UI_Manager> ().SetupUI (this, qin);

		gameManager = FindObjectOfType<SC_GameManager> ();

		//SC_GameManager.GetInstance ().SetPlayer (this);
		
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
