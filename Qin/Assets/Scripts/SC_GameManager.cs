using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class SC_GameManager : NetworkBehaviour {

	//Map
    public int SizeMapX, SizeMapY;
	public SC_Tile[,] tiles;

    //Prefabs
	public GameObject baseMapPrefab;
    public GameObject PlainPrefab, ForestPrefab, MountainPrefab, palacePrefab;
    public GameObject qinPrefab, soldierPrefab, convoyPrefab;
	public List<GameObject> heroPrefabs;
	public GameObject bastionPrefab, wallPrefab, workshopPrefab, villagePrefab;
    
	//Instance
    static SC_GameManager instance;

	//Variables used to determine the movements possible
	List<SC_Tile> openList = new List<SC_Tile>();
	List<SC_Tile> closedList = new List<SC_Tile>();
	Dictionary<SC_Tile, float> movementPoints = new Dictionary<SC_Tile, float>();
	int movementLeft, TotalMovementCost;

	//Turns variables
    int turn = 1;
    Text turns;

	//UI
	public Button constructWallButton, endConstructionButton;
    public Button powerQinButton, cancelPowerQinButton;
    public Button sacrificeUnitButton, cancelSacrificeButton;
    public Button showHealthButton, hideHealthButton;
    public GameObject previewFightPanel, workshopPanel;

	//Other
	bool bastion;
	[HideInInspector]
	public SC_Hero lastHeroDead;
    [HideInInspector]
    public bool rangedAttack;
    [HideInInspector]
    public Vector3 currentWorkshopPos;
	[HideInInspector]
	public bool cantCancelMovement;
	List<SC_Tile> cursedTiles;

	SC_Player player;

    void Awake() {

        if (instance == null) instance = this;

		turns = GameObject.Find("Turns").GetComponent<Text>();

		GenerateMap();
		GenerateCharacters();
		GenerateBuildings ();

        constructWallButton.gameObject.SetActive (false);
		endConstructionButton.gameObject.SetActive (false);
		powerQinButton.gameObject.SetActive (false);
		cancelPowerQinButton.gameObject.SetActive (false);
		sacrificeUnitButton.gameObject.SetActive (false);
		cancelSacrificeButton.gameObject.SetActive (false);
        hideHealthButton.gameObject.SetActive(false);
        previewFightPanel.SetActive (false);
        workshopPanel.SetActive(false);

        bastion = true;

		cursedTiles = new List<SC_Tile> ();

    }
		
    #region Generation
    void GenerateMap() {

		tiles = (baseMapPrefab == null) ? new SC_Tile[SizeMapX, SizeMapY] : new SC_Tile[baseMapPrefab.GetComponent<SC_MapPrefab>().xSize, baseMapPrefab.GetComponent<SC_MapPrefab>().ySize];
		 
		if (baseMapPrefab == null) {

			for (int x = 0; x < SizeMapX; x++) { 

				for (int y = 0; y < SizeMapY; y++) {

					int RandomTileMaker = Mathf.FloorToInt (Random.Range (0, 10));

					GameObject tilePrefab = (RandomTileMaker <= 6) ? PlainPrefab : (RandomTileMaker <= 8) ? ForestPrefab : MountainPrefab;

					if ((x > 25) && (y < 11) && (y > 3))
						tilePrefab = palacePrefab;

					GameObject go = Instantiate (tilePrefab, tilePrefab.transform.position + new Vector3 (x, y, 0), tilePrefab.transform.rotation, GameObject.Find ("Tiles").transform);

					Quaternion rot = Quaternion.identity;
					rot.eulerAngles = new Vector3 (0, 0, Mathf.FloorToInt (Random.Range (0, 4) * 90));
					if (go.name.Contains ("Mountain") || go.name.Contains ("Forest")) go.transform.rotation = rot;

					tiles [x, y] = go.GetComponent<SC_Tile> ();

				}    
	                
			}

		} else {

			foreach (Transform child in baseMapPrefab.transform) {

				SC_EditorTile eTile = child.GetComponent<SC_EditorTile> ();

				GameObject tilePrefab = (eTile.tileType == tileType.Plain) ? PlainPrefab : (eTile.tileType == tileType.Forest) ? ForestPrefab : (eTile.tileType == tileType.Mountain) ? MountainPrefab : palacePrefab;

				GameObject go = Instantiate (tilePrefab, eTile.transform.position, eTile.transform.rotation,  GameObject.Find ("Tiles").transform);

				Quaternion rot = Quaternion.identity;
				rot.eulerAngles = new Vector3 (0, 0, Mathf.FloorToInt (Random.Range (0, 4) * 90));
				if (go.name.Contains ("Mountain") || go.name.Contains ("Forest")) go.transform.rotation = rot;

				int x = (int)go.transform.position.x;
				int y = (int)go.transform.position.y;

				tiles [x, y] = go.GetComponent<SC_Tile> ();

				if (eTile.spawnSoldier || eTile.qin || (eTile.heroPrefab != null) || (eTile.construction != constructionType.None)) {

					GameObject constructionPrefab = (eTile.construction == constructionType.Village) ? eTile.villagePrefab : (eTile.construction == constructionType.Workshop) ? eTile.workshopPrefab : (eTile.construction == constructionType.Bastion) ? eTile.bastionPrefab : null;

					GameObject go2 = Instantiate ((eTile.spawnSoldier) ? soldierPrefab : (eTile.qin) ? qinPrefab : (eTile.heroPrefab != null) ? eTile.heroPrefab : constructionPrefab);

					go2.transform.SetPos (eTile.transform);

					go2.transform.parent = (eTile.spawnSoldier) ? GameObject.Find ("Soldiers").transform : (eTile.qin) ? null : (eTile.heroPrefab != null) ? GameObject.Find ("Heroes").transform : (eTile.construction == constructionType.Village) ? GameObject.Find ("Villages").transform : (eTile.construction == constructionType.Workshop) ? GameObject.Find ("Workshops").transform : GameObject.Find("Bastions").transform;

				}

			}

			foreach (SC_Character character in FindObjectsOfType<SC_Character>())
				character.SetCanMove (character.coalition);

            foreach (SC_Construction construction in FindObjectsOfType<SC_Construction>())
                if (construction.name.Contains("Bastion")) UpdateWallGraph(construction, GetTileAt((int)construction.transform.position.x, (int)construction.transform.position.y));

		}

    }

    void GenerateCharacters() {

		if (baseMapPrefab == null) {

			foreach (GameObject prefab in heroPrefabs)				
				Instantiate (prefab, GameObject.Find ("Heroes").transform);

			Instantiate (qinPrefab);

			foreach (Vector2 pos in SC_Soldier.GetSpawnPositions()) {

				GameObject go = Instantiate (soldierPrefab, GameObject.Find ("Soldiers").transform);
				go.transform.SetPos (pos);

            }

			foreach (SC_Character character in FindObjectsOfType<SC_Character>())
				character.SetCanMove (character.coalition);

        }		

    }

	void GenerateBuildings() {

		foreach (Vector2 pos in SC_Workshop.spawnPositions) {

			GameObject go = Instantiate (workshopPrefab, GameObject.Find ("Workshops").transform);
			go.transform.SetPos (pos);

		}

		foreach (Vector2 pos in SC_Village.spawnPositions) {

			GameObject go = Instantiate (villagePrefab, GameObject.Find ("Villages").transform);
			go.transform.SetPos (pos);

		}

		GameObject b = Instantiate(bastionPrefab, GameObject.Find("Bastions").transform);
		UpdateWallGraph(b.GetComponent<SC_Construction>(), GetTileAt((int)b.transform.position.x, (int)b.transform.position.y));

	}
    #endregion

	#region Display Movements
    public void CheckMovements(SC_Character target) {
        
        cantCancelMovement = false;

        foreach (SC_Character character in FindObjectsOfType<SC_Character>()) {

            character.SetReadyToMove(false);

			if (character.attacking) {
				
                character.Tire ();

                if (character.isHero())					
                    ((SC_Hero)character).berserkTurn = ((SC_Hero)character).berserk;
				
            }

			character.attacking = false;
                                                
        }
        
        target.SetReadyToMove (true);

        foreach (SC_Tile tile in tiles)
            tile.RemoveFilter();
        
        SC_Hero.HideWeapons ();
		SC_Hero.villagePanel.SetActive (false);
		SC_Character.HideCancelMovement ();
		SC_Hero.HideCancelAttack (); 

        CalcRange(tiles[(int)target.transform.position.x, (int)target.transform.position.y], target);
        
        foreach (SC_Tile tile in closedList)
            tile.DisplayMovement(tile.canSetOn);

        GetTileAt ((int)target.transform.position.x, (int)target.transform.position.y).displayMovement = true;
		GetTileAt ((int)target.transform.position.x, (int)target.transform.position.y).SetFilter("T_DisplayMovement");

    }

    void CalcRange(SC_Tile aStartingTile, SC_Character target) {

        openList.Clear();
        closedList.Clear();
		movementPoints[aStartingTile] = target.Movement;
        bool berserk = false;
        if (target.isHero())
            berserk = (((SC_Hero)target).berserk);
		ExpandTile(aStartingTile, target.coalition, berserk);

        while (openList.Count > 0) {
			
            openList.Sort((a, b) => movementPoints[a].CompareTo(movementPoints[b]));
            var tile = openList[openList.Count - 1];
            openList.RemoveAt(openList.Count - 1);
			ExpandTile(tile, target.coalition, berserk);

        }

    }
    
    void ExpandTile(SC_Tile aTile, bool targetCoalition, bool berserk) {
        
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
            neighbors.Add(tiles[x - 1, y]);

        if ((x + 1) < SizeMapX)
            neighbors.Add(tiles[x + 1, y]);

        if ((y - 1) >= 0)
            neighbors.Add(tiles[x, y - 1]);

        if ((y + 1) < SizeMapY)
            neighbors.Add(tiles[x, y + 1]);

        return neighbors;

    }

    public static SC_GameManager GetInstance() {
		
        return instance;

    }		

    public void NextTurn() {
        
		player = GameObject.FindGameObjectWithTag ("Player").GetComponent<SC_Player> ();

		if(player.Turn(CoalitionTurn())) {

	        turn++;

	        foreach (SC_Tile tile in tiles)
	            tile.RemoveFilter();

	        foreach (SC_Character character in FindObjectsOfType<SC_Character>()) {

				SC_Tile under = GetTileAt((int)character.transform.position.x, (int)character.transform.position.y);

				if (character.isHero ())
					((SC_Hero)character).Regen ();
				else if (((SC_Soldier)character).curse1)
					cursedTiles.AddRange (GetNeighbors (under));

				character.attacking = false;
				character.UnTired ();
	            
	            under.movementCost = (character.coalition != CoalitionTurn()) ? 5000 : under.baseCost;
	            
	            if (GetConstructionAt (under) == null)               
	                under.attackable = (character.coalition != CoalitionTurn ());

	        }

			Curse1 ();

			foreach(SC_Construction construction in FindObjectsOfType<SC_Construction>()) {
				
				if (construction.GetType ().Equals (typeof(SC_Wall)) || construction.GetType ().Equals (typeof(SC_Bastion)))
					GetTileAt((int)construction.transform.position.x, (int)construction.transform.position.y).attackable = CoalitionTurn ();

			}
				
	        SC_Hero.HideWeapons ();
			SC_Hero.villagePanel.SetActive (false);
			SC_Hero.HidePower ();
			SC_Character.HideCancelMovement ();
			SC_Hero.HideCancelAttack ();

			foreach (SC_Convoy convoy in FindObjectsOfType<SC_Convoy>()) {

				convoy.MoveConvoy ();
	            SC_Tile under = GetTileAt((int)convoy.transform.position.x, (int)convoy.transform.position.y);
	            under.canSetOn = !CoalitionTurn();

	        }

			if (!CoalitionTurn ()) {
				
				SC_Qin.IncreaseEnergy (50*SC_Village.number);

				foreach (SC_Hero hero in FindObjectsOfType<SC_Hero>()) {

	                hero.SetCanMove (!hero.coalition);
					if (hero.powerUsed) hero.powerBacklash++;
	                if (hero.powerBacklash >= 2) hero.DestroyCharacter();    

	            }

				bastion = true;
				DisplayConstructableTiles ();

			} else {

				constructWallButton.gameObject.SetActive (false);
				endConstructionButton.gameObject.SetActive (false);
				powerQinButton.gameObject.SetActive (false);
				cancelPowerQinButton.gameObject.SetActive (false);
				sacrificeUnitButton.gameObject.SetActive (false);
				cancelSacrificeButton.gameObject.SetActive (false);

				foreach (SC_Character character in FindObjectsOfType<SC_Character>()) {
					
					character.SetCanMove (character.coalition);
					if (character.isHero ()) ((SC_Hero)character).berserkTurn = false;
						
				}

			}

	        turns.text = (((turn - 1) % 3) == 0) ? "1st Turn - Coalition" : (((turn - 2) % 3) == 0) ? "2nd Turn - Coalition" : "Turn Qin";

		}
        
    }

    public bool CoalitionTurn() {

        return ((turn % 3) != 0);

    }
	
	public SC_Character GetCharacterAt(SC_Tile tile) {

		return GetCharacterAt ((int)tile.transform.position.x, (int)tile.transform.position.y);

	}


	public SC_Character GetCharacterAt(int x, int y) {

		SC_Character characterToReturn = null;

		foreach (SC_Character character in FindObjectsOfType<SC_Character>()) {
			
			if ((character.transform.position.x == x) && (character.transform.position.y == y))
				characterToReturn = character;

		}

		return characterToReturn;

	}

	public SC_Convoy GetConvoyAt(SC_Tile tile) {

		return GetConvoyAt ((int)tile.transform.position.x, (int)tile.transform.position.y);

	}


	public SC_Convoy GetConvoyAt(int x, int y) {
        
        SC_Convoy convoyToReturn = null;

		foreach (SC_Convoy convoy in FindObjectsOfType<SC_Convoy>()) {

			if ((convoy.transform.position.x == x) && (convoy.transform.position.y == y))
				convoyToReturn = convoy;

		}

		return convoyToReturn;

	}

	public SC_Construction GetConstructionAt(SC_Tile tile) {

		return GetConstructionAt ((int)tile.transform.position.x, (int)tile.transform.position.y);

	}

	public SC_Construction GetConstructionAt(int x, int y) {

		SC_Construction constructionToReturn = null;

		foreach (SC_Construction construction in FindObjectsOfType<SC_Construction>()) {

			if ((construction.transform.position.x == x) && (construction.transform.position.y == y))
				constructionToReturn = construction;

		}

		return constructionToReturn;

	}

    public SC_Tile GetTileAt(int x, int y) {

        return tiles[x, y];

    }

	public void DisplayConstructableTiles() {

		foreach (SC_Tile tile in tiles)
            tile.RemoveFilter();

        SC_Character.ResetAttacker();

        List<SC_Tile> constructableTiles = new List<SC_Tile>();

        if (!bastion) {

			constructWallButton.gameObject.SetActive (false);
			endConstructionButton.gameObject.SetActive (true);
			sacrificeUnitButton.gameObject.SetActive (true);
			cancelSacrificeButton.gameObject.SetActive (false);
            workshopPanel.SetActive(false);
            SC_Character.HideCancelMovement();

            foreach (SC_Construction construction in FindObjectsOfType<SC_Construction>()) {

				if(construction.name.Contains("Bastion") || construction.name.Contains("Wall"))
					constructableTiles.AddRange (GetNeighbors (GetTileAt ((int)construction.transform.position.x, (int)construction.transform.position.y)));

			}

		    List<SC_Tile> constructableTilesTemp = new List<SC_Tile> (constructableTiles);

		    foreach (SC_Tile tile in constructableTilesTemp) {

			    int x = (int)tile.transform.position.x;
			    int y = (int)tile.transform.position.y;

			    if (GetConstructionAt (x, y) != null)
				    constructableTiles.Remove (tile);
                else if (GetCharacterAt(x, y) != null)
                    if (GetCharacterAt(x, y).isHero()) constructableTiles.Remove(tile);

            }

        } else {
			
            foreach (SC_Tile tile in tiles)
                if (tile.constructable) constructableTiles.Add(tile);
            
        }
        
		foreach (SC_Tile tile in constructableTiles) {

			tile.SetCanConstruct (true);
			tile.SetFilter("T_CanConstruct");

		}

	}

	public void ConstructAt(Vector3 pos) {

		int x = (int)pos.x;
		int y = (int)pos.y;

        if (GetCharacterAt(x, y) != null) {

            SC_Qin.IncreaseEnergy(25);
            GetCharacterAt(x, y).DestroyCharacter();

        }

        if (GetConstructionAt(x, y) != null)
            GetConstructionAt(x, y).DestroyConstruction();

		Transform parentGo = GameObject.Find (bastion ? "Bastions" : "Walls").transform;
		GameObject go = bastion ? Instantiate (bastionPrefab, parentGo) : Instantiate (wallPrefab, parentGo);
		go.transform.SetPos (pos);

		UpdateWallGraph (go.GetComponent<SC_Construction>(), GetTileAt (x, y));

		UpdateNeighborWallGraph (GetTileAt (x, y));

		if (!bastion)
            SC_Qin.DecreaseEnergy (50);

    }

	public void UpdateNeighborWallGraph(SC_Tile center) {

		foreach (SC_Tile tile in GetNeighbors(center)) {

			if (GetConstructionAt (tile) != null) {

				if(GetConstructionAt (tile).GetType().Equals(typeof(SC_Wall)) || GetConstructionAt (tile).GetType().Equals(typeof(SC_Bastion)))
					UpdateWallGraph (GetConstructionAt (tile), tile);

			}

		}

	}

	public void UpdateWallGraph(SC_Construction construction, SC_Tile under) {

		bool left = false;
		bool right = false;
		bool top = false;
		int count = 0;
		bool isBastion = construction.GetType ().Equals (typeof(SC_Bastion));

		foreach (SC_Tile tile in GetNeighbors(under)) {

			if (GetConstructionAt (tile) != null) {

				if (GetConstructionAt (tile).GetType ().Equals (typeof(SC_Wall)) || GetConstructionAt (tile).GetType ().Equals (typeof(SC_Bastion))) {

					if (tile.transform.position.x < under.transform.position.x)
						left = true;
					else if (tile.transform.position.x > under.transform.position.x)
						right = true;
					else if (tile.transform.position.y > under.transform.position.y)
						top = true;						

					count++;

				}

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

		foreach (SC_Tile tile in tiles)
			tile.RemoveFilter();

		if (bastion) {

			foreach (SC_Character character in FindObjectsOfType<SC_Character>())
				character.SetCanMove (!character.coalition);

			constructWallButton.gameObject.SetActive (true);
			powerQinButton.gameObject.SetActive (true);
			sacrificeUnitButton.gameObject.SetActive (true);

			bastion = false;

		} else {

			DisplayConstructableTiles ();

		}

	}

	public void EndConstruction() {

		foreach (SC_Tile tile in tiles)
			tile.RemoveFilter();

		constructWallButton.gameObject.SetActive (true);
		endConstructionButton.gameObject.SetActive (false);

	}

	public void DisplaySacrifices() {

		constructWallButton.gameObject.SetActive (true);
		endConstructionButton.gameObject.SetActive (false);
		sacrificeUnitButton.gameObject.SetActive (false);
		cancelSacrificeButton.gameObject.SetActive (true);
        workshopPanel.SetActive(false);
        SC_Character.HideCancelMovement();

        foreach (SC_Tile tile in tiles)
			tile.RemoveFilter();

		SC_Character.ResetAttacker();

		foreach (SC_Soldier soldier in FindObjectsOfType<SC_Soldier>()) {
			
			GetTileAt ((int)soldier.transform.position.x, (int)soldier.transform.position.y).displaySacrifice = true;
			GetTileAt ((int)soldier.transform.position.x, (int)soldier.transform.position.y).SetFilter ("T_DisplaySacrifice");

		}

	}

	public void HideSacrifices() {

		sacrificeUnitButton.gameObject.SetActive (true);
		cancelSacrificeButton.gameObject.SetActive (false);

		foreach (SC_Tile tile in tiles)
			tile.RemoveFilter();

	}

	public void DisplayResurrectionTiles() {

		if ((lastHeroDead != null) && ((SC_Qin.GetEnergy() - 2000) > 0)) {
			
			foreach (SC_Character character in FindObjectsOfType<SC_Character>()) {

				character.SetReadyToMove(false);

				if (character.attacking) {

					character.Tire ();

					if (character.isHero())					
						((SC_Hero)character).berserkTurn = ((SC_Hero)character).berserk;

				}

				character.attacking = false;

			}
            
            foreach (SC_Tile tile in tiles) {

                tile.RemoveFilter();

                if (tile.attackable && tile.constructable) {

                    tile.displayResurrection = true;
                    tile.SetFilter("T_DisplayResurrection");

                }                    

            }

            foreach(SC_Wall wall in FindObjectsOfType<SC_Wall>())
                GetTileAt((int)wall.transform.position.x, (int)wall.transform.position.y).RemoveFilter();

			powerQinButton.gameObject.SetActive (false);
			cancelPowerQinButton.gameObject.SetActive (true);

		}

	}

	public void HideResurrectionTiles() {

		powerQinButton.gameObject.SetActive (true);
		cancelPowerQinButton.gameObject.SetActive (false);

		foreach (SC_Tile tile in tiles)
			tile.RemoveFilter ();

	}

	public void CheckAttack(SC_Character attacker) {

        foreach (SC_Tile tile in tiles)
            tile.RemoveFilter();

		SC_Hero.HideWeapons ();

        List<SC_Tile> attackableTiles = new List<SC_Tile>();

		attackableTiles.AddRange(GetNeighbors(GetTileAt((int)attacker.transform.position.x, (int)attacker.transform.position.y)));

		if (attacker.HasRange()) {

			foreach (SC_Tile tile in GetNeighbors(GetTileAt((int)attacker.transform.position.x, (int)attacker.transform.position.y)))
				attackableTiles.AddRange(GetNeighbors(tile));

		}

		List<SC_Tile> attackableTilesTemp = new List<SC_Tile> (attackableTiles);

		foreach (SC_Tile tile in attackableTilesTemp)
			if (!tile.attackable) attackableTiles.Remove (tile);

		foreach (SC_Tile tile in attackableTiles) {

			tile.SetDisplayAttack (true);
			tile.SetFilter("T_DisplayAttack");

		}

	}

	public void Attack() {

        SC_Hero.HideWeapons ();
		SC_Character.HideCancelMovement ();
		SC_Hero.HideCancelAttack ();

		SC_Character attacker = SC_Character.GetAttackingCharacter ();

        attacker.Tire();

        attacker.AnimAttack();

        SC_Tile target = attacker.attackTarget;
		SC_Construction targetConstruction = GetConstructionAt (target);

		if (GetCharacterAt (target) != null) {

			SC_Character attacked = GetCharacterAt (target);
            bool counterAttack = (rangedAttack && attacked.GetActiveWeapon().ranged) || (!rangedAttack && !attacked.GetActiveWeapon().IsBow());

            bool killed = attacked.Hit (calcDamages (attacker, attacked, false), false);
			SetCritDodge (attacker, attacked);

			if (attacker.isHero () && killed)
				IncreaseRelationships ((SC_Hero)attacker);

			if(counterAttack) {

                killed = attacker.Hit(calcDamages(attacked, attacker, true), false);
                SetCritDodge(attacked, attacker);

                attacked.AnimAttack();

                if (attacked.isHero() && killed)
					IncreaseRelationships ((SC_Hero)attacked);

			}

		} else if (targetConstruction != null) {

			targetConstruction.health -= attacker.GetActiveWeapon ().weaponOrQi ? attacker.strength : attacker.qi;

			targetConstruction.lifebar.UpdateGraph (targetConstruction.health, targetConstruction.maxHealth);

			if (targetConstruction.selfPanel) targetConstruction.ShowBuildingPanel ();

			if (targetConstruction.health <= 0)
				targetConstruction.DestroyConstruction ();

		} else if (target.Qin ()) {

			SC_Qin.DecreaseEnergy (attacker.GetActiveWeapon ().weaponOrQi ? attacker.strength : attacker.qi);

			if (SC_Qin.selfPanel) SC_Qin.ShowQinPanel ();

		}

        attacker.attacking = false;
        
        if (attacker.isHero())
            ((SC_Hero)attacker).berserkTurn = ((SC_Hero)attacker).berserk;

    }

	void SetCritDodge(SC_Character attacker, SC_Character attacked) {

		attacker.criticalHit = (attacker.criticalHit == 0) ? attacker.technique : (attacker.criticalHit - 1);
		attacked.dodgeHit = (attacked.dodgeHit == 0) ? attacked.speed : (attacked.dodgeHit - 1);

	}
		
	int calcDamages(SC_Character attacker, SC_Character attacked, bool counter) {  

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

                SC_Tile leavingTile = GetTileAt((int)saver.transform.position.x, (int)saver.transform.position.y);

                leavingTile.movementCost = leavingTile.baseCost;
                leavingTile.canSetOn = true;
                if (GetConstructionAt(leavingTile) == null)
                    leavingTile.attackable = true;
                leavingTile.constructable = !leavingTile.isPalace();

				saver.transform.SetPos(NearestTile (toSave).transform);

                SC_Tile newTile = GetTileAt((int)saver.transform.position.x, (int)saver.transform.position.y);

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

		foreach(SC_Tile tile in GetNeighbors(GetTileAt((int)target.transform.position.x, (int)target.transform.position.y)))
			if(tile.IsEmpty()) t = tile;

		return t;

	}

	List<SC_Hero> HeroesInRange(SC_Hero target) {
                        
        List<SC_Tile> range = new List<SC_Tile>();

        int x = (int)target.transform.position.x;
        int y = (int)target.transform.position.y;

        for (int i = (x - 2); i <= (x + 2); i++) {

			for (int j = (y - 2); j <= (y + 2); j++) {

				if ((i >= 0) && (i < SizeMapX) && (j >= 0) && (j < SizeMapY)) {
					
					bool validTile = true;

					if ( ( (i == (x - 2)) || (i == (x + 2)) ) && (j != y))	validTile = false;
					if ( ( (j == (y - 2)) || (j == (y + 2)) ) && (i != x))	validTile = false;
					if ( ( (i == (x - 1)) || (i == (x + 1)) ) && ( (j < (y - 1)) || (j > (y + 1)) ) ) validTile = false;
					if ( ( (j == (y - 1)) || (j == (y + 1)) ) && ( (i < (x - 1)) || (i > (x + 1)) ) ) validTile = false;

					if (validTile) range.Add (tiles [i, j]);

				}

			}

		}

		List<SC_Hero> heroesInRange = new List<SC_Hero>();

		foreach(SC_Tile tile in range) {

            SC_Character character = GetCharacterAt(tile);

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

		previewFightPanel.SetActive (true);

		SC_Functions.SetText ("AttackerName", attacker.characterName);

		SC_Functions.SetText ("AttackerWeapon", attacker.GetActiveWeapon ().weaponName);

		SC_Functions.SetText ("AttackerCrit", attacker.criticalHit.ToString ());

		SC_Functions.SetText ("AttackerDodge", attacker.dodgeHit.ToString ());

		int attackedDamages = 0;

        string attackerDamagesString = "";

        int attackerDamages = attacker.GetActiveWeapon().weaponOrQi ? attacker.strength : attacker.qi;

        string attackedDamagesString = "";

        string attackedName = "";

        string attackedHP = "";

        string attackedWeapon = "";

        string attackedCrit = "";

        string attackedDodge = "";

        if (GetCharacterAt (attacker.attackTarget) != null) {

			SC_Character attacked = GetCharacterAt (attacker.attackTarget);

            attackedName = attacked.characterName;

            attackedWeapon = attacked.GetActiveWeapon ().weaponName;
            
            attackerDamages = calcDamages (attacker, attacked, false);
			attackedDamages = calcDamages (attacked, attacker, true);
			if (!((rangedAttack && attacked.GetActiveWeapon ().ranged) || (!rangedAttack && !attacked.GetActiveWeapon ().IsBow ())))
				attackedDamages = 0;

            attackedHP = (attacked.health - attackerDamages).ToString ();

            attackerDamagesString = attackerDamages.ToString ();

            attackedDamagesString = attackedDamages.ToString ();

            attackedCrit = attacked.criticalHit.ToString ();

            attackedDodge = attacked.dodgeHit.ToString ();

		} else {

			int attackedType = (GetConstructionAt (attacker.attackTarget) != null) ? 0 : attacker.attackTarget.Qin () ? 1 : 2;

            attackedName = (attackedType == 0) ? GetConstructionAt(attacker.attackTarget).buildingName : (attackedType == 1) ? "Qin" : "";			
            
			int attackedHealth = (attackedType == 0) ? GetConstructionAt (attacker.attackTarget).health : (attackedType == 1) ? SC_Qin.GetEnergy () : 0;

			if (attackedType != 2) attackedHP = (attackedHealth - attackerDamages).ToString ();

		}

        SC_Functions.SetText("AttackerHP", (attacker.health - attackedDamages).ToString());

        SC_Functions.SetText("AttackedName", attackedName);

        SC_Functions.SetText("AttackedHP", attackedHP);

        SC_Functions.SetText("AttackerDamages", attackerDamagesString);
        SC_Functions.SetText("AttackedDamages", attackedDamagesString);

        SC_Functions.SetText("AttackedWeapon", attackedWeapon);

        SC_Functions.SetText("AttackedCrit", attackedCrit);
        SC_Functions.SetText("AttackedDodge", attackedDodge);

        if (attacker.isHero ())	((SC_Hero)attacker).SetWeapon (activeWeapon);

    }

	public void HidePreviewFight() {

		previewFightPanel.SetActive (false);

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

		SC_Character chararacter = SC_Character.GetCharacterToMove ();

		if (chararacter.isHero())
			((SC_Hero)chararacter).ActionVillage (destroy);

	}

	public void SpawnConvoy(Vector3 pos) {

		if (pos.x >= 0) {

			SC_Tile baseSpawn = GetTileAt ((int)pos.x, (int)pos.y);

			if ((GetCharacterAt (baseSpawn) != null) || (GetConstructionAt (baseSpawn) != null) || (GetConvoyAt (baseSpawn) != null)) {
			
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
            go.transform.SetPos(currentWorkshopPos);
            go.GetComponent<SC_Soldier>().Tire();

            SC_Qin.DecreaseEnergy(50);

            workshopPanel.SetActive(false);

        }

    }

    public void DisplayWorkshopPanel() {

        if((!CoalitionTurn()) && !bastion && (GetCharacterAt((int)currentWorkshopPos.x, (int)currentWorkshopPos.y) == null)) {

            constructWallButton.gameObject.SetActive(true);
            endConstructionButton.gameObject.SetActive(false);
            sacrificeUnitButton.gameObject.SetActive(true);
            cancelSacrificeButton.gameObject.SetActive(false);

            foreach (SC_Tile tile in tiles)
                tile.RemoveFilter();

            SC_Character.ResetAttacker();

            workshopPanel.SetActive(true);

        }

    }

    public void HideWorkshopPanel() {

        workshopPanel.SetActive(false);

    }

	public void CancelMovement() {

		SC_Character.ResetAttacker ();

		foreach (SC_Tile tile in tiles)
			tile.RemoveFilter ();

		SC_Character character = SC_Character.GetCharacterToMove ();

        SC_Tile leavingTile = GetTileAt((int)character.transform.position.x, (int)character.transform.position.y);

        leavingTile.movementCost = leavingTile.baseCost;
        leavingTile.canSetOn = true;
        if (GetConstructionAt(leavingTile) == null)
            leavingTile.attackable = true;
		leavingTile.constructable = !leavingTile.isPalace ();

        character.transform.position = character.lastPos;

		SC_Tile previousTile = GetTileAt((int)character.transform.position.x, (int)character.transform.position.y);

		previousTile.movementCost = 5000;
		previousTile.canSetOn = false;
		if (GetConstructionAt(previousTile) == null)
			previousTile.attackable = (character.coalition != SC_GameManager.GetInstance ().CoalitionTurn ());
		previousTile.constructable = !character.isHero();

		character.SetCanMove (true);
		character.SetReadyToMove (true);
		CheckMovements (character);

		character.UnTired ();

		SC_Character.HideCancelMovement ();

	}

	public void CancelAttack() {

		SC_Hero.HideWeapons ();

		CheckAttack (SC_Character.GetAttackingCharacter ());

		if(!cantCancelMovement) SC_Character.DisplayCancelMovement ();

		SC_Hero.HideCancelAttack ();

	}

	public void UseHeroPower() {

		SC_Hero hero = GameObject.Find (GameObject.Find ("PowerHero").GetComponentInChildren<Text> ().name).GetComponent<SC_Hero>();
		hero.powerUsed = true;

		GameObject.Find ("PowerHero").SetActive (false);

		foreach (SC_Soldier soldier in FindObjectsOfType<SC_Soldier>())
			soldier.curse1 = true;

	}

    public void ShowHealth() {

        foreach (SC_Lifebar lifebar in FindObjectsOfType<SC_Lifebar>())
            lifebar.Show();

        showHealthButton.gameObject.SetActive(false);
        hideHealthButton.gameObject.SetActive(true);

    }

    public void HideHealth() {

        foreach (SC_Lifebar lifebar in FindObjectsOfType<SC_Lifebar>())
            lifebar.Hide();

        showHealthButton.gameObject.SetActive(true);
        hideHealthButton.gameObject.SetActive(false);

    }

	void Curse1() {
        
        List<SC_Tile> tempCursedTiles = new List<SC_Tile>();

        foreach (SC_Tile tile in cursedTiles)
            if (tile.constructable && !tile.attackable) tempCursedTiles.Add(tile);

        foreach (SC_Wall wall in FindObjectsOfType<SC_Wall>())
            tempCursedTiles.Remove (GetTileAt ((int)wall.transform.position.x, (int)wall.transform.position.y));

		foreach (SC_Tile tile in tempCursedTiles)
			((SC_Soldier)GetCharacterAt (tile)).Hit (2, false);

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

}
