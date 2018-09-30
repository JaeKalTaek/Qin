using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using UnityEngine.EventSystems;

public class SC_UI_Manager : MonoBehaviour {

    #region UI Elements
    [Header("Game")]
	public GameObject loadingPanel;
	public Text turnIndicator;
	public GameObject previewFightPanel;
	public GameObject endTurn;
	public GameObject victoryPanel;

	[Header("Characters")]
	public GameObject statsPanel;
    public GameObject actionsPanel;
    public GameObject attackButton;
    public GameObject destroyConstruButton;
    public GameObject buildConstruButton;
    public Button cancelButton;

	[Header("Heroes")]
	public GameObject relationshipPanel;
    public GameObject weaponChoicePanel;
    public GameObject weaponChoice1;
    public GameObject weaponChoice2;
	public GameObject usePower;

	[Header("Constructions")]
	public GameObject buildingInfosPanel;

	[Header("Qin")]
	public Text energyText;
	public GameObject qinPanel;
	public Transform construct;
    public Transform constructPanel;
    public Transform soldierConstructPanel;
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

    public static bool CanInteract { get { return SC_Player.localPlayer.Turn && !EventSystem.current.IsPointerOverGameObject(); } }

    public float clickSecurityDuration;

    bool clickSecurity;

    SC_Soldier[] soldiers;

    SC_Construction[] qinConstructions;

    public SC_Construction[] soldiersConstructions { get; set; }
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

			//usePower.SetActive (true);
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

        qinConstructions = Resources.LoadAll<SC_Construction>("Prefabs/Constructions");

        soldiersConstructions = Resources.LoadAll<SC_Construction>("Prefabs/Constructions/Production");

        SetupConstructPanel(true, constructPanel);

        SetupConstructPanel(false, soldierConstructPanel);

    }

    void SetupConstructPanel(bool qin, Transform panel) {

        SC_Construction[] constructions = qin ? qinConstructions : soldiersConstructions;

        for (int i = 0; i < panel.childCount; i++) {

            Transform construction = panel.GetChild(i);

            if (i < constructions.Length) {

                construction.GetChild(0).GetComponentInChildren<Text>().text = constructions[i].Name;
                construction.GetChild(1).GetComponentInChildren<Text>().text = constructions[i].cost.ToString();

            } else {

                construction.gameObject.SetActive(false);

            }

        }

    }
    #endregion

    #region Next Turn 
    public void NextTurn() {

        //usePower.SetActive (!gameManager.Qin && !SC_Player.localPlayer.Qin);

        cancelButton.gameObject.SetActive(false);

        if(gameManager.Qin) {

            SetButtonActivated("construct", true);
            SetButtonActivated("sacrifice", true);
            SetButtonActivated("qinPower", true);

        } else {

            construct.gameObject.SetActive(false);
            constructPanel.gameObject.SetActive(false);
            //qinPower.gameObject.SetActive(false);
            sacrifice.gameObject.SetActive(false);

        }

        turnIndicator.text = gameManager.Qin ? "Qin's Turn" : "Coalition's Turn n°" + ((gameManager.Turn % 3) + 1);

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

    public void SetCancelButton (Action a) {

        cancelButton.onClick.RemoveAllListeners();
        cancelButton.onClick.AddListener(delegate { a(); });
        cancelButton.gameObject.SetActive(true);

    }
    #endregion

    #region Infos
    public void ShowHideInfos(GameObject g, Type t) {

		if(HideInfos (g)) {

			if (t == typeof(SC_Hero))
				ShowHeroInfos (g.GetComponent<SC_Hero> ());
			else if (t == typeof(SC_Soldier))
				ShowSoldierInfos (g.GetComponent<SC_Soldier> ());
			else if (t.IsSubclassOf(typeof(SC_Construction)))
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
		SetText("Technique", " Technique : " + character.technique + ", Crit : " + character.CriticalAmount + "/" + gameManager.CommonCharactersVariables.critTrigger);
		SetText("Reflexes", " Reflexes : " + character.reflexes + ", Dodge : " + character.DodgeAmount + "/" + gameManager.CommonCharactersVariables.dodgeTrigger);
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
    // Also called by UI
    public void PreviewFight (bool activeWeapon) {

        SC_Character attacker = SC_Character.attackingCharacter;

        attacker.Hero?.SetWeapon(activeWeapon);

        previewFightPanel.SetActive(true);

        SetText("AttackerName", attacker.characterName);

        SetText("AttackerWeapon", attacker.GetActiveWeapon().weaponName);

        /*SetText ("AttackerCrit", attacker.CriticalAmount.ToString ());

		SetText ("AttackerDodge", attacker.DodgeAmount.ToString ());*/

        int attackedDamages = 0;

        int attackerDamages = attacker.GetActiveWeapon().weaponOrQi ? attacker.strength : attacker.qi;

        string attackedName = "";

        int attackedHP = 0;

        string attackedWeapon = "";

        /*string attackedCrit = "";

		string attackedDodge = "";*/

        if (attacker.AttackTarget.Character && (!attacker.AttackTarget.Construction || !attacker.AttackTarget.Bastion)) {

            SC_Character attacked = attacker.AttackTarget.Character;

            attackedName = attacked.characterName;

            attackedWeapon = attacked.GetActiveWeapon().weaponName;

            attackerDamages = fightManager.CalcDamages(attacker, attacked, false);

            if (!TileManager.GetTileAt(attacker.gameObject).Bastion && (fightManager.RangedAttack && attacked.GetActiveWeapon().ranged || !fightManager.RangedAttack && !attacked.GetActiveWeapon().IsBow))
                attackedDamages = fightManager.CalcDamages(attacked, attacker, true);

            attackedHP = attacked.Health - attackerDamages;

            /*attackedCrit = attacked.CriticalHit.ToString ();

			attackedDodge = attacked.DodgeHit.ToString ();*/

        } else {

            int attackedType = attacker.AttackTarget.Construction ? 0 : attacker.AttackTarget.Qin ? 1 : 2;

            attackedName = (attackedType == 0) ? attacker.AttackTarget.Construction.Name : (attackedType == 1) ? "Qin" : "";

            int attackedHealth = (attackedType == 0) ? attacker.AttackTarget.Construction.Health : (attackedType == 1) ? SC_Qin.Energy : 0;

            if (attackedType != 2)
                attackedHP = attackedHealth - attackerDamages;

        }

        SetText("AttackerHP", (Mathf.Max(attacker.Health - attackedDamages, 0)).ToString());

        SetText("AttackedName", attackedName);

        SetText("AttackedHP", Mathf.Max(attackedHP, 0).ToString());

        SetText("AttackerDamages", attackerDamages.ToString());
        SetText("AttackedDamages", attackedDamages.ToString());

        SetText("AttackedWeapon", attackedWeapon);

        /*SetText("AttackedCrit", attackedCrit);
		SetText("AttackedDodge", attackedDodge);*/

        attacker.Hero?.SetWeapon(activeWeapon);

    }

    // Also called by UI
    public void HidePreviewFight () {

        previewFightPanel.SetActive(false);

    }  
    #endregion

    #region Heroes
    /*public void ShowHeroPower(bool show, string heroName) {

		usePower.SetActive (!show);

		if (show)
			usePower.GetComponentInChildren<Text> ().name = heroName;

	}*/

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

        Attack();

    }

    public void HideWeapons () {

        weaponChoicePanel.SetActive(false);
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

            SC_Player.localPlayer.CmdSetQinTurnStarting(false);

            cancelButton.gameObject.SetActive(false);

            TileManager.RemoveAllFilters();

            SC_Player.localPlayer.CmdRemoveAllFiltersOnClient(false);

        }

    }

    public void EndQinAction(string action) {

        if (action != "workshop") {

            SC_Tile_Manager.Instance.RemoveAllFilters();

            SetButtonActivated(action, true);

        }

        if (action == "construct")
            cancelButton.gameObject.SetActive(false);

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
    /*public void DisplayResurrection () {

        if (!SC_Player.localPlayer.Busy && gameManager.LastHeroDead && (SC_Qin.Energy > SC_Qin.Qin.powerCost)) {

            StartQinAction("qinPower");

            TileManager.DisplayResurrection();

        }

    }*/
    #endregion

    #region Building
    public void UpdateQinConstructPanel () {

        for (int i = 0; i < constructPanel.childCount; i++)
            constructPanel.GetChild(i).GetComponentInChildren<Button>().interactable = (SC_Qin.GetConstruCost(qinConstructions[i].Name) < SC_Qin.Energy) && (TileManager.GetConstructableTiles(qinConstructions[i].Name == "Wall").Count > 0);

    }

    // Called by UI
    public void DisplayQinConstructPanel() {        

        UpdateQinConstructPanel();

        StartQinAction("construct");                  

    }

    // Called by UI
    public void DisplaySoldiersConstructPanel () {

        actionsPanel.SetActive(false);

        for (int i = 0; i < soldierConstructPanel.childCount; i++)
            soldierConstructPanel.GetChild(i).GetComponentInChildren<Button>().interactable = (SC_Qin.GetConstruCost(soldiersConstructions[i].Name) < SC_Qin.Energy) && (TileManager.GetConstructableTiles(soldiersConstructions[i].Name == "Wall").Count > 0);

        TileManager.RemoveAllFilters();

        soldierConstructPanel.gameObject.SetActive(true);

        SetCancelButton(CancelAction);

    }

    // Called by UI
    public void DisplayConstructableTiles(int id) {

        SC_Player.localPlayer.CmdSetConstru(qinConstructions[id].Name);

        TileManager.RemoveAllFilters();

        TileManager.DisplayConstructableTiles(qinConstructions[id].Name == "Wall");

    }
    #endregion

    #region Workshop
    public void DisplayWorkshopPanel() {

        Transform uiSoldiers = workshopPanel.transform.GetChild(1);

        for (int i = 0; i < uiSoldiers.childCount; i++)
            uiSoldiers.GetChild(i).GetComponentInChildren<Button>().interactable = soldiers[i].cost < SC_Qin.Energy;

        StartCoroutine(ClickSafety());

        StartQinAction("workshop");     

    }

    IEnumerator ClickSafety() {

        clickSecurity = true;

        yield return new WaitForSeconds(clickSecurityDuration);

        clickSecurity = false;


    }

    public void WorkshopCreateSoldier (int id) {

        if (!clickSecurity) {            

            SC_Player.localPlayer.CmdCreateSoldier(gameManager.CurrentWorkshopPos, soldiers[id].characterName);

            EndQinAction("workshop");

        }

    }
    #endregion

    #endregion

    #region Both Players
    void Update () {

        if (cancelButton.isActiveAndEnabled && Input.GetButtonDown("Cancel"))
            cancelButton.onClick.Invoke();

    }

    // Called by UI
    public void ToggleHealth () {

        foreach(SC_Lifebar lifebar in FindObjectsOfType<SC_Lifebar>())
            lifebar.Toggle();

    }

    public void ShowVictory (bool qinWon) {

        victoryPanel.GetComponentInChildren<Text>().text = (qinWon ? "Qin" : "The Heroes") + " won the war !";

        victoryPanel.SetActive(true);

    }

    public void Attack() {

        actionsPanel.SetActive(false);

        SetCancelButton(CancelAction);

        TileManager.CheckAttack();

    }

    void CancelAction() {

        soldierConstructPanel.gameObject.SetActive(false);

        TileManager.RemoveAllFilters();

        actionsPanel.SetActive(true);

        SetCancelButton(gameManager.ResetMovement);

        TileManager.PreviewAttack();

    }

    public void Wait() {

        SC_Player.localPlayer.CmdWait();

        actionsPanel.SetActive(false);

        cancelButton.gameObject.SetActive(false);

        SC_Player.localPlayer.Busy = false;

    }
    #endregion

    #region Utility functions
    void SetText (string id, string text) {

        GameObject.Find(id).GetComponent<Text>().text = text;

    }    
    #endregion

}
