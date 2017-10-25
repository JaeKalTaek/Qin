﻿using System.Collections;
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
	public GameObject tileManagerPrefab;
    
	//Instance
    static SC_GameManager instance;

	//Variables used to determine the movements possible
	List<SC_Tile> openList = new List<SC_Tile>();
	List<SC_Tile> closedList = new List<SC_Tile>();
	Dictionary<SC_Tile, float> movementPoints = new Dictionary<SC_Tile, float>();

	[SyncVar]
    int turn;

	//UI
    /*public Text turns;
	public Button constructWallButton, endConstructionButton;
    public Button powerQinButton, cancelPowerQinButton;
    public Button sacrificeUnitButton, cancelSacrificeButton;
    public Button showHealthButton, hideHealthButton;
    public GameObject previewFightPanel, workshopPanel;*/
	public SC_UI_Manager uiManager;

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

	SC_Player player;

	SC_Tile_Manager tileManager;

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

		uiManager = GameObject.FindObjectOfType<SC_UI_Manager> ();

		FindObjectOfType<SC_Camera> ().Setup (this);

    }

	void GenerateMap() {

		tiles = (baseMapPrefab == null) ? new SC_Tile[SizeMapX, SizeMapY] : new SC_Tile[baseMapPrefab.GetComponent<SC_MapPrefab>().xSize, baseMapPrefab.GetComponent<SC_MapPrefab>().ySize];

		if (baseMapPrefab == null) {

			for (int x = 0; x < SizeMapX; x++) { 

				for (int y = 0; y < SizeMapY; y++) {

					GameObject tilePrefab;
					int RandomTileMaker = Mathf.FloorToInt (UnityEngine.Random.Range (0, 10));

					tilePrefab = ((x > 25) && (y < 11) && (y > 3)) ? palacePrefab : ((RandomTileMaker <= 6) ? PlainPrefab : (RandomTileMaker <= 8) ? ForestPrefab : MountainPrefab);

					GameObject go = Instantiate (tilePrefab, new Vector3 (x, y, 0), tilePrefab.transform.rotation, GameObject.Find ("Tiles").transform);

					NetworkServer.Spawn (go);

				}    

			}

		} else {

			foreach (Transform child in baseMapPrefab.transform) {

				SC_EditorTile eTile = child.GetComponent<SC_EditorTile> ();

				GameObject tilePrefab = (eTile.tileType == tileType.Plain) ? PlainPrefab : (eTile.tileType == tileType.Forest) ? ForestPrefab : (eTile.tileType == tileType.Mountain) ? MountainPrefab : palacePrefab;

				GameObject go = Instantiate (tilePrefab, eTile.transform.position, eTile.transform.rotation,  GameObject.Find ("Tiles").transform);

				NetworkServer.Spawn (go);

			}

		}

	}

	void SetupTileManager() {

		GameObject tm = Instantiate (tileManagerPrefab);
		SC_Tile_Manager stm = tm.GetComponent<SC_Tile_Manager> ();

		if (baseMapPrefab == null) {

			stm.xSize = SizeMapX;
			stm.ySize = SizeMapY;

		} else {

			stm.xSize = baseMapPrefab.GetComponent<SC_MapPrefab>().xSize;
			stm.ySize = baseMapPrefab.GetComponent<SC_MapPrefab>().ySize;

		}

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

		if (baseMapPrefab == null) {

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

			/*GameObject b = */Instantiate (bastionPrefab, GameObject.Find ("Bastions").transform);

		} else {

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

		}

		foreach (SC_Construction construction in FindObjectsOfType<SC_Construction>()) {

			if (construction.GetType ().Equals (typeof(SC_Bastion)) || construction.GetType ().Equals (typeof(SC_Wall)))
				UpdateWallGraph (construction, tileManager.GetTileAt (construction.gameObject));

			NetworkServer.Spawn (construction.gameObject);

		}

		//UpdateWallGraph(b.GetComponent<SC_Construction>(), tileManager.GetTileAt(b) /*GetTileAt((int)b.transform.position.x, (int)b.transform.position.y)*/);
		//NetworkServer.Spawn (b);

	}

    void GenerateCharacters() {

		if (baseMapPrefab == null) {
		
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

		} else {

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
		uiManager.villagePanel.SetActive (true);
		//SC_Hero.villagePanel.SetActive (false);
		SC_Character.HideCancelMovement ();
		SC_Hero.HideCancelAttack (); 

        CalcRange(tiles[(int)target.transform.position.x, (int)target.transform.position.y], target);
        
        foreach (SC_Tile tile in closedList)
            tile.DisplayMovement(tile.canSetOn);

		tileManager.GetTileAt (target.gameObject).displayMovement = true;
		tileManager.GetTileAt (target.gameObject).SetFilter("T_DisplayMovement");

        //GetTileAt ((int)target.transform.position.x, (int)target.transform.position.y).displayMovement = true;
		//GetTileAt ((int)target.transform.position.x, (int)target.transform.position.y).SetFilter("T_DisplayMovement");

    }

    void CalcRange(SC_Tile aStartingTile, SC_Character target) {

        openList.Clear();
        closedList.Clear();
		movementPoints[aStartingTile] = target.movement;
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
			neighbors.Add(tileManager.tiles[x - 1, y]);

        if ((x + 1) < SizeMapX)
			neighbors.Add(tileManager.tiles[x + 1, y]);

        if ((y - 1) >= 0)
			neighbors.Add(tileManager.tiles[x, y - 1]);

        if ((y + 1) < SizeMapY)
			neighbors.Add(tileManager.tiles[x, y + 1]);

        return neighbors;

    }

    public static SC_GameManager GetInstance() {
		
        return instance;

    }		
		
    public void NextTurn() {    

	    turn++;

        foreach (SC_Tile tile in tiles)
            tile.RemoveFilter();

        foreach (SC_Character character in FindObjectsOfType<SC_Character>()) {

			SC_Tile under = tileManager.GetTileAt (character.gameObject);
			//SC_Tile under = GetTileAt((int)character.transform.position.x, (int)character.transform.position.y);

			if (character.isHero ())
				((SC_Hero)character).Regen ();
			else if (((SC_Soldier)character).curse1)
				cursedTiles.AddRange (GetNeighbors (under));

			character.attacking = false;
			character.UnTired ();
            
            under.movementCost = (character.coalition != CoalitionTurn()) ? 5000 : under.baseCost;
            
			if (tileManager.GetAt<SC_Construction>(under))//GetConstructionAt (under) == null)               
                under.attackable = (character.coalition != CoalitionTurn ());

        }

		Curse1 ();

		foreach(SC_Construction construction in FindObjectsOfType<SC_Construction>()) {
			
			if (construction.GetType ().Equals (typeof(SC_Wall)) || construction.GetType ().Equals (typeof(SC_Bastion)))
				tileManager.GetTileAt (construction.gameObject).attackable = CoalitionTurn ();
				//GetTileAt((int)construction.transform.position.x, (int)construction.transform.position.y).attackable = CoalitionTurn ();

		}
			
        SC_Hero.HideWeapons ();
		uiManager.villagePanel.SetActive (false);
		//SC_Hero.villagePanel.SetActive (false);
		SC_Hero.HidePower ();
		SC_Character.HideCancelMovement ();
		SC_Hero.HideCancelAttack ();

		foreach (SC_Convoy convoy in FindObjectsOfType<SC_Convoy>()) {

			convoy.MoveConvoy ();
			SC_Tile under = tileManager.GetTileAt (convoy.gameObject);
            //SC_Tile under = GetTileAt((int)convoy.transform.position.x, (int)convoy.transform.position.y);
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
			//uiManager.constructWallButton.SetActive (false);
			//uiManager.endConstructionButton.SetActive (false);
			uiManager.HideButton ("qinPower");
			//uiManager.powerQinButton.SetActive (false);
			//uiManager.cancelPowerQinButton.SetActive (false);
			uiManager.HideButton ("sacrifice");
			//uiManager.sacrificeUnitButton.SetActive (false);
			//uiManager.cancelSacrificeButton.SetActive (false);

			/*constructWallButton.gameObject.SetActive (false);
			endConstructionButton.gameObject.SetActive (false);
			powerQinButton.gameObject.SetActive (false);
			cancelPowerQinButton.gameObject.SetActive (false);
			sacrificeUnitButton.gameObject.SetActive (false);
			cancelSacrificeButton.gameObject.SetActive (false);*/

			foreach (SC_Character character in FindObjectsOfType<SC_Character>()) {
				
				character.SetCanMove (character.coalition);
				if (character.isHero ()) ((SC_Hero)character).berserkTurn = false;
					
			}

		}

        //turns.text = (((turn - 1) % 3) == 0) ? "1st Turn - Coalition" : (((turn - 2) % 3) == 0) ? "2nd Turn - Coalition" : "Turn Qin";
		uiManager.SetTurnText (turn);
        
    }

    public bool CoalitionTurn() {

        return ((turn % 3) != 0);

    }
	
	/*public SC_Character GetCharacterAt(SC_Tile tile) {

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

    }*/

	public void DisplayConstructableTiles() {

		foreach (SC_Tile tile in tiles)
            tile.RemoveFilter();

        SC_Character.ResetAttacker();

        List<SC_Tile> constructableTiles = new List<SC_Tile>();

        if (!bastion) {

			uiManager.ToggleButton ("construct");
			//uiManager.constructWallButton.SetActive (false);
			//uiManager.endConstructionButton.SetActive (true);
			uiManager.ToggleButton ("sacrifice");
			//uiManager.sacrificeUnitButton.SetActive (true);
			//uiManager.cancelSacrificeButton.SetActive (false);
			uiManager.workshopPanel.SetActive (false);

			/*constructWallButton.gameObject.SetActive (false);
			endConstructionButton.gameObject.SetActive (true);
			sacrificeUnitButton.gameObject.SetActive (true);
			cancelSacrificeButton.gameObject.SetActive (false);
            workshopPanel.SetActive(false);*/
            SC_Character.HideCancelMovement();

            foreach (SC_Construction construction in FindObjectsOfType<SC_Construction>()) {

				if (construction.name.Contains ("Bastion") || construction.name.Contains ("Wall"))
					constructableTiles.AddRange (GetNeighbors(tileManager.GetTileAt (construction.gameObject)));//GetTileAt ((int)construction.transform.position.x, (int)construction.transform.position.y)));

			}

		    List<SC_Tile> constructableTilesTemp = new List<SC_Tile> (constructableTiles);

		    foreach (SC_Tile tile in constructableTilesTemp) {

			    /*int x = (int)tile.transform.position.x;
			    int y = (int)tile.transform.position.y;*/

				if (tileManager.GetAt<SC_Construction> (tile) != null) {//GetConstructionAt (x, y) != null)
					
					constructableTiles.Remove (tile);

				} else if (/*GetCharacterAt(x, y)*/tileManager.GetAt<SC_Character> (tile) != null) {
						
					if (/*GetCharacterAt(x, y)*/tileManager.GetAt<SC_Character> (tile).isHero ())
						constructableTiles.Remove (tile);

				}

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

	public void ConstructAt(SC_Tile tile) {

		/*int x = (int)pos.x;
		int y = (int)pos.y;*/

		if (/*GetCharacterAt(x, y)*/tileManager.GetAt<SC_Character> (tile) != null) {

            SC_Qin.ChangeEnergy(25);
			/*GetCharacterAt(x, y)*/tileManager.GetAt<SC_Character> (tile).DestroyCharacter();

        }

		if (/*GetConstructionAt(x, y)*/tileManager.GetAt<SC_Construction> (tile) != null)
			/*GetConstructionAt(x, y)*/tileManager.GetAt<SC_Construction> (tile).DestroyConstruction();

		Transform parentGo = GameObject.Find (bastion ? "Bastions" : "Walls").transform;
		GameObject go = bastion ? Instantiate (bastionPrefab, parentGo) : Instantiate (wallPrefab, parentGo);
		go.transform.SetPos (tile.transform);

		UpdateWallGraph (go.GetComponent<SC_Construction>(), /*GetTileAt (x, y)*/tile);

		UpdateNeighborWallGraph (/*GetTileAt (x, y)*/tile);

		if (!bastion)
            SC_Qin.ChangeEnergy (-50);

    }

	public void UpdateNeighborWallGraph(SC_Tile center) {

		foreach (SC_Tile tile in GetNeighbors(center)) {

			if((tileManager.GetAt<SC_Bastion> (tile) != null) || (tileManager.GetAt<SC_Wall> (tile) != null))
				UpdateWallGraph (tileManager.GetAt<SC_Construction> (tile), tile);

			/*if (GetConstructionAt (tile)tileManager.GetAt<SC_Construction> (tile) != null) {

				if(GetConstructionAt (tile)tileManager.GetAt<SC_Construction> (tile).GetType().Equals(typeof(SC_Wall)) || GetConstructionAt (tile)tileManager.GetAt<SC_Construction> (tile).GetType().Equals(typeof(SC_Bastion)))
					UpdateWallGraph (GetConstructionAt (tile), tile);

			}*/

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

			/*if (GetConstructionAt (tile) != null) {

				if (GetConstructionAt (tile).GetType ().Equals (typeof(SC_Wall)) || GetConstructionAt (tile).GetType ().Equals (typeof(SC_Bastion))) {*/

					if (tile.transform.position.x < under.transform.position.x)
						left = true;
					else if (tile.transform.position.x > under.transform.position.x)
						right = true;
					else if (tile.transform.position.y > under.transform.position.y)
						top = true;						

					count++;

				//}

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

			uiManager.ShowButton ("construct");
			//uiManager.constructWallButton.SetActive (true);
			uiManager.ShowButton("qinPower");
			//uiManager.powerQinButton.SetActive (true);
			uiManager.ShowButton("sacrifice");
			//uiManager.sacrificeUnitButton.SetActive (true);

			/*constructWallButton.gameObject.SetActive (true);
			powerQinButton.gameObject.SetActive (true);
			sacrificeUnitButton.gameObject.SetActive (true);*/

			bastion = false;

		} else {

			DisplayConstructableTiles ();

		}

	}

	public void EndConstruction() {

		foreach (SC_Tile tile in tiles)
			tile.RemoveFilter();

		uiManager.ToggleButton ("construct");
		//uiManager.constructWallButton.SetActive (true);
		//uiManager.endConstructionButton.SetActive (false);

		/*constructWallButton.gameObject.SetActive (true);
		endConstructionButton.gameObject.SetActive (false);*/

	}

	public void DisplaySacrifices() {

		uiManager.ToggleButton ("construct");
		//uiManager.constructWallButton.SetActive (true);
		//uiManager.endConstructionButton.SetActive (false);
		uiManager.ToggleButton("sacrifice");
		//uiManager.sacrificeUnitButton.SetActive (false);
		//uiManager.cancelSacrificeButton.SetActive (true);
		uiManager.workshopPanel.SetActive (false);

		/*constructWallButton.gameObject.SetActive (true);
		endConstructionButton.gameObject.SetActive (false);
		sacrificeUnitButton.gameObject.SetActive (false);
		cancelSacrificeButton.gameObject.SetActive (true);
        workshopPanel.SetActive(false);*/
        SC_Character.HideCancelMovement();

        foreach (SC_Tile tile in tiles)
			tile.RemoveFilter();

		SC_Character.ResetAttacker();

		foreach (SC_Soldier soldier in FindObjectsOfType<SC_Soldier>()) {

			tileManager.GetTileAt(soldier.gameObject).displaySacrifice = true;
			tileManager.GetTileAt(soldier.gameObject).SetFilter ("T_DisplaySacrifice");
			/*GetTileAt ((int)soldier.transform.position.x, (int)soldier.transform.position.y).displaySacrifice = true;
			GetTileAt ((int)soldier.transform.position.x, (int)soldier.transform.position.y).SetFilter ("T_DisplaySacrifice");*/

		}

	}

	public void HideSacrifices() {

		uiManager.ToggleButton ("sacrifice");
		//uiManager.sacrificeUnitButton.SetActive (true);
		//uiManager.cancelSacrificeButton.SetActive (false);

		/*sacrificeUnitButton.gameObject.SetActive (true);
		cancelSacrificeButton.gameObject.SetActive (false);*/

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
				tileManager.GetTileAt(wall.gameObject).RemoveFilter();
                //GetTileAt((int)wall.transform.position.x, (int)wall.transform.position.y).RemoveFilter();

			uiManager.ToggleButton ("powerQin");
			//uiManager.powerQinButton.SetActive (false);
			//uiManager.cancelPowerQinButton.SetActive (true);

			/*powerQinButton.gameObject.SetActive (false);
			cancelPowerQinButton.gameObject.SetActive (true);*/

		}

	}

	public void HideResurrectionTiles() {

		uiManager.ToggleButton ("powerQin");
		//uiManager.powerQinButton.SetActive (true);
		//uiManager.cancelPowerQinButton.SetActive (false);

		/*powerQinButton.gameObject.SetActive (true);
		cancelPowerQinButton.gameObject.SetActive (false);*/

		foreach (SC_Tile tile in tiles)
			tile.RemoveFilter ();

	}

	public void CheckAttack(SC_Character attacker) {

        foreach (SC_Tile tile in tiles)
            tile.RemoveFilter();

		SC_Hero.HideWeapons ();

        List<SC_Tile> attackableTiles = new List<SC_Tile>();

		attackableTiles.AddRange (GetNeighbors (tileManager.GetTileAt (attacker.gameObject))); //(int)attacker.transform.position.x, (int)attacker.transform.position.y)));

		if (attacker.HasRange()) {

			foreach (SC_Tile tile in GetNeighbors(tileManager.GetTileAt(attacker.gameObject)))//int)attacker.transform.position.x, (int)attacker.transform.position.y)))
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

        //attacker.AnimAttack();

        SC_Tile target = attacker.attackTarget;
		SC_Construction targetConstruction = tileManager.GetAt<SC_Construction> (target); //GetConstructionAt (target);

		if (/*GetCharacterAt (target)*/ tileManager.GetAt<SC_Character>(target) != null) {

			SC_Character attacked = tileManager.GetAt<SC_Character> (target); //GetCharacterAt (target);
            bool counterAttack = (rangedAttack && attacked.GetActiveWeapon().ranged) || (!rangedAttack && !attacked.GetActiveWeapon().IsBow());

            bool killed = attacked.Hit (calcDamages (attacker, attacked, false), false);
			SetCritDodge (attacker, attacked);

			if (attacker.isHero () && killed)
				IncreaseRelationships ((SC_Hero)attacker);

			if(counterAttack) {

                killed = attacker.Hit(calcDamages(attacked, attacker, true), false);
                SetCritDodge(attacked, attacker);

                //attacked.AnimAttack();

                if (attacked.isHero() && killed)
					IncreaseRelationships ((SC_Hero)attacked);

			}

		} else if (targetConstruction != null) {

			targetConstruction.health -= attacker.GetActiveWeapon ().weaponOrQi ? attacker.strength : attacker.qi;

			targetConstruction.lifebar.UpdateGraph (targetConstruction.health, targetConstruction.maxHealth);

			//if (targetConstruction.selfPanel) targetConstruction.ShowBuildingPanel ();
			uiManager.UpdateBuildingHealth(targetConstruction.gameObject);

			if (targetConstruction.health <= 0)
				targetConstruction.DestroyConstruction ();

		} else if (target.Qin ()) {

			SC_Qin.ChangeEnergy (-(attacker.GetActiveWeapon ().weaponOrQi ? attacker.strength : attacker.qi));

			//if (SC_Qin.selfPanel) SC_Qin.ShowQinPanel ();


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

				SC_Tile leavingTile = tileManager.GetTileAt (saver.gameObject); //GetTileAt((int)saver.transform.position.x, (int)saver.transform.position.y);

                leavingTile.movementCost = leavingTile.baseCost;
                leavingTile.canSetOn = true;
				if (/*GetConstructionAt(leavingTile)*/ tileManager.GetAt<SC_Construction>(leavingTile) == null)
                    leavingTile.attackable = true;
                leavingTile.constructable = !leavingTile.isPalace();

				saver.transform.SetPos(NearestTile (toSave).transform);

				SC_Tile newTile = tileManager.GetTileAt (saver.gameObject); //GetTileAt((int)saver.transform.position.x, (int)saver.transform.position.y);

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

		foreach(SC_Tile tile in GetNeighbors(tileManager.GetTileAt(target.gameObject))) //GetTileAt((int)target.transform.position.x, (int)target.transform.position.y)))
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

			SC_Character character = tileManager.GetAt<SC_Character> (tile); //GetCharacterAt(tile);

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

		uiManager.previewFightPanel.SetActive (true);
		//previewFightPanel.SetActive (true);

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

		if (/*GetCharacterAt*/ tileManager.GetAt<SC_Character> (attacker.attackTarget) != null) {

			SC_Character attacked = /*GetCharacterAt*/tileManager.GetAt<SC_Character> (attacker.attackTarget);

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

			int attackedType = (/*GetConstructionAt*/ tileManager.GetAt<SC_Construction> (attacker.attackTarget) != null) ? 0 : attacker.attackTarget.Qin () ? 1 : 2;

			attackedName = (attackedType == 0) ? /*GetConstructionAt*/ tileManager.GetAt<SC_Construction> (attacker.attackTarget).buildingName : (attackedType == 1) ? "Qin" : "";			
            
			int attackedHealth = (attackedType == 0) ? /*GetConstructionAt*/ tileManager.GetAt<SC_Construction> (attacker.attackTarget).health : (attackedType == 1) ? SC_Qin.GetEnergy () : 0;

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

		uiManager.previewFightPanel.SetActive (false);
		//previewFightPanel.SetActive (false);

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

			if (tileManager.GetTileAt(pos).IsEmpty() /*(GetCharacterAttileManager.GetAt<SC_Character> (baseSpawn) != null) || (GetConstructionAt (baseSpawn) != null) || (GetConvoyAt (baseSpawn) != null)*/) {
			
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
            //workshopPanel.SetActive(false);

        }

    }

    public void DisplayWorkshopPanel() {

		if((!CoalitionTurn()) && !bastion && (/*GetCharacterAt((int)currentWorkshopPos.x, (int)currentWorkshopPos.y)*/ tileManager.GetAt<SC_Character>(currentWorkshop.gameObject) == null)) {

			uiManager.ToggleButton ("construct");
			//uiManager.constructWallButton.SetActive (true);
			//uiManager.endConstructionButton.SetActive (false);
			uiManager.ToggleButton("sacrifice");
			//uiManager.sacrificeUnitButton.SetActive (true);
			//uiManager.cancelSacrificeButton.SetActive (false);

            /*constructWallButton.gameObject.SetActive(true);
            endConstructionButton.gameObject.SetActive(false);
            sacrificeUnitButton.gameObject.SetActive(true);
            cancelSacrificeButton.gameObject.SetActive(false);*/

            foreach (SC_Tile tile in tiles)
                tile.RemoveFilter();

            SC_Character.ResetAttacker();

			uiManager.workshopPanel.SetActive (true);
           // workshopPanel.SetActive(true);

        }

    }

    public void HideWorkshopPanel() {

		uiManager.workshopPanel.SetActive (false);
        //workshopPanel.SetActive(false);

    }

	public void CancelMovement() {

		SC_Character.ResetAttacker ();

		foreach (SC_Tile tile in tiles)
			tile.RemoveFilter ();

		SC_Character character = SC_Character.GetCharacterToMove ();

		SC_Tile leavingTile = tileManager.GetTileAt (character.gameObject); //GetTileAt((int)character.transform.position.x, (int)character.transform.position.y);

        leavingTile.movementCost = leavingTile.baseCost;
        leavingTile.canSetOn = true;
		if (/*GetConstructionAt*/ tileManager.GetAt<SC_Construction> (leavingTile) == null)
            leavingTile.attackable = true;
		leavingTile.constructable = !leavingTile.isPalace ();

		character.transform.SetPos (character.lastPos.transform);

		//SC_Tile previousTile = tileManager.GetTileAt (character.gameObject); //GetTileAt((int)character.transform.position.x, (int)character.transform.position.y);

		character.lastPos.movementCost = 5000;
		character.lastPos.canSetOn = false;
		if (/*GetConstructionAt*/ tileManager.GetAt<SC_Construction> (character.lastPos) == null)
			character.lastPos.attackable = (character.coalition != SC_GameManager.GetInstance ().CoalitionTurn ());
		character.lastPos.constructable = !character.isHero();

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

		uiManager.ToggleButton("health");
		//uiManager.showHealthButton.SetActive (false);
		//uiManager.hideHealthButton.SetActive (true);

        /*showHealthButton.gameObject.SetActive(false);
        hideHealthButton.gameObject.SetActive(true);*/

    }

    public void HideHealth() {

        foreach (SC_Lifebar lifebar in FindObjectsOfType<SC_Lifebar>())
            lifebar.Hide();

		uiManager.ToggleButton("health");
		//uiManager.showHealthButton.SetActive (true);
		//uiManager.hideHealthButton.SetActive (false);

        /*showHealthButton.gameObject.SetActive(true);
        hideHealthButton.gameObject.SetActive(false);*/

    }

	void Curse1() {
        
        List<SC_Tile> tempCursedTiles = new List<SC_Tile>();

        foreach (SC_Tile tile in cursedTiles)
            if (tile.constructable && !tile.attackable) tempCursedTiles.Add(tile);

        foreach (SC_Wall wall in FindObjectsOfType<SC_Wall>())
			tempCursedTiles.Remove (tileManager.GetTileAt(wall.gameObject) /*GetTileAt ((int)wall.transform.position.x, (int)wall.transform.position.y)*/);

		foreach (SC_Tile tile in tempCursedTiles)
			tileManager.GetAt<SC_Soldier>(tile).Hit(2, false);
			//((SC_Soldier)GetCharacterAt (tile)).Hit (2, false);

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
