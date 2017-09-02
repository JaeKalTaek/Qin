using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class SC_Functions {

	public static void SetText(string id, string text) {

		GameObject.Find (id).GetComponent<Text> ().text = text;

	}

}
