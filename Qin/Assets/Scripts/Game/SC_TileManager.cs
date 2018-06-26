using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SC_TileManager : MonoBehaviour {

	public static SC_TileManager instance;

	[Serializable]
	public struct TileStatus {

		public string name;

		public Color color;

	}

	public TileStatus[] tileStatus;

	void Awake() {

		if (instance == null)
			instance = this;
		else
			Destroy (this);

	}

	public Color GetStatusColor(string s) {

		Color c = new Color (0, 0, 0, 0);

		foreach (TileStatus tS in tileStatus)
			if (tS.name == s)
				c = tS.color;

		return c;

	}

}
