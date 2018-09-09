using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using static SC_Enums;

public class SC_Game_Manager : NetworkBehaviour {

    //Prefabs
	public GameObject baseMapPrefab;
    public GameObject plainPrefab, forestPrefab, mountainPrefab, palacePrefab;
    public GameObject qinPrefab, soldierPrefab, convoyPrefab;
    public SC_Common_Heroes_Variables commonHeroesVariables;
	public List<GameObject> heroPrefabs;
	public GameObject bastionPrefab, wallPrefab, workshopPrefab, villagePrefab;
	public GameObject tileManagerPrefab;
    
	//Instance
    public static SC_Game_Manager Instance { get; set; }  

    int turn;

    public bool CoalitionTurn { get { return turn % 3 != 0; } }

    //Other
    public bool Bastion { get; set; }

	public SC_Hero LastHeroDead { get; set; }

    public bool RangedAttack { get; set; }

	public SC_Workshop CurrentWorkshop { get; set; }

	public bool CantCancelMovement { get; set; }

	public SC_Player Player { get; set; }

	SC_UI_Manager uiManager;

	SC_Tile_Manager tileManager;	

	#region Setup
    void Start() {        

        turn = 1;

		if(GameObject.FindGameObjectWithTag ("Player")) {
			
			Player = GameObject.FindGameObjectWithTag ("Player").GetComponent<SC_Player> ();

            Player.SetSide();

			Player.SetGameManager (this);

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
		
	public void NextTurn() {

		Player.CmdNextTurn ();

	}

	public void NextTurnFunction() {    

	    turn++;

        tileManager.RemoveAllFilters();

        foreach (SC_Character character in FindObjectsOfType<SC_Character>()) {

			if (character.IsHero ()) {
				
				SC_Hero hero = ((SC_Hero)character);

				hero.Regen ();

				if (!CoalitionTurn) {

					if (hero.powerUsed)	hero.powerBacklash++;
					if (hero.powerBacklash >= 2) hero.DestroyCharacter ();  

				} else {

					hero.berserkTurn = false;

				}

			}

			character.UnTired ();
            
			bool turn = character.coalition == CoalitionTurn;

            character.CanMove = turn;

        }

        SC_Character.attackingCharacter = null;

		/*foreach (SC_Convoy convoy in FindObjectsOfType<SC_Convoy>())
			convoy.MoveConvoy ();*/

		if (!CoalitionTurn) {

            SC_Qin.ChangeEnergy(SC_Qin.Qin.regenPerVillage * SC_Village.number);

			Bastion = true;

            if(Player.IsQin())
                DisplayConstructableTiles ();

		}

		uiManager.NextTurn (CoalitionTurn, turn);
        
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

		Player.CmdConstructAt ((int)tile.transform.position.x, (int)tile.transform.position.y);

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

        if(Player.IsQin()) {

            tileManager.RemoveAllFilters();

            if(Bastion) {

                foreach(SC_Character character in FindObjectsOfType<SC_Character>())
                    character.CanMove = !character.coalition;

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

		if (LastHeroDead && (SC_Qin.Energy > SC_Qin.Qin.powerCost)) {

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

        if(!attacker.AttackTarget.Empty) {

            SC_Character attacked = attacker.AttackTarget.Character;
            SC_Construction targetConstruction = attacker.AttackTarget.Construction;

            if(attacked) {

                bool counterAttack = (RangedAttack && attacked.GetActiveWeapon().ranged) || (!RangedAttack && !attacked.GetActiveWeapon().IsBow());

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

                //uiManager.TryRefreshInfos(attacked.gameObject, attacked.GetType());

            } else if(targetConstruction) {

                targetConstruction.health -= attacker.GetActiveWeapon().weaponOrQi ? attacker.strength : attacker.qi;

                targetConstruction.lifebar.UpdateGraph(targetConstruction.health, targetConstruction.maxHealth);

                if(targetConstruction.health <= 0)
                    targetConstruction.DestroyConstruction();
                else
                    uiManager.TryRefreshInfos(targetConstruction.gameObject, typeof(SC_Construction));

            } else if(attacker.AttackTarget.Qin) {

                SC_Qin.ChangeEnergy(-(attacker.GetActiveWeapon().weaponOrQi ? attacker.strength : attacker.qi));                

            }

            //uiManager.TryRefreshInfos(attacker.gameObject, attacker.GetType());            

        }
        
        if (attacker.IsHero())
            ((SC_Hero)attacker).berserkTurn = ((SC_Hero)attacker).berserk;

        SC_Character.attackingCharacter = null;

    }

	void SetCritDodge(SC_Character attacker, SC_Character attacked) {

		attacker.CriticalHit = (attacker.CriticalHit == 0) ? attacker.technique : (attacker.CriticalHit - 1);
		attacked.DodgeHit = (attacked.DodgeHit == 0) ? attacked.speed : (attacked.DodgeHit - 1);

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

		if (attacker.CriticalHit == 0) damages *= 3;

		if (heroAttacker && ((SC_Hero)attacker).berserk)
            damages = Mathf.CeilToInt(damages * commonHeroesVariables.berserkDamageMultiplier);

		if (attacked.DodgeHit == 0) damages = 0;

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

		uiManager.PreviewFight (SC_Character.attackingCharacter, RangedAttack);

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

        Player.CmdHeroAttack(usedActiveWeapon);

    }
    #endregion

    #region Actions
    public void ActionVillage(bool destroy) {

		Player.CmdActionVillage (destroy);

	}

    public void ActionVillageFunction (bool destroy) {

        if (destroy) {

            CantCancelMovement = true;
            tileManager.GetTileAt(SC_Character.attackingCharacter.gameObject).Construction.DestroyConstruction();

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
			go.transform.SetPos(CurrentWorkshop.transform);
            go.GetComponent<SC_Soldier>().Tire();

            SC_Qin.ChangeEnergy(-SC_Qin.Qin.soldierCost);

			uiManager.workshopPanel.SetActive (false);

        }

    }

    public void DisplayWorkshopPanel() {

		if(!CoalitionTurn && !Bastion && !tileManager.GetTileAt(CurrentWorkshop.gameObject).Character)
            uiManager.StartQinAction("workshop");

    }

    public void HideWorkshopPanel() {

		uiManager.workshopPanel.SetActive (false);

    }
    #endregion

    #region Cancel
    public void ResetMovement() {

        Player.CmdResetMovement();

    }

    public void ResetMovementFunction() {

        tileManager.RemoveAllFilters();

        tileManager.GetTileAt (SC_Character.characterToMove.gameObject).Character = null;

        SC_Character.characterToMove.transform.SetPos (SC_Character.characterToMove.LastPos.transform);

        SC_Character.characterToMove.LastPos.Character = SC_Character.characterToMove;

        SC_Character.characterToMove.CanMove = true;

        tileManager.CheckMovements(SC_Character.characterToMove);

        SC_Character.characterToMove.UnTired ();

		uiManager.cancelMovementButton.SetActive (false);

    }

	public void ResetAttackChoice() {

		uiManager.HideWeapons();

		SC_Character.attackingCharacter.CheckAttack();

		uiManager.cancelMovementButton.SetActive (!CantCancelMovement);

		uiManager.cancelAttackButton.SetActive (false);

	}
    #endregion

    public void UseHeroPower() {

		SC_Hero hero = GameObject.Find (GameObject.Find ("PowerHero").GetComponentInChildren<Text> ().name).GetComponent<SC_Hero>();
		hero.powerUsed = true;

		GameObject.Find ("PowerHero").SetActive (false);

        print("Implement Power");

	}	

}
