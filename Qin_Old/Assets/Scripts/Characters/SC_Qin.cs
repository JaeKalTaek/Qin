using UnityEngine;
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

    public static bool SelfPanel { get; set; }

	static SC_GameManager gameManager;

	static SC_Tile_Manager tileManager;

	static SC_UI_Manager uiManager;

	public static SC_Qin Qin;

	void Start() {

		Qin = this;

		gameManager = FindObjectOfType<SC_GameManager> ();

		tileManager = FindObjectOfType<SC_Tile_Manager> ();

		uiManager = FindObjectOfType<SC_UI_Manager> ();

        Energy = startEnergy;

		uiManager.energyText.text = "Qin's Energy : " + Energy;

		tileManager.SetQin (this);

	}

	void OnMouseDown() {

		SC_Tile under = tileManager.GetTileAt (gameObject);

		if (under.CurrentDisplay == TDisplay.Attack) {
			
			SC_Tile attackingCharacterTile = tileManager.GetTileAt (SC_Character.attackingCharacter.gameObject);
			gameManager.rangedAttack = !tileManager.IsNeighbor(attackingCharacterTile, under);

            SC_Character.attackingCharacter.attackTarget = under;

			if (SC_Character.attackingCharacter.IsHero ()) {

				((SC_Hero)SC_Character.attackingCharacter).ChooseWeapon ();

			} else {

				foreach (SC_Tile tile in tileManager.tiles)
					tile.RemoveFilter ();

				gameManager.Attack ();

			}

		}

	}

	void OnMouseOver() {

		if(Input.GetMouseButtonDown(1))
			uiManager.ShowHideInfos(gameObject, GetType());

	}

	public static void UsePower(Vector3 pos) {

		SC_Hero hero = gameManager.lastHeroDead;

		hero.transform.SetPos(pos);
		hero.coalition = false;
		hero.powerUsed = false;
		hero.powerBacklash = 0;
		hero.SetBaseColor (new Color (255, 0, 205));
		hero.health = hero.maxHealth;
		hero.lifebar.UpdateGraph(hero.health, hero.maxHealth);
		hero.SetCanMove (true);
		hero.berserk = false;
		hero.berserkTurn = false;
		hero.UnTired ();

		Quaternion rotation = Quaternion.identity;
		rotation.eulerAngles = new Vector3(0, 0, 180);

		Quaternion lifebarRotation = Quaternion.identity;
		lifebarRotation.eulerAngles = hero.lifebar.transform.parent.rotation.eulerAngles;

		hero.transform.rotation = rotation;
		hero.lifebar.transform.parent.rotation = lifebarRotation;

		hero.gameObject.SetActive (true);

		ChangeEnergy(-Qin.powerCost);

		gameManager.lastHeroDead = null;

	}

	public static void ChangeEnergy(int amount) {

		Energy += amount;

		if (Energy >= Qin.energyToWin)
			uiManager.ShowVictory (true);
		else if (Energy > 0)
			uiManager.energyText.text = "Qin's Energy : " + Energy;
		else
			uiManager.ShowVictory (false);

	}

}
