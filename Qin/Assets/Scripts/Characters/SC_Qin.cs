﻿using UnityEngine;
using UnityEngine.Networking;
using static SC_Enums;

public class SC_Qin : NetworkBehaviour {

	[Header("Qin variables")]
	[Tooltip("Energy of Qin at the start of the game")]
	public int startEnergy;
	public static int Energy { get; set; }

	[Tooltip("Energy necessary for Qin to win the game")]
	public int energyToWin;

	[Tooltip("Cost of the power of Qin")]
	public int powerCost;

	[Tooltip("Energy won when a hero dis")]
	public int energyWhenHeroDies;

    [Tooltip("Cost in energy for Qin to build a wall")]
    public int wallCost;

    [Tooltip("Energy won when Qin sacrifices a soldier")]
    public int sacrificeValue;

    [Tooltip("Cost in energy for Qin to create a soldier from a workshop")]
    public int soldierCost;

    [Tooltip("Energy won for each village at the beginning of each of Qin's turn")]
    public int regenPerVillage;

	static SC_Game_Manager gameManager;

	static SC_Tile_Manager tileManager;

	static SC_UI_Manager uiManager;

	public static SC_Qin Qin;

	void Start() {

		Qin = this;

		gameManager = FindObjectOfType<SC_Game_Manager> ();

		tileManager = FindObjectOfType<SC_Tile_Manager> ();

		uiManager = FindObjectOfType<SC_UI_Manager> ();

        Energy = startEnergy;

		uiManager.energyText.text = "Qin's Energy : " + Energy;

        tileManager.GetTileAt(gameObject).Qin = true;

    }

	void OnMouseOver() {

		if(Input.GetMouseButtonDown(1))
			uiManager.ShowHideInfos(gameObject, GetType());

	}

	public static void UsePower(Vector3 pos) {

		SC_Hero hero = gameManager.LastHeroDead;

		hero.transform.SetPos(pos);
		hero.coalition = false;
		hero.PowerUsed = false;
		hero.PowerBacklash = 0;
		hero.SetBaseColor (new Color (255, 0, 205));
		hero.Health = hero.maxHealth;
		hero.Lifebar.UpdateGraph(hero.Health, hero.maxHealth);
        hero.CanMove = true;
		hero.Berserk = false;
		hero.BerserkTurn = false;
		hero.UnTired ();

		Quaternion rotation = Quaternion.identity;
		rotation.eulerAngles = new Vector3(0, 0, 180);

		Quaternion lifebarRotation = Quaternion.identity;
		lifebarRotation.eulerAngles = hero.Lifebar.transform.parent.rotation.eulerAngles;

		hero.transform.rotation = rotation;
		hero.Lifebar.transform.parent.rotation = lifebarRotation;

		hero.gameObject.SetActive (true);

		ChangeEnergy(-Qin.powerCost);

		gameManager.LastHeroDead = null;

	}

	public static void ChangeEnergy(int amount) {

		Energy += amount;

        if (Energy >= Qin.energyToWin)
            uiManager.ShowVictory(true);
        else if (Energy > 0) {
            uiManager.energyText.text = "Qin's Energy : " + Energy;
            uiManager.TryRefreshInfos(Qin.gameObject, Qin.GetType());
        }  else
            uiManager.ShowVictory(false);

	}

}