using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class SC_GameManager : NetworkBehaviour {

	//Map
    //public int SizeMapX, SizeMapY;

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
	Dictionary<SC_Tile, float> movementPoints = new Dictionary<SC_Tile, float>();

	[SyncVar]
    int turn;

	//Other
	bool bastion;
	[HideInInspector]
	public SC_Hero lastHeroDead;
    [HideInInspector]
    public bool rangedAttack;
    [HideInInspector]
	public SC_Workshop currentWorkshop;
	[HideInInspector]
	public bool cantCancelMovement;
	List<SC_Tile> cursedTiles;

	[SerializeField]
	public SC_Player player { get { return Player; } set { Player = value; } }
	SC_Player Player;

	SC_UI_Manager uiManager;

	SC_Tile_Manager tileManager;

	SC_Character characterToMove;

	#region Setup
    void Start() {

		turn = 1;

		if (isServer) {

			GenerateMap ();
			SetupTileManager ();

		}

		bastion = true;

		cursedTiles = new List<SC_Tile> ();

		if (instance == null)
			instance = this;

		if(GameObject.FindGameObjectWithTag ("Player"))
			player = GameObject.FindGameObjectWithTag ("Player").GetComponent<SC_Player> ();

		if (player)
			player.SetGameManager (this);

		/*if (player != null) {

			uiManager = GetComponent<SC_UI_Manager> ();
			uiManager.SetupUI (player);

		} else {

			print ("Player is null");

		}*/

		uiManager = GetComponent<SC_UI_Manager> ();
		uiManager.SetupUI (FindObjectOfType<SC_Network_Manager>().IsQinHost() == isServer);

		if (isServer)
			RpcFinishLoading ();

    }

	[ClientRpc]
	void RpcFinishLoading() {

		uiManager.loadingPanel.SetActive(false);

	}

	void GenerateMap() {

		//tiles = /*(baseMapPrefab == null) ? new SC_Tile[SizeMapX, SizeMapY] :*/ new SC_Tile[baseMapPrefab.GetComponent<SC_MapPrefab>().xSize, baseMapPrefab.GetComponent<SC_MapPrefab>().ySize];

		/*if (baseMapPrefab == null) {

			for (int x = 0; x < SizeMapX; x++) { 

				for (int y = 0; y < SizeMapY; y++) {

					GameObject tilePrefab;
					int RandomTileMaker = Mathf.FloorToInt (UnityEngine.Random.Range (0, 10));

					tilePrefab = ((x > 25) && (y < 11) && (y > 3)) ? palacePrefab : ((RandomTileMaker <= 6) ? PlainPrefab : (RandomTileMaker <= 8) ? ForestPrefab : MountainPrefab);

					GameObject go = Instantiate (tilePrefab, new Vector3 (x, y, 0), tilePrefab.transform.rotation, GameObject.Find ("Tiles").transform);

					NetworkServer.Spawn (go);

				}    

			}

		} else {*/

			foreach (Transform child in baseMapPrefab.transform) {

				SC_EditorTile eTile = child.GetComponent<SC_EditorTile> ();

				GameObject tilePrefab = (eTile.tileType == tileType.Plain) ? plainPrefab : (eTile.tileType == tileType.Forest) ? forestPrefab : (eTile.tileType == tileType.Mountain) ? mountainPrefab : palacePrefab;

				GameObject go = Instantiate (tilePrefab, new Vector3(eTile.transform.position.x, eTile.transform.position.y, 0), eTile.transform.rotation,  GameObject.Find ("Tiles").transform);

				NetworkServer.Spawn (go);

			}

		//}

	}

	void SetupTileManager() {

		GameObject tm = Instantiate (tileManagerPrefab);
		SC_Tile_Manager stm = tm.GetComponent<SC_Tile_Manager> ();

		/*if (baseMapPrefab == null) {

			stm.xSize = SizeMapX;
			stm.ySize = SizeMapY;

		} else {*/

			stm.xSize = baseMapPrefab.GetComponent<SC_MapPrefab>().xSize;
			stm.ySize = baseMapPrefab.GetComponent<SC_MapPrefab>().ySize;

		FindObjectOfType<SC_Camera> ().Setup (stm.xSize, stm.ySize);

		//}

		NetworkServer.Spawn (tm);

	}

	public void FinishSetup() {

		tileManager = GameObject.FindObjectOfType<SC_Tile_Manager> ();

		if (isServer) {

			GenerateBuildings ();
			GenerateCharacters ();

		}

	}

	void GenerateBuildings() {

		/*if (baseMapPrefab == null) {

			foreach (Vector2 pos in SC_Workshop.spawnPositions) {

				GameObject go = Instantiate (workshopPrefab, GameObject.Find ("Workshops").transform);
				go.transform.SetPos (pos);
				NetworkServer.Spawn (go);

			}

			foreach (Vector2 pos in SC_Village.spawnPositions) {

				GameObject go = Instantiate (villagePrefab, GameObject.Find ("Villages").transform);
				go.transform.SetPos (pos);
				NetworkServer.Spawn (go);

			}

			Instantiate (bastionPrefab, GameObject.Find ("Bastions").transform);

		} else {*/

			foreach (Transform child in baseMapPrefab.transform) {

				SC_EditorTile eTile = child.GetComponent<SC_EditorTile> ();

				if (eTile.construction != constructionType.None) {

					GameObject constructionPrefab = (eTile.construction == constructionType.Village) ? eTile.villagePrefab : (eTile.construction == constructionType.Workshop) ? eTile.workshopPrefab : (eTile.construction == constructionType.Bastion) ? eTile.bastionPrefab : null;

					GameObject go2 = Instantiate (constructionPrefab);

					go2.transform.SetPos (eTile.transform);

					go2.transform.parent = (eTile.construction == constructionType.Village) ? GameObject.Find ("Villages").transform : (eTile.construction == constructionType.Workshop) ? GameObject.Find ("Workshops").transform : GameObject.Find("Bastions").transform;

					NetworkServer.Spawn (go2);

				}

			//}

		}

		foreach (SC_Construction construction in FindObjectsOfType<SC_Construction>()) {

			if (construction.GetType ().Equals (typeof(SC_Bastion)) || construction.GetType ().Equals (typeof(SC_Wall)))
				UpdateWallGraph (construction, tileManager.GetTileAt (construction.gameObject));

			NetworkServer.Spawn (construction.gameObject);

		}

	}

    void GenerateCharacters() {

		/*if (baseMapPrefab == null) {
		
			foreach (GameObject prefab in heroPrefabs) {
				
				GameObject hero = Instantiate (prefab, GameObject.Find ("Heroes").transform);
				NetworkServer.Spawn (hero);

			}

			GameObject qin = Instantiate (qinPrefab);
			NetworkServer.Spawn (qin);

			foreach (Vector2 pos in SC_Soldier.GetSpawnPositions()) {

				GameObject go = Instantiate (soldierPrefab, GameObject.Find ("Soldiers").transform);
				go.transform.SetPos (pos);
				NetworkServer.Spawn (go);

			}

		} else {*/

			foreach (Transform child in baseMapPrefab.transform) {

				SC_EditorTile eTile = child.GetComponent<SC_EditorTile> ();

				if (eTile.spawnSoldier || eTile.qin || (eTile.heroPrefab != null)) {

					GameObject go2 = Instantiate ((eTile.spawnSoldier) ? soldierPrefab : (eTile.qin) ? qinPrefab : eTile.heroPrefab);

					go2.transform.SetPos (eTile.transform);

					go2.transform.parent = (eTile.spawnSoldier) ? GameObject.Find ("Soldiers").transform : (eTile.qin) ? null : GameObject.Find ("Heroes").transform;

					NetworkServer.Spawn (go2);

				}

			}

		//}

    }
    #endregion

	#region Display Movements
    public void CheckMovements(SC_Character target) {
        
        cantCancelMovement = false;

        foreach (SC_Character character in FindObjectsOfType<SC_Character>()) {

			if (character.attacking) {
				
                character.Tire ();

                if (character.isHero())					
                    ((SC_Hero)character).berserkTurn = ((SC_Hero)character).berserk;
				
            }

			character.attacking = false;
                                                
        }
        
		characterToMove = target;

		foreach (SC_Tile tile in tileManager.tiles)
			player.CmdRemoveFilters (tile.gameObject);
			//tile.RemoveFilter();
        
		uiManager.HideWeapons();
		uiManager.villagePanel.SetActive (false);
		uiManager.cancelMovementButton.SetActive (false);
		uiManager.cancelAttackButton.SetActive (false);

		SC_Tile tileTarget = tileManager.GetTileAt (target.gameObject);

		CalcRange(tileTarget, target);
        
		int[] xArray = new int[closedList.Count + 1];
		int[] yArray = new int[closedList.Count + 1];

		int i = 0;

		foreach (SC_Tile tile in closedList) {

			xArray [i] = (int)tile.transform.position.x;
			yArray [i] = (int)tile.transform.position.y;

			i++;

			//player.CmdDisplayMovement (tile.gameObject);
			//tile.DisplayMovement(tile.canSetOn);

		}			
			
		xArray [i] = (int)tileTarget.transform.position.x;
		yArray [i] = (int)tileTarget.transform.position.y;

		player.CmdDisplayMovement (xArray, yArray);

		//player.CmdDisplayMovement (tileTarget.gameObject);
		//tileTarget.DisplayMovement (true);

    }

    void CalcRange(SC_Tile aStartingTile, SC_Character target) {

        openList.Clear();
        closedList.Clear();
		movementPoints[aStartingTile] = target.movement;
        bool berserk = false;
        if (target.isHero())
            berserk = (((SC_Hero)target).berserk);
		ExpandTile(aStartingTile, berserk);

        while (openList.Count > 0) {
			
            openList.Sort((a, b) => movementPoints[a].CompareTo(movementPoints[b]));
            var tile = openList[openList.Count - 1];
            openList.RemoveAt(openList.Count - 1);
			ExpandTile(tile, berserk);

        }

    }
    
    void ExpandTile(SC_Tile aTile, bool berserk) {
        
        float parentPoints = movementPoints[aTile];
		closedList.Add(aTile);
        
        foreach (SC_Tile tile in GetNeighbors(aTile)) {
            
            if (closedList.Contains(tile) || openList.Contains(tile)) continue;

			float points = parentPoints - ((berserk && (tile.movementCost != 10000)) ? 1 : tile.movementCost);

            if (points >= 0) {

				openList.Add(tile);
				movementPoints[tile] = points;

			}
            
        }
        
    }
    #endregion

    public List<SC_Tile> GetNeighbors(SC_Tile tileParam) {
		
		List<SC_Tile> neighbors = new List<SC_Tile>();
        int x = (int)tileParam.transform.position.x;
        int y = (int)tileParam.transform.position.y;

        if ((x - 1) >= 0)
			neighbors.Add(tileManager.tiles[x - 1, y]);

		if ((x + 1) < tileManager.xSize)
			neighbors.Add(tileManager.tiles[x + 1, y]);

        if ((y - 1) >= 0)
			neighbors.Add(tileManager.tiles[x, y - 1]);

		if ((y + 1) < tileManager.ySize)
			neighbors.Add(tileManager.tiles[x, y + 1]);

        return neighbors;

    }

    public static SC_GameManager GetInstance() {
		
        return instance;

    }		
		
    public void NextTurn() {    

	    turn++;

		foreach (SC_Tile tile in tileManager.tiles)
            tile.RemoveFilters();

        foreach (SC_Character character in FindObjectsOfType<SC_Character>()) {

			SC_Tile under = tileManager.GetTileAt (character.gameObject);

			if (character.isHero ())
				((SC_Hero)character).Regen ();
			else if (((SC_Soldier)character).curse1)
				cursedTiles.AddRange (GetNeighbors (under));

			character.attacking = false;
			character.UnTired ();
            
            under.movementCost = (character.coalition != CoalitionTurn()) ? 5000 : under.baseCost;
            
			if (tileManager.GetAt<SC_Construction>(under))              
                under.attackable = (character.coalition != CoalitionTurn ());

        }

		Curse1 ();

		foreach(SC_Construction construction in FindObjectsOfType<SC_Construction>()) {
			
			if (construction.GetType ().Equals (typeof(SC_Wall)) || construction.GetType ().Equals (typeof(SC_Bastion)))
				tileManager.GetTileAt (construction.gameObject).attackable = CoalitionTurn ();

		}
			
		uiManager.HideWeapons();
		uiManager.villagePanel.SetActive (false);
		uiManager.usePower.SetActive (false);
		uiManager.cancelMovementButton.SetActive (false);
		uiManager.cancelAttackButton.SetActive (false);

		foreach (SC_Convoy convoy in FindObjectsOfType<SC_Convoy>()) {

			convoy.MoveConvoy ();
			SC_Tile under = tileManager.GetTileAt (convoy.gameObject);
            under.canSetOn = !CoalitionTurn();

        }

		if (!CoalitionTurn ()) {
			
			SC_Qin.ChangeEnergy (50*SC_Village.number);

			foreach (SC_Hero hero in FindObjectsOfType<SC_Hero>()) {

                hero.SetCanMove (!hero.coalition);
				if (hero.powerUsed) hero.powerBacklash++;
                if (hero.powerBacklash >= 2) hero.DestroyCharacter();    

            }

			bastion = true;
			DisplayConstructableTiles ();

		} else {

			uiManager.HideButton ("construct");
			uiManager.HideButton ("qinPower");
			uiManager.HideButton ("sacrifice");

			foreach (SC_Character character in FindObjectsOfType<SC_Character>()) {
				
				character.SetCanMove (character.coalition);
				if (character.isHero ()) ((SC_Hero)character).berserkTurn = false;
					
			}

		}

		uiManager.SetTurnText (turn);
        
    }

    public bool CoalitionTurn() {

        return ((turn % 3) != 0);

    }

	public void DisplayConstructableTiles() {

		foreach (SC_Tile tile in tileManager.tiles)
            tile.RemoveFilters();

        SC_Character.ResetAttacker();

        List<SC_Tile> constructableTiles = new List<SC_Tile>();

        if (!bastion) {

			uiManager.ToggleButton ("construct");
			uiManager.ToggleButton ("sacrifice");
			uiManager.workshopPanel.SetActive (false);
			uiManager.cancelMovementButton.SetActive (false);

            foreach (SC_Construction construction in FindObjectsOfType<SC_Construction>()) {

				if (construction.name.Contains ("Bastion") || construction.name.Contains ("Wall"))
					constructableTiles.AddRange (GetNeighbors(tileManager.GetTileAt (construction.gameObject)));

			}

		    List<SC_Tile> constructableTilesTemp = new List<SC_Tile> (constructableTiles);

		    foreach (SC_Tile tile in constructableTilesTemp) {

				if (tileManager.GetAt<SC_Construction> (tile) != null) {
					
					constructableTiles.Remove (tile);

				} else if (tileManager.GetAt<SC_Character> (tile) != null) {
						
					if (tileManager.GetAt<SC_Character> (tile).isHero ())
						constructableTiles.Remove (tile);

				}

            }

        } else {
			
			foreach (SC_Tile tile in tileManager.tiles)
                if (tile.constructable) constructableTiles.Add(tile);
            
        }
        
		foreach (SC_Tile tile in constructableTiles)
			player.CmdDisplayConstructable (tile.gameObject);

	}

	public void ConstructAt(SC_Tile tile) {

		if (tileManager.GetAt<SC_Character> (tile) != null) {

            SC_Qin.ChangeEnergy(25);
			tileManager.GetAt<SC_Character> (tile).DestroyCharacter();

        }

		if (tileManager.GetAt<SC_Construction> (tile) != null)
			tileManager.GetAt<SC_Construction> (tile).DestroyConstruction();

		Transform parentGo = GameObject.Find (bastion ? "Bastions" : "Walls").transform;
		GameObject go = bastion ? Instantiate (bastionPrefab, parentGo) : Instantiate (wallPrefab, parentGo);
		go.transform.SetPos (tile.transform);

		UpdateWallGraph (go.GetComponent<SC_Construction>(), tile);

		UpdateNeighborWallGraph (tile);

		if (!bastion)
            SC_Qin.ChangeEnergy (-50);

    }

	public void UpdateNeighborWallGraph(SC_Tile center) {

		foreach (SC_Tile tile in GetNeighbors(center)) {

			if((tileManager.GetAt<SC_Bastion> (tile) != null) || (tileManager.GetAt<SC_Wall> (tile) != null))
				UpdateWallGraph (tileManager.GetAt<SC_Construction> (tile), tile);

		}

	}

	public void UpdateWallGraph(SC_Construction construction, SC_Tile under) {

		bool left = false;
		bool right = false;
		bool top = false;
		int count = 0;
		bool isBastion = construction.GetType ().Equals (typeof(SC_Bastion));

		foreach (SC_Tile tile in GetNeighbors(under)) {

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
			rotation = !isBastion ? ((right || left) ? "Horizontal" : "Vertical" ) : (right ? "Right" : left ? "Left" : top ? "Top" : "Bottom");
		else if (count == 2)
			rotation = right ? (left ? "RightLeft" : top ? "RightTop" : "RightBottom") : left ? (top ? "LeftTop" : "LeftBottom") : "TopBottom";
		else if (count == 3)
			rotation = !right ? "Left" : (!left ? "Right" : (!top ? "Bottom" : "Top"));

		if (!rotation.Equals ("")) rotation = "_" + rotation;

		construction.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Sprites/" + (isBastion ? "Bastion/" : "Wall/") + count.ToString () + rotation);

	}


	public void StopConstruction() {

		foreach (SC_Tile tile in tileManager.tiles)
			tile.RemoveFilters();

		if (bastion) {

			foreach (SC_Character character in FindObjectsOfType<SC_Character>())
				character.SetCanMove (!character.coalition);

			uiManager.ShowButton ("construct");
			uiManager.ShowButton("qinPower");
			uiManager.ShowButton("sacrifice");

			bastion = false;

		} else {

			DisplayConstructableTiles ();

		}

	}

	public void EndConstruction() {

		foreach (SC_Tile tile in tileManager.tiles)
			tile.RemoveFilters();

		uiManager.ToggleButton ("construct");

	}

	public void DisplaySacrifices() {

		uiManager.ToggleButton ("construct");
		uiManager.ToggleButton("sacrifice");
		uiManager.workshopPanel.SetActive (false);

		uiManager.cancelMovementButton.SetActive (false);

		foreach (SC_Tile tile in tileManager.tiles)
			tile.RemoveFilters();

		SC_Character.ResetAttacker();

		foreach (SC_Soldier soldier in FindObjectsOfType<SC_Soldier>()) {

			tileManager.GetTileAt(soldier.gameObject).displaySacrifice = true;
			tileManager.GetTileAt(soldier.gameObject).SetFilter ("T_DisplaySacrifice");

		}

	}

	public void HideSacrifices() {

		uiManager.ToggleButton ("sacrifice");

		foreach (SC_Tile tile in tileManager.tiles)
			tile.RemoveFilters();

	}

	public void DisplayResurrectionTiles() {

		if ((lastHeroDead != null) && ((SC_Qin.GetEnergy() - 2000) > 0)) {
			
			foreach (SC_Character character in FindObjectsOfType<SC_Character>()) {

				characterToMove = null;

				if (character.attacking) {

					character.Tire ();

					if (character.isHero())					
						((SC_Hero)character).berserkTurn = ((SC_Hero)character).berserk;

				}

				character.attacking = false;

			}
            
			foreach (SC_Tile tile in tileManager.tiles) {

                tile.RemoveFilters();

                if (tile.attackable && tile.constructable) {

                    tile.displayResurrection = true;
                    tile.SetFilter("T_DisplayResurrection");

                }                    

            }

            foreach(SC_Wall wall in FindObjectsOfType<SC_Wall>())
				tileManager.GetTileAt(wall.gameObject).RemoveFilters();

			uiManager.ToggleButton ("powerQin");

		}

	}

	public void HideResurrectionTiles() {

		uiManager.ToggleButton ("powerQin");

		foreach (SC_Tile tile in tileManager.tiles)
			tile.RemoveFilters ();

	}

	public void CheckAttack(SC_Character attacker) {

		foreach (SC_Tile tile in tileManager.tiles)
            tile.RemoveFilters();

		uiManager.HideWeapons();

        List<SC_Tile> attackableTiles = new List<SC_Tile>();

		attackableTiles.AddRange (GetNeighbors (tileManager.GetTileAt (attacker.gameObject)));

		if (attacker.HasRange()) {

			foreach (SC_Tile tile in GetNeighbors(tileManager.GetTileAt(attacker.gameObject)))
				attackableTiles.AddRange(GetNeighbors(tile));

		}

		List<SC_Tile> attackableTilesTemp = new List<SC_Tile> (attackableTiles);

		foreach (SC_Tile tile in attackableTilesTemp)
			if (!tile.attackable) attackableTiles.Remove (tile);

		foreach (SC_Tile tile in attackableTiles)
			player.CmdDisplayAttack (tile.gameObject);

	}

	public void Attack() {

		uiManager.HideWeapons();
		uiManager.cancelMovementButton.SetActive (false);
		uiManager.cancelAttackButton.SetActive (false);

		SC_Character attacker = SC_Character.GetAttackingCharacter ();

        attacker.Tire();

        SC_Tile target = attacker.attackTarget;
		SC_Construction targetConstruction = tileManager.GetAt<SC_Construction> (target);

		if (tileManager.GetAt<SC_Character>(target) != null) {

			SC_Character attacked = tileManager.GetAt<SC_Character> (target);
            bool counterAttack = (rangedAttack && attacked.GetActiveWeapon().ranged) || (!rangedAttack && !attacked.GetActiveWeapon().IsBow());

            bool killed = attacked.Hit (CalcDamages (attacker, attacked, false), false);
			SetCritDodge (attacker, attacked);

			if (attacker.isHero () && killed)
				IncreaseRelationships ((SC_Hero)attacker);

			if(counterAttack) {

                killed = attacker.Hit(CalcDamages(attacked, attacker, true), false);
                SetCritDodge(attacked, attacker);

                if (attacked.isHero() && killed)
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

        attacker.attacking = false;
        
        if (attacker.isHero())
            ((SC_Hero)attacker).berserkTurn = ((SC_Hero)attacker).berserk;

    }

	void SetCritDodge(SC_Character attacker, SC_Character attacked) {

		attacker.criticalHit = (attacker.criticalHit == 0) ? attacker.technique : (attacker.criticalHit - 1);
		attacked.dodgeHit = (attacked.dodgeHit == 0) ? attacked.speed : (attacked.dodgeHit - 1);

	}
		
	public int CalcDamages(SC_Character attacker, SC_Character attacked, bool counter) {  

        bool heroAttacker = attacker.isHero();
        bool heroAttacked = attacked.isHero();

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
                leavingTile.constructable = !leavingTile.isPalace();

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

		foreach(SC_Tile tile in GetNeighbors(tileManager.GetTileAt(target.gameObject)))
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
				
				if (character.isHero () && character.coalition) {
					
					if (!character.characterName.Equals (target.characterName) && !heroesInRange.Contains(((SC_Hero)character)))
						heroesInRange.Add ((SC_Hero)character);

				}

			}
					
		}

        return heroesInRange;

	}

	public void PreviewFight(bool activeWeapon) {

		SC_Character attacker = SC_Character.GetAttackingCharacter ();

		if (attacker.isHero ())	((SC_Hero)attacker).SetWeapon (activeWeapon);

		uiManager.PreviewFight (attacker, rangedAttack);

		if (attacker.isHero ())	((SC_Hero)attacker).SetWeapon (activeWeapon);

	}

	void IncreaseRelationships(SC_Hero killer) {

		List<SC_Hero> heroesInRange = HeroesInRange (killer);

		foreach (SC_Hero hero in heroesInRange) {

			killer.relationships[hero.characterName] += Mathf.CeilToInt ((float)(100 / heroesInRange.Count));
			hero.relationships[killer.characterName] += Mathf.CeilToInt ((float)(100 / heroesInRange.Count));

		}

	}

    public void SetAttackWeapon(bool usedActiveWeapon) {

        ((SC_Hero)SC_Character.GetAttackingCharacter()).SetWeapon(usedActiveWeapon);
		Attack ();

    }

	public void ActionVillage(bool destroy) {

		if (characterToMove.isHero())
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

        if(((SC_Qin.GetEnergy() - 50) > 0)) {

			GameObject go = Instantiate(soldierPrefab, GameObject.Find("Soldiers").transform);
			go.transform.SetPos(currentWorkshop.transform);
            go.GetComponent<SC_Soldier>().Tire();

            SC_Qin.ChangeEnergy(-50);

			uiManager.workshopPanel.SetActive (false);

        }

    }

    public void DisplayWorkshopPanel() {

		if((!CoalitionTurn()) && !bastion && (tileManager.GetAt<SC_Character>(currentWorkshop.gameObject) == null)) {

			uiManager.ToggleButton ("construct");
			uiManager.ToggleButton("sacrifice");

			foreach (SC_Tile tile in tileManager.tiles)
                tile.RemoveFilters();

            SC_Character.ResetAttacker();

			uiManager.workshopPanel.SetActive (true);

        }

    }

    public void HideWorkshopPanel() {

		uiManager.workshopPanel.SetActive (false);

    }

	public void CancelMovement() {

		SC_Character.ResetAttacker ();

		foreach (SC_Tile tile in tileManager.tiles)
			player.CmdRemoveFilters (tile.gameObject);
			//tile.RemoveFilters ();

		SC_Tile leavingTile = tileManager.GetTileAt (characterToMove.gameObject);

        leavingTile.movementCost = leavingTile.baseCost;
        leavingTile.canSetOn = true;
		if (tileManager.GetAt<SC_Construction> (leavingTile) == null)
            leavingTile.attackable = true;
		leavingTile.constructable = !leavingTile.isPalace ();

		player.CmdMove (characterToMove.gameObject, characterToMove.lastPos.transform.position);
		//characterToMove.transform.SetPos (characterToMove.lastPos.transform);

		characterToMove.lastPos.movementCost = 5000;
		characterToMove.lastPos.canSetOn = false;
		if (tileManager.GetAt<SC_Construction> (characterToMove.lastPos) == null)
			characterToMove.lastPos.attackable = (characterToMove.coalition != SC_GameManager.GetInstance ().CoalitionTurn ());
		characterToMove.lastPos.constructable = !characterToMove.isHero();

		characterToMove.SetCanMove (true);
		CheckMovements (characterToMove);

		characterToMove.UnTired ();

		uiManager.cancelMovementButton.SetActive (false);

	}

	public void CancelAttack() {

		uiManager.HideWeapons();

		CheckAttack (SC_Character.GetAttackingCharacter ());

		if(!cantCancelMovement) uiManager.cancelMovementButton.SetActive (true);

		uiManager.cancelAttackButton.SetActive (false);

	}

	public void UseHeroPower() {

		SC_Hero hero = GameObject.Find (GameObject.Find ("PowerHero").GetComponentInChildren<Text> ().name).GetComponent<SC_Hero>();
		hero.powerUsed = true;

		GameObject.Find ("PowerHero").SetActive (false);

		foreach (SC_Soldier soldier in FindObjectsOfType<SC_Soldier>())
			soldier.curse1 = true;

	}

	public void ToggleHealth() {

		foreach (SC_Lifebar lifebar in FindObjectsOfType<SC_Lifebar>())
			lifebar.Toggle();

		uiManager.ToggleButton("health");

	}

	void Curse1() {
        
        List<SC_Tile> tempCursedTiles = new List<SC_Tile>();

        foreach (SC_Tile tile in cursedTiles)
            if (tile.constructable && !tile.attackable) tempCursedTiles.Add(tile);

        foreach (SC_Wall wall in FindObjectsOfType<SC_Wall>())
			tempCursedTiles.Remove (tileManager.GetTileAt(wall.gameObject));

		foreach (SC_Tile tile in tempCursedTiles)
			tileManager.GetAt<SC_Soldier>(tile).Hit(2, false);

		cursedTiles = new List<SC_Tile> ();

	}

	public bool IsBastion() {

		return bastion;

	}

	public bool IsNeighbor(SC_Tile pos, SC_Tile target) {

		bool neighbor = false;

		foreach (SC_Tile tile in GetNeighbors(pos))
			if (tile.transform.position == target.transform.position) neighbor = true;

		return neighbor;

	}

	public List<SC_Tile> GetClosedList() {

		return closedList;

	}

	public SC_Character GetCharacterToMove() {

		return characterToMove;

	}

	public void SetCharacterToMove(SC_Character chara) {

		characterToMove = chara;

	}

}
