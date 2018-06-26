using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Networking;

public class SC_TileManager : NetworkBehaviour {

	public static SC_TileManager instance;

	[Serializable]
	public struct TileStatus {

		public string name;

		public Color color;

	}

	public TileStatus[] tileStatus;

	void Start() {

		if (instance == null)
			instance = this;
		else
			Destroy (this);

		if (isServer) {

			for (int i = 0; i < 25; i++)
				for (int j = 0; j < 50; j++)
					NetworkServer.Spawn (Instantiate (Resources.Load<GameObject> ("Prefabs/P_Tile"), new Vector3(i * 9, 0, j * 9), Quaternion.identity, transform));			

		}

	}

	public Color GetStatusColor(string s) {

		Color c = new Color (0, 0, 0, 0);

		foreach (TileStatus tS in tileStatus)
			if (tS.name == s)
				c = tS.color;

		return c;

	}

}
