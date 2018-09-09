using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

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

    #region Next Turn
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
                tileManager.DisplayConstructableTiles ();

		}

		uiManager.NextTurn (CoalitionTurn, turn);
        
    }
    #endregion

    public void SetAttackWeapon (bool usedActiveWeapon) {

        Player.CmdHeroAttack(usedActiveWeapon);

    }

    public void DisplayConstructableTiles () {

        tileManager.DisplayConstructableTiles();

    }

    public void ConstructAt (SC_Tile tile) {

        Player.CmdConstructAt((int)tile.transform.position.x, (int)tile.transform.position.y);

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

	public void DisplaySacrifices() {

        tileManager.DisplaySacrifices();

	}

	public void DisplayResurrectionTiles() {

        tileManager.DisplayResurrectionTiles();

	}

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
    #endregion    

    public void UseHeroPower() {

		SC_Hero hero = GameObject.Find (GameObject.Find ("PowerHero").GetComponentInChildren<Text> ().name).GetComponent<SC_Hero>();
		hero.powerUsed = true;

		GameObject.Find ("PowerHero").SetActive (false);

        print("Implement Power");

	}	

    public void ResetMovement() {

        SC_Player.localPlayer.CmdResetMovement();

    }

}
