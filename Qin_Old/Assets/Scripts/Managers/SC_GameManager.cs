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
    public SC_Common_Heroes_Variables commonHeroesVariables;
	public List<GameObject> heroPrefabs;
	public GameObject bastionPrefab, wallPrefab, workshopPrefab, villagePrefab;
	public GameObject tileManagerPrefab;
    
	//Instance
    public static SC_GameManager Instance { get; set; }

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

		if (!Instance)
			Instance = this;

        uiManager = SC_UI_Manager.Instance;
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

		NetworkServer.Spawn (tm);

	}

	public void FinishSetup() {

        tileManager = SC_Tile_Manager.Instance;

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

			if (eTile.spawnSoldier || eTile.qin || eTile.heroPrefab) {

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
            if(tile.CanSetOn || (tile == tileTarget))
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
            
            if (closedList.Contains(tile) || openList.Contains(tile) || !tile.CanGoThrough) continue;

			int points = parentPoints - (berserk ? 1 : tile.cost);

            if (points >= 0) {

				openList.Add(tile);

				movementPoints[tile] = points;

			}
            
        }
        
    }
    #endregion	
		
	public void NextTurn() {

		player.CmdNextTurn ();

	}

	public void NextTurnFunction() {    

	    turn++;

        tileManager.RemoveAllFilters();

        foreach (SC_Character character in FindObjectsOfType<SC_Character>()) {

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
            
			character.SetCanMove (turn);

        }

        SC_Character.attackingCharacter = null;

		/*foreach (SC_Convoy convoy in FindObjectsOfType<SC_Convoy>())
			convoy.MoveConvoy ();*/

		if (!CoalitionTurn ()) {

            SC_Qin.ChangeEnergy(SC_Qin.Qin.regenPerVillage * SC_Village.number);

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

        } else {

            foreach (SC_Tile tile in tileManager.tiles)
                constructableTiles.Add(tile);
            
        }        

        foreach(SC_Tile tile in constructableTiles)
            if(tile.Constructable && (Bastion || !tile.Wall))
                tile.GetComponent<SC_Tile>().ChangeDisplay(TDisplay.Construct);

	}

	public void ConstructAt(SC_Tile tile) {

		player.CmdConstructAt ((int)tile.transform.position.x, (int)tile.transform.position.y);

	}

	public void ConstructAt(int x, int y) {

        SC_Tile tile = tileManager.GetTileAt (x, y);

		if (tile.Character) {

            SC_Qin.ChangeEnergy(SC_Qin.Qin.sacrificeValue);
            tile.Character.DestroyCharacter();

        }

        tile.Construction?.DestroyConstruction();

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

	public void UpdateNeighborWallGraph(SC_Tile center) {

		foreach (SC_Tile tile in tileManager.GetNeighbors(center))
            if(tile.Bastion)
                UpdateWallGraph(tile.Bastion.gameObject);

	}

	public void UpdateWallGraph(GameObject go) {

        SC_Construction construction = go.GetComponent<SC_Construction>();

        SC_Tile under = tileManager.GetTileAt(go);

        bool left = false;
		bool right = false;
		bool top = false;
		int count = 0;

		foreach (SC_Tile tile in tileManager.GetNeighbors(under)) {

			if(tile.Bastion) {

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

		if (lastHeroDead && (SC_Qin.Energy > SC_Qin.Qin.powerCost)) {

            uiManager.StartQinAction("qinPower");
            
			foreach (SC_Tile tile in tileManager.tiles)
                if(tile.Empty)
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

        if(!attacker.attackTarget.Empty) {

            SC_Character attacked = attacker.attackTarget.Character;
            SC_Construction targetConstruction = attacker.attackTarget.Construction;

            if(attacked) {

                bool counterAttack = (rangedAttack && attacked.GetActiveWeapon().ranged) || (!rangedAttack && !attacked.GetActiveWeapon().IsBow());

                bool killed = attacked.Hit(CalcDamages(attacker, attacked, false), false);
                SetCritDodge(attacker, attacked);

                if(attacker.IsHero() && killed)
                    IncreaseRelationships((SC_Hero)attacker);

                if(counterAttack) {

                    killed = attacker.Hit(CalcDamages(attacked, attacker, true), false);
                    SetCritDodge(attacked, attacker);

                    if(attacked.IsHero() && killed)
                        IncreaseRelationships((SC_Hero)attacked);

                }

                uiManager.TryRefreshInfos(attacked.gameObject, attacked.GetType());

            } else if(targetConstruction) {

                targetConstruction.health -= attacker.GetActiveWeapon().weaponOrQi ? attacker.strength : attacker.qi;

                targetConstruction.lifebar.UpdateGraph(targetConstruction.health, targetConstruction.maxHealth);

                if(targetConstruction.health <= 0)
                    targetConstruction.DestroyConstruction();
                else
                    uiManager.TryRefreshInfos(targetConstruction.gameObject, typeof(SC_Construction));

            } else if(attacker.attackTarget.Qin) {

                SC_Qin.ChangeEnergy(-(attacker.GetActiveWeapon().weaponOrQi ? attacker.strength : attacker.qi));

                uiManager.TryRefreshInfos(SC_Qin.Qin.gameObject, SC_Qin.Qin.GetType());

            }

            uiManager.TryRefreshInfos(attacker.gameObject, attacker.GetType());            

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

		if (heroAttacker && ((SC_Hero)attacker).berserk)
            damages = Mathf.CeilToInt(damages * commonHeroesVariables.berserkDamageMultiplier);

		if (attacked.dodgeHit == 0) damages = 0;

		int boostedArmor = attacked.armor;
		int boostedResistance = attacked.resistance;

		if (heroAttacked) {

            float relationBoost = RelationBoost((SC_Hero)attacked);
            boostedArmor += Mathf.CeilToInt (boostedArmor * relationBoost);
			boostedResistance += Mathf.CeilToInt (boostedResistance * relationBoost);

		}			

		damages -= (attacker.GetActiveWeapon().weaponOrQi) ? boostedArmor : boostedResistance;

		if (counter) damages = Mathf.CeilToInt (damages / 2);

        return Mathf.Max(0, damages);

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

        float v = 0;

        for (int i = 0; i < commonHeroesVariables.relationBoostValues.relations.Length; i++)
            if (value >= commonHeroesVariables.relationBoostValues.relations[i])
                v = commonHeroesVariables.relationBoostValues.values[i];

        return v;

    }

	public SC_Hero CheckHeroSaved(SC_Hero toSave, bool alreadySaved) {

		SC_Hero saver = null;

		if (!alreadySaved) {

			foreach (SC_Hero hero in FindObjectsOfType<SC_Hero>()) {

				if (hero.coalition) {

					int value = 0;
					toSave.relationships.TryGetValue (hero.characterName, out value);

					int currentValue = -1;
					if (saver)
						toSave.relationships.TryGetValue (saver.characterName, out currentValue);

					if ((value >= commonHeroesVariables.saveTriggerRelation) && (value > currentValue))
						saver = hero;

				}

			}

            SC_Tile nearestTile = NearestTile(toSave);

            if (saver && nearestTile) {

				tileManager.GetTileAt (saver.gameObject).Character = null;

				saver.transform.SetPos(NearestTile (toSave).transform);

				tileManager.GetTileAt (saver.gameObject).Character = saver;

            } else {

 				saver = null;

            }

		}

		return saver;

	}    

    public SC_Tile NearestTile(SC_Character target) {

		SC_Tile t = null;

		foreach(SC_Tile tile in tileManager.GetNeighbors(tileManager.GetTileAt(target.gameObject)))
			if(tile.Empty) t = tile;

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

            if (tile.Character) {
				
				if (tile.Character.IsHero () && tile.Character.coalition) {
					
					if (!tile.Character.characterName.Equals (target.characterName) && !heroesInRange.Contains(((SC_Hero)tile.Character)))
						heroesInRange.Add ((SC_Hero)tile.Character);

				}

			}
					
		}

        return heroesInRange;

	}

	public void PreviewFight(bool activeWeapon) {

		if (SC_Character.attackingCharacter.IsHero ())	((SC_Hero)SC_Character.attackingCharacter).SetWeapon (activeWeapon);

		uiManager.PreviewFight (SC_Character.attackingCharacter, rangedAttack);

		if (SC_Character.attackingCharacter.IsHero ())	((SC_Hero)SC_Character.attackingCharacter).SetWeapon (activeWeapon);

	}

	void IncreaseRelationships(SC_Hero killer) {

		List<SC_Hero> heroesInRange = HeroesInRange (killer);

		foreach (SC_Hero hero in heroesInRange) {

			killer.relationships[hero.characterName] += Mathf.CeilToInt (commonHeroesVariables.killRelationValue / heroesInRange.Count);
			hero.relationships[killer.characterName] += Mathf.CeilToInt (commonHeroesVariables.killRelationValue / heroesInRange.Count);

		}

	}

    public void SetAttackWeapon(bool usedActiveWeapon) {

        player.CmdHeroAttack(usedActiveWeapon);

    }
    #endregion

    #region Actions
    public void ActionVillage(bool destroy) {

		player.CmdActionVillage (destroy);

	}

    public void ActionVillageFunction (bool destroy) {

        if (destroy) {

            cantCancelMovement = true;
            tileManager.GetTileAt(gameObject).Construction.DestroyConstruction();

        } else {

            uiManager.cancelMovementButton.SetActive(true);

        }

        uiManager.villagePanel.SetActive(false);

        SC_Character.attackingCharacter.CheckAttack();

    }

    /*public void SpawnConvoy(Vector3 pos) {

		if (pos.x >= 0) {

			if (tileManager.GetTileAt(pos).IsEmpty()) {
			
				SpawnConvoy (pos + new Vector3 (-1, 0, 0));

			} else {

				GameObject go = Instantiate (convoyPrefab, GameObject.Find ("Convoys").transform);
				go.transform.position = pos;

			}

		}

	}*/

    public void CreateSoldier() {

        if(SC_Qin.Energy > SC_Qin.Qin.soldierCost) {

			GameObject go = Instantiate(soldierPrefab, GameObject.Find("Soldiers").transform);
			go.transform.SetPos(currentWorkshop.transform);
            go.GetComponent<SC_Soldier>().Tire();

            SC_Qin.ChangeEnergy(-SC_Qin.Qin.soldierCost);

			uiManager.workshopPanel.SetActive (false);

        }

    }

    public void DisplayWorkshopPanel() {

		if(!CoalitionTurn() && !Bastion && !tileManager.GetTileAt(currentWorkshop.gameObject).Character)
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

        tileManager.GetTileAt (characterToMove.gameObject).Character = null;

		characterToMove.transform.SetPos (characterToMove.LastPos.transform);

        characterToMove.LastPos.Character = characterToMove;

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
