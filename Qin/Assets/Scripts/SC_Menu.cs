using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SC_Menu : MonoBehaviour {

	public GameObject mainPanel, onlinePanel, qmPanel, searchGamePanel;

	void Awake() {

		mainPanel.SetActive (true);

	}

	void Update() {

		if (Input.GetKeyDown (KeyCode.Escape))
			GameObject.Find ("Back_Button").GetComponent<Button> ().onClick.Invoke ();

	}

	public void ShowPanel(GameObject panel) {

		foreach (Transform t in transform)
			t.gameObject.SetActive (t.name.Equals (panel.name));

	}
		
}
