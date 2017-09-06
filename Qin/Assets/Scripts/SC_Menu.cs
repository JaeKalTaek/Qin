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

	public void OnlineGame() {

		mainPanel.SetActive (false);
		qmPanel.SetActive (false);
		onlinePanel.SetActive (true);

	}

	public void QuickMatchmaking() {

		onlinePanel.SetActive (false);
		qmPanel.SetActive (true);

	}

	public void SearchingGame() {

		qmPanel.SetActive (false);
		searchGamePanel.SetActive (true);

	}

	public void Back(string panel) {

		foreach (Transform t in transform)
			t.gameObject.SetActive (t.name.Equals (panel));

	}

}
