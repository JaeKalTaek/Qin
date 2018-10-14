using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using UnityEngine.EventSystems;
using static SC_Global;

public class SC_UI_Manager : MonoBehaviour {

    #region UI Elements
    [Header("Game")]
	public GameObject loadingPanel;
    public GameObject connectingPanel;
    public Text turnIndicator;
	public GameObject previewFightPanel;
	public GameObject endTurn;
	public GameObject victoryPanel;
    public GameObject playerActionsPanel;

	[Header("Characters")]
	public GameObject statsPanel;
    public GameObject characterActionsPanel;
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

    public static bool CanInteract { get { return SC_Player.localPlayer.Turn && (!EventSystem.current.IsPointerOverGameObject() || !Cursor.visible); } }

    public float clickSecurityDuration;

    bool clickSecurity;

    SC_Soldier[] soldiers;

    SC_Construction[] qinConstructions;

    public SC_Construction[] SoldiersConstructions { get; set; }
    #endregion

    #region Setup
    private void Awake() {

        Instance = this;

    }

    public void SetupUI(bool qin) {       

        gameManager = SC_Game_Manager.Instance;

        TileManager = SC_Tile_Manager.Instance;

        fightManager = SC_Fight_Manager.Instance;

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

        SoldiersConstructions = Resources.LoadAll<SC_Construction>("Prefabs/Constructions/Production");

        SetupConstructPanel(true, constructPanel);

        SetupConstructPanel(false, soldierConstructPanel);

        if (gameManager.Qin) {

            SetButtonActivated("construct", true);
            SetButtonActivated("sacrifice", true);
            //SetButtonActivated("qinPower", true);

        }

    }

    void SetupConstructPanel(bool qin, Transform panel) {

        SC_Construction[] constructions = qin ? qinConstructions : SoldiersConstructions;

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

        playerActionsPanel.SetActive(false);

        //usePower.SetActive (!gameManager.Qin && !SC_Player.localPlayer.Qin);

        cancelButton.gameObject.SetActive(false);

        turnIndicator.text = gameManager.Qin ? "Qin's Turn" : (gameManager.Turn % 3 == 1 ? "1st" : "2nd") + " Coalition's Turn";

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

    public void SetCancelButton (Action a) {

        cancelButton.onClick.RemoveAllListeners();
        cancelButton.onClick.AddListener(delegate { a(); });
        cancelButton.gameObject.SetActive(true);

    }
    #endregion

    #region Infos
    public void ShowInfos(GameObject g, Type t) {

        CurrentGameObject = g;

        if (t == typeof(SC_Hero))
            ShowHeroInfos(g.GetComponent<SC_Hero>());
        else if (t == typeof(SC_Soldier))
            ShowSoldierInfos(g.GetComponent<SC_Soldier>());
        else if (t.IsSubclassOf(typeof(SC_Construction)))
            ShowConstructionsInfos(g.GetComponent<SC_Construction>());
        else if (t == typeof(SC_Qin))
            ShowQinInfos();
        else if (t == typeof(SC_Ruin)) {

            buildingInfosPanel.SetActive(true);

            SetText("BuildingName", "Ruin");
            SetText("BuildingHealth", "");

        } else
            print("ERRROR");

	}

    public void HideInfosIfActive(GameObject g) {

        if (CurrentGameObject == g)
            HideInfos(true);

    }

	public void HideInfos(bool removeFilters) {

        if (removeFilters)
            TileManager.RemoveAllFilters();

		statsPanel.SetActive (false);
		relationshipPanel.SetActive (false);
		buildingInfosPanel.SetActive (false);
		qinPanel.SetActive (false);

        CurrentGameObject = null;

	}

    public void TryRefreshInfos(GameObject g, Type t) {

        if(CurrentGameObject == g) {

            CurrentGameObject = null;

            ShowInfos(g, t);

        }

    }

	void ShowCharacterInfos(SC_Character character) {

		statsPanel.SetActive (true);

		SetText("Name", character.characterName);
		SetText("Health", "Health : " + character.Health + " / " + character.maxHealth);
		SetText("Strength", " Strength : " + GetFightStat(character, "strength"));
		SetText("Armor", " Armor : " + GetFightStat(character, "armor"));
		SetText("Qi", " Qi : " + GetFightStat(character, "qi"));
		SetText("Resistance", " Resistance : " + GetFightStat(character, "resistance"));
		SetText("Technique", " Technique : " + GetFightStat(character, "technique") + ", Crit : " + character.CriticalAmount + "/" + gameManager.CommonCharactersVariables.critTrigger);
		SetText("Reflexes", " Reflexes : " + GetFightStat(character, "reflexes") + ", Dodge : " + character.DodgeAmount + "/" + gameManager.CommonCharactersVariables.dodgeTrigger);
        SetText("Movement", " Movement : " + GetModifiedStat(character.baseMovement, character.Movement - character.baseMovement));
		SetText("WeaponsTitle", " Weapons :");

        if (SC_Tile.CanChangeFilters)
            TileManager.DisplayMovementAndAttack(character, true);

    }

    string GetFightStat(SC_Character chara, string stat) {

        return GetModifiedStat((int)typeof(SC_Character).GetField(stat).GetValue(chara), (int)typeof(CombatModifiers).GetField(stat).GetValue(chara.Modifiers));

    }

    string GetModifiedStat(int baseStat, int modifier) {

        return (baseStat + modifier) + (modifier == 0 ? "" : (" (" + baseStat + " " + (modifier > 0 ? "+" : "-") + " " + Mathf.Abs(modifier) + ")"));

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

        if (SC_Tile.CanChangeFilters && construction.Pump) {

            TileManager.DisplayedPump = construction.Pump;

            foreach (SC_Tile tile in TileManager.GetRange(construction.transform.position, construction.Pump.range))
                tile.SetFilter(TDisplay.PumpRange);

        }

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

        if (attacker.AttackTarget.Character && !attacker.AttackTarget.GreatWall) {

            SC_Character attacked = attacker.AttackTarget.Character;

            attackedName = attacked.characterName;

            attackedWeapon = attacked.GetActiveWeapon().weaponName;

            attackerDamages = fightManager.CalcDamages(attacker, attacked, false);

            if (!attacker.Tile.GreatWall && (fightManager.RangedAttack && attacked.GetActiveWeapon().ranged || !fightManager.RangedAttack && !attacked.GetActiveWeapon().IsBow))
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

            SetButtonActivated(action, false);

            constructPanel.gameObject.SetActive(action == "construct");

            workshopPanel.SetActive(action == "workshop");

            SC_Player.localPlayer.CmdSetQinTurnStarting(false);

            cancelButton.gameObject.SetActive(false);

            TileManager.RemoveAllFilters();

            //SC_Player.localPlayer.CmdRemoveAllFiltersOnClient(false);

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

        characterActionsPanel.SetActive(false);

        for (int i = 0; i < soldierConstructPanel.childCount; i++)
            soldierConstructPanel.GetChild(i).GetComponentInChildren<Button>().interactable = (SC_Qin.GetConstruCost(SoldiersConstructions[i].Name) < SC_Qin.Energy) && (TileManager.GetConstructableTiles(SoldiersConstructions[i].Name == "Wall").Count > 0);

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

        if (Input.GetButtonDown("Action") || Input.GetButtonDown("Infos") || Input.GetButtonDown("Cancel"))
            TileManager.HidePumpRange();

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

        SC_Cursor.Instance.Locked = false;

        characterActionsPanel.SetActive(false);

        SetCancelButton(CancelAction);

        TileManager.CheckAttack();

    }

    void CancelAction() {

        SC_Cursor.Instance.Locked = true;

        soldierConstructPanel.gameObject.SetActive(false);

        TileManager.RemoveAllFilters();

        characterActionsPanel.SetActive(true);

        SetCancelButton(gameManager.ResetMovement);

        TileManager.PreviewAttack();

    }

    public void Wait() {

        SC_Cursor.Instance.Locked = false;

        SC_Player.localPlayer.CmdWait();

        characterActionsPanel.SetActive(false);

        cancelButton.gameObject.SetActive(false);

        SC_Player.localPlayer.Busy = false;

    }
    #endregion

    #region Utility functions
    void SetText (string id, string text) {

        GameObject.Find(id).GetComponent<Text>().text = text;

    }
    #endregion

    #region Menu Position
    
    //Move the menu next to the tile
    public void MenuPos(GameObject menu) {

        RectTransform Rect = menu.GetComponent<RectTransform>();

        //Get the viewport position of the tile
        Vector3 currentTileViewportPos = Camera.main.WorldToViewportPoint(TileManager.GetTileAt(SC_Cursor.Instance.gameObject).transform.position);

        //If tile on the left side of the screen, offset the menu on the right
        //If tile on the right side of the screen, offset the menu on the left
        int offset = currentTileViewportPos.x < 0.5 ? 1 : -1;

        Rect.anchorMin = new Vector3(currentTileViewportPos.x + (offset * (0.1f + (0.05f*(1/(Mathf.Pow(Camera.main.orthographicSize, Camera.main.orthographicSize/4)))))), currentTileViewportPos.y, currentTileViewportPos.z);
        Rect.anchorMax = Rect.anchorMin;

        menu.SetActive(true);

    }

    #endregion
}
