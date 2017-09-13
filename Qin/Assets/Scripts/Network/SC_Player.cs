using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class SC_Player : NetworkBehaviour {

	[SyncVar]
	bool qin;

	public override void OnStartLocalPlayer () {

		if (isLocalPlayer)
			tag = "Player";

	}

	public bool Turn(bool turn) {

		return (qin == (!turn));

	}

	public void SetSide(bool side) {

		qin = side;

	}

}
