using UnityEngine;
using UnityEngine.UI;
using System;
using static SC_Enums;
using System.Collections;
using UnityEngine.EventSystems;

public class SC_UI_Manager : MonoBehaviour {

    #region UI Elements
    [Header("Game")]
	public GameObject loadingPanel;
	public Text turns;
	public GameObject previewFightPanel;
	public GameObject endTurn;
	public GameObject victoryPanel;

	[Header("Characters")]
	public GameObject statsPanel;
    public GameObject cancelMovementButton;
    public GameObject resetMovementButton;
	public GameObject weaponChoice1;
	public GameObject resetAttackChoiceButton;

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
    public Transform constructPanel;
    public GameObject cancelLastConstructButton;
	public Transform qinPower;
	public Transform sacrifice;
	public GameObject workshopPanel;
    #endregion

    #region Variables
    public GameObject CurrentGameObject { get; set; }

	static SC_Game_Manager gameManager;

    public SC_Tile_Manager TileManager { get; set; }

    static SC_Fight_Manager fightManager;

    public static SC_UI_Manager Instance { get; set; }

    public static bool CanInteract { get { return SC_Player.localPlayer.Turn && !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(); } }

    public float clickSecurityDuration;

    bool clickSecurity;

    SC_Soldier[] soldiers;
    #endregion

    #region Setup
    private void Awake() {

        Instance = this;

    }

    public void SetupUI(bool qin) {       

        gameManager = SC_Game_Manager.Instance;

        TileManager = SC_Tile_Manager.Instance;

        fightManager = SC_Fight_Manager.Instance;

        if (!qin) {

			usePower.SetActive (true);
			endTurn.SetActive (true);

		}

        soldiers = Resources.LoadAll<SC_Soldier>("Prefabs/Characters/Soldiers");

        for (int i = 0; i < workshopPanel.transform.GetChild(1).childCount; i++) {

            Transform soldier = workshopPanel.transform.GetChild(1).GetChild(i);

            if (i < soldiers.Length) {

                soldier.GetChild(0).GetComponentInChildren<Text>().text = soldiers[i].characterName;
                soldier.GetChild(1).GetComponentInChildren<Text>().text = soldiers[i].cost.ToString();

            } else {

                soldier.gameObject.SetActive(false);

            }

        }

	}
    #endregion

    #region Next Turn 
    public void NextTurn() {

		HideWeapons();

		villagePanel.SetActive (false);
		usePower.SetActive (!gameManager.Qin && !SC_Player.localPlayer.Qin);
        cancelMovementButton.SetActive(false);
		resetMovementButton.SetActive (false);
		resetAttackChoiceButton.SetActive (false);

        if(gameManager.Qin) {

            SetButtonActivated("construct", true);
            SetButtonActivated("sacrifice", true);
            SetButtonActivated("qinPower", true);

        } else {

            construct.gameObject.SetActive(false);
            constructPanel.gameObject.SetActive(false);
            cancelLastConstructButton.SetActive(false);
            qinPower.gameObject.SetActive(false);
            sacrifice.gameObject.SetActive(false);

        }

        turns.text = gameManager.Qin ? "Qin's Turn" : "Coalition's Turn n�" + ((gameManager.Turn % 3) + 1);

        endTurn.SetActive(SC_Player.localPlayer.Turn && !SC_Player.localPlayer.Qin);

	}
    #endregion

    #region Buttons
    /*public void ToggleButton(string id) {

        foreach(Transform t in (Transform)typeof(SC_UI_Manager).GetField(id).GetValue(this))
            t.gameObject.SetActive(t.gameObject.activeSelf);

	}*/

    public void SetButtonActivated(string id, bool active) {

        ((Transform)typeof(SC_UI_Manager).GetField(id).GetValue(this)).GetChild(0).gameObject.SetActive(active);
        ((Transform)typeof(SC_UI_Manager).GetField(id).GetValue(this)).GetChild(1).gameObject.SetActive(!active);

    }

    public void SetButtonActivated(string b, string id) {

        SetButtonActivated(b, b != id);

    }
    #endregion

    #region Infos
    public void ShowHideInfos(GameObject g, Type t) {

		if(HideInfos (g)) {

			if (t == typeof(SC_Hero))
				ShowHeroInfos (g.GetComponent<SC_Hero> ());
			else if (t == typeof(SC_Soldier))
				ShowSoldierInfos (g.GetComponent<SC_Soldier> ());
			else if (t == typeof(SC_Construction))
				ShowConstructionsInfos (g.GetComponent<SC_Construction> ());
			else if (t == typeof(SC_Qin))
				ShowQinInfos ();
			else
				print ("ERRROR");

		}

	}

    public void HideInfosIfActive(GameObject g) {

        if (CurrentGameObject == g)
            HideInfos(g);

    }

	public bool HideInfos(GameObject g) {

		statsPanel.SetActive (false);
		relationshipPanel.SetActive (false);
		buildingInfosPanel.SetActive (false);
		qinPanel.SetActive (false);

        CurrentGameObject = (CurrentGameObject == g) ? null : g;

        return CurrentGameObject;

	}

    public void TryRefreshInfos(GameObject g, Type t) {

        if(CurrentGameObject == g) {

            CurrentGameObject = null;

            ShowHideInfos(g, t);

        }

    }

	void ShowCharacterInfos(SC_Character character) {

		statsPanel.SetActive (true);

		SetText("Name", character.characterName);
		SetText("Health", "Health : " + character.Health + " / " + character.maxHealth);
		SetText("Strength", " Strength : " + character.strength);
		SetText("Armor", " Armor : " + character.armor);
		SetText("Qi", " Qi : " + character.qi);
		SetText("Resistance", " Resistance : " + character.resistance);
		SetText("Technique", " Technique : " + character.technique + ", Crit Jauge : " + character.CriticalAmount + "/" + gameManager.CommonCharactersVariables.critTrigger);
		SetText("Reflexes", " Reflexes : " + character.reflexes + ", Dodge Jauge : " + character.DodgeAmount + "/" + gameManager.CommonCharactersVariables.dodgeTrigger);
        SetText("Movement", " Movement : " + character.movement);
		SetText("WeaponsTitle", " Weapons :");

	}

	void ShowHeroInfos(SC_Hero hero) {

		ShowCharacterInfos (hero);

		relationshipPanel.SetActive (true);

		SetText("Weapon 1", "  - " + hero.GetWeapon(true).weaponName + " (E)");
		SetText("Weapon 2", "  - " + hero.GetWeapon(false).weaponName);

		for (int i = 0; i < hero.RelationshipKeys.Count; i++) {

			int value;
			hero.Relationships.TryGetValue(hero.RelationshipKeys [i], out value);
			GameObject.Find ("Relation_" + (i + 1)).GetComponent<Text> ().text = "  " + hero.RelationshipKeys [i] + " : " + value;

		}


	}

	void ShowSoldierInfos(SC_Soldier soldier) {

		ShowCharacterInfos (soldier);

		SetText("Weapon 1", "  - " + soldier.weapon.weaponName);
		SetText("Weapon 2", "");

	}     

	void ShowConstructionsInfos(SC_Construction construction) {

		buildingInfosPanel.SetActive (true);

		SetText("BuildingName", construction.Name);
		SetText("BuildingHealth", construction.Health != 0 ? "Health : " + construction.Health + " / " + construction.maxHealth : "");

	}

	void ShowQinInfos() {

		qinPanel.SetActive (true);

		SetText("QinEnergy", SC_Qin.Energy + "");

	}
    #endregion

    #region Fight related
    public void PreviewFight(SC_Character attacker, bool rangedAttack) {

		previewFightPanel.SetActive (true);

		SetText ("AttackerName", attacker.characterName);

		SetText ("AttackerWeapon", attacker.GetActiveWeapon ().weaponName);

		/*SetText ("AttackerCrit", attacker.CriticalAmount.ToString ());

		SetText ("AttackerDodge", attacker.DodgeAmount.ToString ());*/

		int attackedDamages = 0;

		int attackerDamages = attacker.GetActiveWeapon().weaponOrQi ? attacker.strength : attacker.qi;

		string attackedName = "";

		string attackedHP = "";

		string attackedWeapon = "";

		/*string attackedCrit = "";

		string attackedDodge = "";*/

		if (attacker.AttackTarget.Character && (!attacker.AttackTarget.Construction || !attacker.AttackTarget.Bastion)) {

			SC_Character attacked = attacker.AttackTarget.Character;

			attackedName = attacked.characterName;

			attackedWeapon = attacked.GetActiveWeapon ().weaponName;

			attackerDamages = fightManager.CalcDamages (attacker, attacked, false);
			
			if (!TileManager.GetTileAt(attacker.gameObject).Bastion && (rangedAttack && attacked.GetActiveWeapon ().ranged || !rangedAttack && !attacked.GetActiveWeapon ().IsBow))
                attackedDamages = fightManager.CalcDamages(attacked, attacker, true);

            attackedHP = (attacked.Health - attackerDamages).ToString ();			

			/*attackedCrit = attacked.CriticalHit.ToString ();

			attackedDodge = attacked.DodgeHit.ToString ();*/

		} else {

            int attackedType = attacker.AttackTarget.Construction ? 0 : attacker.AttackTarget.Qin ? 1 : 2;

			attackedName = (attackedType == 0) ? attacker.AttackTarget.Construction.Name : (attackedType == 1) ? "Qin" : "";			

			int attackedHealth = (attackedType == 0) ? attacker.AttackTarget.Construction.Health : (attackedType == 1) ? SC_Qin.Energy : 0;

			if (attackedType != 2) attackedHP = (attackedHealth - attackerDamages).ToString ();

		}

		SetText("AttackerHP", (Mathf.Max(attacker.Health - attackedDamages, 0)).ToString());

		SetText("AttackedName", attackedName);

		SetText("AttackedHP", attackedHP);

        SetText("AttackerDamages", attackerDamages.ToString());
		SetText("AttackedDamages", attackedDamages.ToString());

		SetText("AttackedWeapon", attackedWeapon);

		/*SetText("AttackedCrit", attackedCrit);
		SetText("AttackedDodge", attackedDodge);*/

	}

    // Also called by UI
    public void PreviewFight (bool activeWeapon) {

        SC_Character.attackingCharacter?.Hero.SetWeapon(activeWeapon);

        PreviewFight(SC_Character.attackingCharacter, fightManager.RangedAttack);

        SC_Character.attackingCharacter?.Hero.SetWeapon(activeWeapon);

    }

    // Also called by UI
    public void HidePreviewFight () {

        previewFightPanel.SetActive(false);

    }  
    #endregion

    #region Heroes
    public void ShowHeroPower(bool show, string heroName) {

		usePower.SetActive (!show);

		if (show)
			usePower.GetComponentInChildren<Text> ().name = heroName;

	}

    #region Weapons
    public void ShowWeapon (SC_Weapon weapon, bool first) {

        if (first)
            weaponChoice1.SetActive(true);
        else
            weaponChoice2.SetActive(true);

        SetText("Weapon Choice " + (first ? "1" : "2") + " Text", weapon.weaponName);

    }

    public void ResetAttackChoice () {

        HideWeapons();

        TileManager.CheckAttack();

        resetMovementButton.SetActive(!gameManager.CantCancelMovement);

        resetAttackChoiceButton.SetActive(false);

    }

    public void HideWeapons () {

        weaponChoice1.SetActive(false);
        weaponChoice2.SetActive(false);
        previewFightPanel.SetActive(false);

    }
    #endregion
    #endregion

    #region Qin
    #region Actions
    public void StartQinAction(string action) {

        if (!SC_Player.localPlayer.Busy) {

            SC_Player.localPlayer.Busy = true;

            SetButtonActivated("construct", action);
            SetButtonActivated("sacrifice", action);
            SetButtonActivated("qinPower", action);

            constructPanel.gameObject.SetActive(action == "construct");

            workshopPanel.SetActive(action == "workshop");

            resetMovementButton.SetActive(false);

            StopCancelConstruct();

            TileManager.RemoveAllFilters();

            SC_Player.localPlayer.CmdRemoveAllFiltersOnClient(false);

            SC_Character.CancelAttack();

        }

    }

    public void EndQinAction(string action) {

        if (action != "workshop") {

            SC_Tile_Manager.Instance.RemoveAllFilters();

            SetButtonActivated(action, true);

        }

        if (action == "construct")
            StopCancelConstruct();

        constructPanel.gameObject.SetActive(false);

        workshopPanel.SetActive(false);

        SC_Player.localPlayer.Busy = false;

    }

    // Called by UI
    public void DisplaySacrifices () {

        if (!SC_Player.localPlayer.Busy) {

            StartQinAction("sacrifice");

            TileManager.DisplaySacrifices();

        }

    }

    // Called by UI
    public void DisplayResurrection () {

        if (!SC_Player.localPlayer.Busy && gameManager.LastHeroDead && (SC_Qin.Energy > SC_Qin.Qin.powerCost)) {

            StartQinAction("qinPower");

            TileManager.DisplayResurrection();

        }

    }
    #endregion

    #region Building
    public void UpdateConstructPanel () {

        foreach (Transform t in constructPanel) {

            t.GetComponentInChildren<Button>().interactable = (SC_Qin.GetConstruCost(t.name) < SC_Qin.Energy) && (TileManager.GetConstructableTiles(t.name == "Wall").Count > 0);

            t.GetChild(1).GetComponent<Text>().text = SC_Qin.GetConstruCost(t.name).ToString();

        }

    }

    // Called by UI
    public void DisplayConstructPanel() {        

        UpdateConstructPanel();

        StartQinAction("construct");                  

    }

    // Called by UI
    public void DisplayConstructableTiles(int c) {

        SC_Player.localPlayer.CmdSetConstru(c);

        TileManager.RemoveAllFilters();

        TileManager.DisplayConstructableTiles((Constru)c == Constru.Wall);

    }

    public void StopCancelConstruct() {

        gameManager.QinTurnBeginning = false;

        cancelLastConstructButton.SetActive(false);

    }
    #endregion

    #region Workshop
    public void DisplayWorkshopPanel() {

        Transform uiSoldiers = workshopPanel.transform.GetChild(1);

        for (int i = 0; i < uiSoldiers.childCount; i++)
            if (uiSoldiers.GetChild(i).gameObject.activeSelf)
                uiSoldiers.GetChild(i).GetComponentInChildren<Button>().interactable = soldiers[i].cost < SC_Qin.Energy;

        StartCoroutine(ClickSafety());

        StartQinAction("workshop");     

    }

    IEnumerator ClickSafety() {

        clickSecurity = true;

        yield return new WaitForSeconds(clickSecurityDuration);

        clickSecurity = false;


    }

    public void WorkshopCreateSoldier () {

        if (!clickSecurity) {

            SC_Player.localPlayer.CmdCreateSoldier(gameManager.CurrentWorkshop.transform.position, EventSystem.current.currentSelectedGameObject.GetComponentInChildren<Text>().text);

            EndQinAction("workshop");

        }

    }
    #endregion

    #endregion

    #region Both Players
    // Called by UI
    public void ToggleHealth() {

        foreach(SC_Lifebar lifebar in FindObjectsOfType<SC_Lifebar>())
            lifebar.Toggle();

    }

    public void ShowVictory (bool qinWon) {

        victoryPanel.GetComponentInChildren<Text>().text = (qinWon ? "Qin" : "The Heroes") + " won the war !";

        victoryPanel.SetActive(true);

    }
    #endregion

    #region Utility functions
    void SetText (string id, string text) {

        GameObject.Find(id).GetComponent<Text>().text = text;

    }
    #endregion

}
