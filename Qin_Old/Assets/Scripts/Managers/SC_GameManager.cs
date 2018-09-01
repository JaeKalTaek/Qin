using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using static SC_Enums;

public class SC_GameManager : NetworkBehaviour {

    //Prefabs
	public GameObject baseMapPrefab;
    public GameObject plainPrefab, forestPrefab, mountainPrefab, palacePrefab;
    public GameObject qinPrefab, soldierPrefab, convoyPrefab;
	public List<GameObject> heroPrefabs;
	public GameObject bastionPrefab, wallPrefab, workshopPrefab, villagePrefab;
	public GameObject tileManagerPrefab;
    
	//Instance
    static SC_GameManager instance;

    //Variables used to determine the movements possible
    List<SC_Tile> openList = new List<SC_Tile>();
	List<SC_Tile> closedList = new List<SC_Tile>();
	Dictionary<SC_Tile, int> movementPoints = new Dictionary<SC_Tile, int>();

    int turn;

	//Other
	public bool Bastion { get; set; }

	public SC_Hero lastHeroDead { get; set; }

    public bool rangedAttack { get; set; }

	public SC_Workshop currentWorkshop { get; set; }

	public bool cantCancelMovement { get; set; }

    [SerializeField]
	public SC_Player player { get; set; }

	SC_UI_Manager uiManager;

	SC_Tile_Manager tileManager;

	public SC_Character characterToMove { get; set; }

	#region Setup
    void Start() {

		turn = 1;

		if(GameObject.FindGameObjectWithTag ("Player")) {
			
			player = GameObject.FindGameObjectWithTag ("Player").GetComponent<SC_Player> ();

            player.SetSide();

			player.SetGameManager (this);

		}

		if (isServer) {

			GenerateMap ();
			SetupTileManager ();

		}

		Bastion = true;

		if (instance == null)
			instance = this;

		uiManager = FindObjectOfType<SC_UI_Manager> ();
		uiManager.SetupUI (FindObjectOfType<SC_Network_Manager>().IsQinHost() == isServer);

    }

	void GenerateMap() {

		foreach (Transform child in baseMapPrefab.transform) {

			SC_EditorTile eTile = child.GetComponent<SC_EditorTile> ();

			GameObject tilePrefab = (eTile.tileType == tileType.Plain) ? plainPrefab : (eTile.tileType == tileType.Forest) ? forestPrefab : (eTile.tileType == tileType.Mountain) ? mountainPrefab : palacePrefab;

			GameObject go = Instantiate (tilePrefab, new Vector3(eTile.transform.position.x, eTile.transform.position.y, 0), eTile.transform.rotation,  GameObject.Find ("Tiles").transform);

			NetworkServer.Spawn (go);

		}

	}

	void SetupTileManager() {

		GameObject tm = Instantiate (tileManagerPrefab);
		SC_Tile_Manager stm = tm.GetComponent<SC_Tile_Manager> ();

		stm.xSize = baseMapPrefab.GetComponent<SC_MapPrefab>().xSize;
		stm.ySize = baseMapPrefab.GetComponent<SC_MapPrefab>().ySize;

		FindObjectOfType<SC_Camera> ().Setup (stm.xSize, stm.ySize);

		NetworkServer.Spawn (tm);

	}

	public void FinishSetup() {

		tileManager = FindObjectOfType<SC_Tile_Manager> ();

		if (isServer) {

			GenerateBuildings ();
			GenerateCharacters ();

		}

	}

	void GenerateBuildings() {

		foreach (Transform child in baseMapPrefab.transform) {

			SC_EditorTile eTile = child.GetComponent<SC_EditorTile> ();

			if (eTile.construction != constructionType.None) {

				GameObject constructionPrefab = (eTile.construction == constructionType.Village) ? eTile.villagePrefab : (eTile.construction == constructionType.Workshop) ? eTile.workshopPrefab : (eTile.construction == constructionType.Bastion) ? eTile.bastionPrefab : null;

				GameObject go2 = Instantiate (constructionPrefab);

				go2.transform.SetPos (eTile.transform);

				go2.transform.parent = (eTile.construction == constructionType.Village) ? GameObject.Find ("Villages").transform : (eTile.construction == constructionType.Workshop) ? GameObject.Find ("Workshops").transform : GameObject.Find("Bastions").transform;

				NetworkServer.Spawn (go2);

			}

		}

		foreach (SC_Construction construction in FindObjectsOfType<SC_Construction>())
			NetworkServer.Spawn (construction.gameObject);

	}

    void GenerateCharacters() {

		foreach (Transform child in baseMapPrefab.transform) {

			SC_EditorTile eTile = child.GetComponent<SC_EditorTile> ();

			if (eTile.spawnSoldier || eTile.qin || (eTile.heroPrefab != null)) {

				GameObject go2 = Instantiate ((eTile.spawnSoldier) ? soldierPrefab : (eTile.qin) ? qinPrefab : eTile.heroPrefab);

				go2.transform.SetPos (eTile.transform);

				go2.transform.parent = (eTile.spawnSoldier) ? GameObject.Find ("Soldiers").transform : (eTile.qin) ? null : GameObject.Find ("Heroes").transform;

				NetworkServer.Spawn (go2);

			}

		}		

    }
    #endregion

	#region Display Movements
    public void CheckMovements(SC_Character target) {

        cantCancelMovement = false;

        SC_Character.CancelAttack();

        characterToMove = target;

        tileManager.RemoveAllFilters();
        
		uiManager.HideWeapons();
		uiManager.villagePanel.SetActive (false);
		uiManager.cancelMovementButton.SetActive (false);
		uiManager.cancelAttackButton.SetActive (false);

		SC_Tile tileTarget = tileManager.GetTileAt (target.gameObject);

		CalcRange(tileTarget, target);

        foreach(SC_Tile tile in new List<SC_Tile>(closedList) { tileTarget })
            if(tile.canSetOn)
                tile.ChangeDisplay(TDisplay.Movement);

    }

    void CalcRange(SC_Tile aStartingTile, SC_Character target) {

        openList.Clear();
        closedList.Clear();

		movementPoints[aStartingTile] = target.movement;

        bool berserk = false;
        if (target.IsHero())
            berserk = (((SC_Hero)target).berserk);

		ExpandTile(aStartingTile, berserk);

        while (openList.Count > 0) {
			
            openList.Sort((a, b) => movementPoints[a].CompareTo(movementPoints[b]));

            SC_Tile tile = openList[openList.Count - 1];

            openList.RemoveAt(openList.Count - 1);

			ExpandTile(tile, berserk);

        }

    }
    
    void ExpandTile(SC_Tile aTile, bool berserk) {
        
        int parentPoints = movementPoints[aTile];

		closedList.Add(aTile);
        
        foreach (SC_Tile tile in tileManager.GetNeighbors(aTile)) {
            
            if (closedList.Contains(tile) || openList.Contains(tile)) continue;

			int points = parentPoints - ((berserk && (tile.movementCost != 10000)) ? 1 : tile.movementCost);

            if (points >= 0) {

				openList.Add(tile);

				movementPoints[tile] = points;

			}
            
        }
        
    }
    #endregion

    public static SC_GameManager GetInstance() {
		
        return instance;

    }		
		
	public void NextTurn() {

		player.CmdNextTurn ();

	}

	public void NextTurnFunction() {    

	    turn++;

        tileManager.RemoveAllFilters();

        foreach (SC_Character character in FindObjectsOfType<SC_Character>()) {

			SC_Tile under = tileManager.GetTileAt (character.gameObject);

			if (character.IsHero ()) {
				
				SC_Hero hero = ((SC_Hero)character);

				hero.Regen ();

				if (!CoalitionTurn ()) {

					if (hero.powerUsed)	hero.powerBacklash++;
					if (hero.powerBacklash >= 2) hero.DestroyCharacter ();  

				} else {

					hero.berserkTurn = false;

				}

			}

			character.UnTired ();
            
			bool turn = character.coalition == CoalitionTurn ();

			under.movementCost = turn ? under.baseCost : 5000;
            
			character.SetCanMove (turn);

			if (tileManager.GetAt<SC_Construction> (under))
				under.attackable = !turn;

        }

        SC_Character.attackingCharacter = null;

        foreach(SC_Construction construction in FindObjectsOfType<SC_Construction>()) {
			
			if (construction.GetType ().Equals (typeof(SC_Wall)) || construction.GetType ().Equals (typeof(SC_Bastion)))
				tileManager.GetTileAt (construction.gameObject).attackable = CoalitionTurn ();

		}

		foreach (SC_Convoy convoy in FindObjectsOfType<SC_Convoy>())
			convoy.MoveConvoy ();

		if (!CoalitionTurn ()) {
			
			SC_Qin.ChangeEnergy (50 * SC_Village.number);

			Bastion = true;

            if(player.IsQin())
                DisplayConstructableTiles ();

		}

		uiManager.NextTurn (CoalitionTurn (), turn);
        
    }

    public bool CoalitionTurn() {

        return ((turn % 3) != 0);

    }

	public void DisplayConstructableTiles() {		

        List<SC_Tile> constructableTiles = new List<SC_Tile>();

        if (!Bastion) {

            uiManager.StartQinAction("construct");

            foreach (SC_Construction construction in FindObjectsOfType<SC_Construction>()) {

				if (construction.name.Contains ("Bastion") || construction.name.Contains ("Wall"))
					constructableTiles.AddRange (tileManager.GetNeighbors(tileManager.GetTileAt (construction.gameObject)));

			}

		    List<SC_Tile> constructableTilesTemp = new List<SC_Tile> (constructableTiles);

		    foreach (SC_Tile tile in constructableTilesTemp) {

				if (tileManager.GetAt<SC_Construction> (tile) != null) {
					
					constructableTiles.Remove (tile);

				} else if (tileManager.GetAt<SC_Character> (tile) != null) {
						
					if (tileManager.GetAt<SC_Character> (tile).IsHero ())
						constructableTiles.Remove (tile);

				}

            }

        } else {
			
			foreach (SC_Tile tile in tileManager.tiles)
                if (tile.constructable) constructableTiles.Add(tile);
            
        }        

        foreach(SC_Tile tile in constructableTiles)
            tile.GetComponent<SC_Tile>().ChangeDisplay(TDisplay.Construct);

	}

	public void ConstructAt(SC_Tile tile) {

		player.CmdConstructAt ((int)tile.transform.position.x, (int)tile.transform.position.y);

	}

	public void ConstructAt(int x, int y) {

        SC_Tile tile = tileManager.GetTileAt (x, y);

		if (tileManager.GetAt<SC_Character> (tile) != null) {

            SC_Qin.ChangeEnergy(SC_Qin.Qin.sacrificeValue);
			tileManager.GetAt<SC_Character> (tile).DestroyCharacter();

        }

		if (tileManager.GetAt<SC_Construction> (tile) != null)
			tileManager.GetAt<SC_Construction> (tile).DestroyConstruction();

        if(isServer) {

            GameObject go = Instantiate(Bastion ? bastionPrefab : wallPrefab, GameObject.Find(Bastion ? "Bastions" : "Walls").transform);
            go.transform.SetPos(tile.transform);

            NetworkServer.Spawn(go);

        }

        if(!Bastion)
            SC_Qin.ChangeEnergy(-SC_Qin.Qin.wallCost);

        if(player.IsQin()) {

            tileManager.RemoveAllFilters();

            if(Bastion) {

                foreach(SC_Character character in FindObjectsOfType<SC_Character>())
                    character.SetCanMove(!character.coalition);

                uiManager.construct.gameObject.SetActive(true);
                uiManager.qinPower.gameObject.SetActive(true);
                uiManager.sacrifice.gameObject.SetActive(true);
                uiManager.endTurn.SetActive(true);

            }

        }

        Bastion = false;

    }

    public void FinishConstruction() {

        if(player.IsQin()) {

            tileManager.RemoveAllFilters();

            if(Bastion) {

                foreach(SC_Character character in FindObjectsOfType<SC_Character>())
                    character.SetCanMove(!character.coalition);

                uiManager.construct.gameObject.SetActive(true);
                uiManager.qinPower.gameObject.SetActive(true);
                uiManager.sacrifice.gameObject.SetActive(true);
                uiManager.endTurn.SetActive(true);

            } else {

                DisplayConstructableTiles();

            }

        }

        Bastion = false;

    }

	public void UpdateNeighborWallGraph(SC_Tile center) {

		foreach (SC_Tile tile in tileManager.GetNeighbors(center)) {

			if((tileManager.GetAt<SC_Bastion> (tile) != null) || (tileManager.GetAt<SC_Wall> (tile) != null))
				UpdateWallGraph (tileManager.GetAt<SC_Construction>(tile).gameObject);

		}

	}

	public void UpdateWallGraph(GameObject go) {

        SC_Construction construction = go.GetComponent<SC_Construction>();

        SC_Tile under = tileManager.GetTileAt(go);

        bool left = false;
		bool right = false;
		bool top = false;
		int count = 0;

		foreach (SC_Tile tile in tileManager.GetNeighbors(under)) {

			if((tileManager.GetAt<SC_Bastion> (tile) != null) || (tileManager.GetAt<SC_Wall> (tile) != null)) {

				if (tile.transform.position.x < under.transform.position.x)
					left = true;
				else if (tile.transform.position.x > under.transform.position.x)
					right = true;
				else if (tile.transform.position.y > under.transform.position.y)
					top = true;						

				count++;

			}

		}

		string rotation = "";

        if (count == 1)
			rotation = right ? "Right" : left ? "Left" : top ? "Top" : "Bottom";
		else if (count == 2)
			rotation = right ? (left ? "RightLeft" : top ? "RightTop" : "RightBottom") : left ? (top ? "LeftTop" : "LeftBottom") : "TopBottom";
		else if (count == 3)
			rotation = !right ? "Left" : (!left ? "Right" : (!top ? "Bottom" : "Top"));

		if (!rotation.Equals ("")) rotation = "_" + rotation;

		construction.GetComponentInChildren<SpriteRenderer>().sprite = Resources.Load<Sprite>("Sprites/" + (construction.GetType().Equals(typeof(SC_Bastion)) ? "Bastion/" : "Wall/") + count.ToString () + rotation);

	}

	public void DisplaySacrifices() {

        uiManager.StartQinAction("sacrifice");

        foreach(SC_Soldier soldier in FindObjectsOfType<SC_Soldier>())
            tileManager.GetTileAt(soldier.gameObject).ChangeDisplay(TDisplay.Sacrifice);

	}

	public void DisplayResurrectionTiles() {

		if ((lastHeroDead != null) && (SC_Qin.Energy > SC_Qin.Qin.powerCost)) {

            uiManager.StartQinAction("qinPower");

            characterToMove = null;
            
			foreach (SC_Tile tile in tileManager.tiles)
                if(tile.IsEmpty())
                    tile.ChangeDisplay(TDisplay.Resurrection);                  

		}

	}

    #region Combat
	public void Attack() {

		uiManager.HideWeapons();
		uiManager.cancelMovementButton.SetActive (false);
		uiManager.cancelAttackButton.SetActive (false);

		SC_Character attacker = SC_Character.attackingCharacter;

        attacker.Tire();

        SC_Tile target = attacker.attackTarget;
		SC_Construction targetConstruction = tileManager.GetAt<SC_Construction> (target);

		if (tileManager.GetAt<SC_Character>(target) != null) {

			SC_Character attacked = tileManager.GetAt<SC_Character> (target);
            bool counterAttack = (rangedAttack && attacked.GetActiveWeapon().ranged) || (!rangedAttack && !attacked.GetActiveWeapon().IsBow());

            bool killed = attacked.Hit (CalcDamages (attacker, attacked, false), false);
			SetCritDodge (attacker, attacked);

			if (attacker.IsHero () && killed)
				IncreaseRelationships ((SC_Hero)attacker);

			if(counterAttack) {

                killed = attacker.Hit(CalcDamages(attacked, attacker, true), false);
                SetCritDodge(attacked, attacker);

                if (attacked.IsHero() && killed)
					IncreaseRelationships ((SC_Hero)attacked);

			}

		} else if (targetConstruction != null) {

			targetConstruction.health -= attacker.GetActiveWeapon ().weaponOrQi ? attacker.strength : attacker.qi;

			targetConstruction.lifebar.UpdateGraph (targetConstruction.health, targetConstruction.maxHealth);

			uiManager.UpdateBuildingHealth(targetConstruction.gameObject);

			if (targetConstruction.health <= 0)
				targetConstruction.DestroyConstruction ();

		} else if (target.Qin ()) {

			SC_Qin.ChangeEnergy (-(attacker.GetActiveWeapon ().weaponOrQi ? attacker.strength : attacker.qi));

		}     
        
        if (attacker.IsHero())
            ((SC_Hero)attacker).berserkTurn = ((SC_Hero)attacker).berserk;

        SC_Character.attackingCharacter = null;

    }

	void SetCritDodge(SC_Character attacker, SC_Character attacked) {

		attacker.criticalHit = (attacker.criticalHit == 0) ? attacker.technique : (attacker.criticalHit - 1);
		attacked.dodgeHit = (attacked.dodgeHit == 0) ? attacked.speed : (attacked.dodgeHit - 1);

	}
		
	public int CalcDamages(SC_Character attacker, SC_Character attacked, bool counter) {  

        bool heroAttacker = attacker.IsHero();
        bool heroAttacked = attacked.IsHero();

        int damages = attacker.GetActiveWeapon ().weaponOrQi ? attacker.strength : attacker.qi;

		damages = Mathf.CeilToInt (damages * attacker.GetActiveWeapon ().ShiFuMi (attacked.GetActiveWeapon()));

		if (heroAttacker)
			damages += Mathf.CeilToInt (damages * RelationBoost ((SC_Hero)attacker));

        if (heroAttacker && heroAttacked && !attacked.coalition)
            damages = Mathf.CeilToInt(damages * RelationMalus((SC_Hero)attacker, (SC_Hero)attacked));

		if (attacker.criticalHit == 0) damages *= 3;

		if (heroAttacker)
			damages = ((SC_Hero)attacker).berserk ? damages * 2 : damages;

		if (attacked.dodgeHit == 0) damages *= 0;

		int boostedArmor = attacked.armor;
		int boostedResistance = attacked.resistance;

		if (heroAttacked) {

            float relationBoost = RelationBoost((SC_Hero)attacked);
            boostedArmor += Mathf.CeilToInt (boostedArmor * relationBoost);
			boostedResistance += Mathf.CeilToInt (boostedResistance * relationBoost);

		}			

		damages -= (attacker.GetActiveWeapon().weaponOrQi) ? boostedArmor : boostedResistance;

		if (counter) damages = Mathf.FloorToInt ((float)(damages / 2));

        return (damages >= 0) ? damages : 0;

	}

    float RelationMalus(SC_Hero target, SC_Hero opponent) {

        int value;
        target.relationships.TryGetValue(opponent.characterName, out value);

        return 1 - RelationValue(value);

    }

	float RelationBoost(SC_Hero target) {

		float boost = 0;

		foreach (SC_Hero hero in HeroesInRange(target)) {

			int value;
			target.relationships.TryGetValue (hero.characterName, out value);

            boost += RelationValue(value);

        }

		return boost;

	}

    float RelationValue(int value) {

        return (value >= 1000) ? 0.25f : (value >= 750) ? 0.15f : (value >= 500) ? 0.1f : (value >= 350) ? 0.05f : 0;

    }

	public SC_Hero CheckHeroSaved(SC_Hero toSave, bool alreadySaved) {

		SC_Hero saver = null;

		if (!alreadySaved) {

			foreach (SC_Hero hero in FindObjectsOfType<SC_Hero>()) {

				if (hero.coalition) {

					int value = 0;
					toSave.relationships.TryGetValue (hero.characterName, out value);

					int currentValue = -1;
					if (saver != null)
						toSave.relationships.TryGetValue (saver.characterName, out currentValue);

					if ((value >= 200) && (value > currentValue))
						saver = hero;

				}

			}

            SC_Tile nearestTile = NearestTile(toSave);

            if ((saver != null) && (nearestTile != null)) {

				SC_Tile leavingTile = tileManager.GetTileAt (saver.gameObject);

                leavingTile.movementCost = leavingTile.baseCost;
                leavingTile.canSetOn = true;
				if (tileManager.GetAt<SC_Construction>(leavingTile) == null)
                    leavingTile.attackable = true;
                leavingTile.constructable = !leavingTile.IsPalace();

				saver.transform.SetPos(NearestTile (toSave).transform);

				SC_Tile newTile = tileManager.GetTileAt (saver.gameObject);

                newTile.movementCost = 5000;
                newTile.canSetOn = false;
				newTile.attackable = (saver.coalition != CoalitionTurn());
				newTile.constructable = false;

            } else {

 				saver = null;

            }

		}

		return saver;

	}    

    public SC_Tile NearestTile(SC_Character target) {

		SC_Tile t = null;

		foreach(SC_Tile tile in tileManager.GetNeighbors(tileManager.GetTileAt(target.gameObject)))
			if(tile.IsEmpty()) t = tile;

		return t;

	}

	List<SC_Hero> HeroesInRange(SC_Hero target) {
                        
        List<SC_Tile> range = new List<SC_Tile>();

        int x = (int)target.transform.position.x;
        int y = (int)target.transform.position.y;

        for (int i = (x - 2); i <= (x + 2); i++) {

			for (int j = (y - 2); j <= (y + 2); j++) {

				if ((i >= 0) && (i < tileManager.xSize) && (j >= 0) && (j < tileManager.ySize)) {
					
					bool validTile = true;

					if ( ( (i == (x - 2)) || (i == (x + 2)) ) && (j != y))	validTile = false;
					if ( ( (j == (y - 2)) || (j == (y + 2)) ) && (i != x))	validTile = false;
					if ( ( (i == (x - 1)) || (i == (x + 1)) ) && ( (j < (y - 1)) || (j > (y + 1)) ) ) validTile = false;
					if ( ( (j == (y - 1)) || (j == (y + 1)) ) && ( (i < (x - 1)) || (i > (x + 1)) ) ) validTile = false;

					if (validTile) range.Add (tileManager.tiles [i, j]);

				}

			}

		}

		List<SC_Hero> heroesInRange = new List<SC_Hero>();

		foreach(SC_Tile tile in range) {

			SC_Character character = tileManager.GetAt<SC_Character> (tile);

            if (character != null) {
				
				if (character.IsHero () && character.coalition) {
					
					if (!character.characterName.Equals (target.characterName) && !heroesInRange.Contains(((SC_Hero)character)))
						heroesInRange.Add ((SC_Hero)character);

				}

			}
					
		}

        return heroesInRange;

	}

	public void PreviewFight(bool activeWeapon) {

		SC_Character attacker = SC_Character.attackingCharacter;

		if (attacker.IsHero ())	((SC_Hero)attacker).SetWeapon (activeWeapon);

		uiManager.PreviewFight (attacker, rangedAttack);

		if (attacker.IsHero ())	((SC_Hero)attacker).SetWeapon (activeWeapon);

	}

	void IncreaseRelationships(SC_Hero killer) {

		List<SC_Hero> heroesInRange = HeroesInRange (killer);

		foreach (SC_Hero hero in heroesInRange) {

			killer.relationships[hero.characterName] += Mathf.CeilToInt ((float)(100 / heroesInRange.Count));
			hero.relationships[killer.characterName] += Mathf.CeilToInt ((float)(100 / heroesInRange.Count));

		}

	}

    public void SetAttackWeapon(bool usedActiveWeapon) {

        ((SC_Hero)SC_Character.attackingCharacter).SetWeapon(usedActiveWeapon);
		Attack ();

    }
    #endregion

    #region Actions
    public void ActionVillage(bool destroy) {

		if (characterToMove.IsHero())
			((SC_Hero)characterToMove).ActionVillage (destroy);

	}

	public void SpawnConvoy(Vector3 pos) {

		if (pos.x >= 0) {

			if (tileManager.GetTileAt(pos).IsEmpty()) {
			
				SpawnConvoy (pos + new Vector3 (-1, 0, 0));

			} else {

				GameObject go = Instantiate (convoyPrefab, GameObject.Find ("Convoys").transform);
				go.transform.position = pos;

			}

		}

	}

    public void CreateSoldier() {

        if(SC_Qin.Energy > SC_Qin.Qin.soldierCost) {

			GameObject go = Instantiate(soldierPrefab, GameObject.Find("Soldiers").transform);
			go.transform.SetPos(currentWorkshop.transform);
            go.GetComponent<SC_Soldier>().Tire();

            SC_Qin.ChangeEnergy(-50);

			uiManager.workshopPanel.SetActive (false);

        }

    }

    public void DisplayWorkshopPanel() {

		if((!CoalitionTurn()) && !Bastion && (tileManager.GetAt<SC_Character>(currentWorkshop.gameObject) == null))
            uiManager.StartQinAction("workshop");

    }

    public void HideWorkshopPanel() {

		uiManager.workshopPanel.SetActive (false);

    }
    #endregion

    #region Cancel
    public void ResetMovement() {

        player.CmdResetMovement();

    }

    public void ResetMovementFunction() {

        tileManager.RemoveAllFilters();

        SC_Tile leavingTile = tileManager.GetTileAt (characterToMove.gameObject);

        leavingTile.movementCost = leavingTile.baseCost;
        leavingTile.canSetOn = true;
        leavingTile.attackable = tileManager.GetAt<SC_Construction>(leavingTile) == null;
		leavingTile.constructable = !leavingTile.IsPalace ();

		characterToMove.transform.SetPos (characterToMove.lastPos.transform);

		characterToMove.lastPos.movementCost = 5000;
		characterToMove.lastPos.canSetOn = false;
		if (tileManager.GetAt<SC_Construction> (characterToMove.lastPos) == null)
			characterToMove.lastPos.attackable = (characterToMove.coalition != GetInstance ().CoalitionTurn ());
		characterToMove.lastPos.constructable = !characterToMove.IsHero();

		characterToMove.SetCanMove (true);

        CheckMovements(characterToMove);

        characterToMove.UnTired ();

		uiManager.cancelMovementButton.SetActive (false);

    }

	public void ResetAttackChoice() {

		uiManager.HideWeapons();

		SC_Character.attackingCharacter.CheckAttack();

		uiManager.cancelMovementButton.SetActive (!cantCancelMovement);

		uiManager.cancelAttackButton.SetActive (false);

	}
    #endregion

    public void UseHeroPower() {

		SC_Hero hero = GameObject.Find (GameObject.Find ("PowerHero").GetComponentInChildren<Text> ().name).GetComponent<SC_Hero>();
		hero.powerUsed = true;

		GameObject.Find ("PowerHero").SetActive (false);

        print("Implement Power");

	}	

    public List<SC_Tile> GetClosedList() {

        return closedList;

    }

}
