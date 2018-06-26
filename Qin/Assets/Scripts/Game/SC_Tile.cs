using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class SC_Tile : NetworkBehaviour {

	SpriteRenderer statusSprite;

	[SyncVar(hook="OnStatusChanged")]
	public string status = "";

	void Start() {

		statusSprite = GetComponentInChildren<SpriteRenderer> ();

	}

	void OnMouseDown() {

		CmdMouseDown ();

	}

	[Command]
	void CmdMouseDown() {

		status = (status == "") ? "destination" : "";

	}

	void OnStatusChanged(string newStatus) {

		status = newStatus;

		statusSprite.color = SC_TileManager.instance.GetStatusColor (status);

	}

}
