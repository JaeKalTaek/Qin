using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SC_UI_Manager : MonoBehaviour {

	[Header("Game")]
	public Text turns;
	public Transform health;
	public GameObject previewFightPanel;
	public GameObject endTurn;

	[Header("Characters")]
	public GameObject statsPanel;
	public GameObject cancelMovementButton;
	public GameObject weaponChoice1;
	public GameObject cancelAttackButton;

	[Header("Heroes")]
	public GameObject relationshipPanel;
	public GameObject villagePanel;
	public GameObject weaponChoice2;
	public GameObject usePower;

	[Header("Constructions")]
	public GameObject buildingInfosPanel;

	[Header("Qin")]
	public Text energyText;
	public GameObject qinPanel;
	public Transform construct;
	public Transform qinPower;
	public Transform sacrifice;
	public GameObject workshopPanel;

	SC_Player player;

	public void SetupUI(SC_Player p, bool qin) {

		player = p;

		if (!player.IsQin()) {

			usePower.SetActive (true);
			endTurn.SetActive (true);

		}

	}

	public void NextTurn() {

		/*constructWallButton.SetActive (false);
		endConstructionButton.SetActive (false);
		powerQinButton.SetActive (false);
		cancelPowerQinButton.SetActive (false);
		sacrificeUnitButton.SetActive (false);
		cancelSacrificeButton.SetActive (false);

		if (player.Turn ()) {

			endTurn.SetActive (true);

			if (!player.IsQin ()) {

				usePower.SetActive (true);

			}

		}*/

	}

	public void ToggleButton(string id) {

		Transform parent = (Transform)typeof(SC_UI_Manager).GetField (id).GetValue(this);
		bool turnedOn = !parent.GetChild (0).gameObject.activeSelf;
		parent.GetChild (0).gameObject.SetActive (turnedOn);
		parent.GetChild (1).gameObject.SetActive (!turnedOn);

	}

	public void HideButton(string id) {

		Transform parent = (Transform)typeof(SC_UI_Manager).GetField (id).GetValue(this);
		parent.GetChild (0).gameObject.SetActive (false);
		parent.GetChild (1).gameObject.SetActive (false);

	}

	public void ShowButton(string id) {

		Transform parent = (Transform)typeof(SC_UI_Manager).GetField (id).GetValue(this);
		parent.GetChild (0).gameObject.SetActive (true);

	}

	public void SetTurnText(int turn) {

		turns.text = (((turn - 1) % 3) == 0) ? "1st Turn - Coalition" : (((turn - 2) % 3) == 0) ? "2nd Turn - Coalition" : "Turn Qin";

	}

	void SetText(string id, string text) {

		GameObject.Find (id).GetComponent<Text> ().text = text;

	}

}
