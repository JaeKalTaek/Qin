using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using static SC_Enums;

public class SC_Game_Manager : NetworkBehaviour {

    //Prefabs
	public GameObject baseMapPrefab;
    public GameObject plainPrefab, forestPrefab, mountainPrefab, palacePrefab;
    public GameObject qinPrefab, convoyPrefab;
    public SC_Soldier[] soldiersPrefabs;
    public SC_Common_Characters_Variables commonCharactersVariables;
	public List<GameObject> heroPrefabs;
	public GameObject tileManagerPrefab;
    
	//Instance
    public static SC_Game_Manager Instance { get; set; }  

    int turn;

    public bool CoalitionTurn { get { return turn % 3 != 0; } }

    //Other
    public bool Bastion { get; set; }

	public SC_Hero LastHeroDead { get; set; }

    public Constru CurrentConstru { get; set; }

	public SC_Workshop CurrentWorkshop { get; set; }

	public bool CantCancelMovement { get; set; }

	public SC_Player Player { get; set; }

	SC_UI_Manager uiManager;

	SC_Tile_Manager tileManager;

    #region Setup
    private void Awake () {

        SC_Village.number = 0;

    }

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

				GameObject go2 = Instantiate (eTile.spawnSoldier ? soldiersPrefabs[Random.Range(0, soldiersPrefabs.Length)].gameObject : eTile.qin ? qinPrefab : eTile.heroPrefab);

				go2.transform.SetPos (eTile.transform);

				go2.transform.parent = (eTile.spawnSoldier) ? GameObject.Find ("Soldiers").transform : (eTile.qin) ? null : GameObject.Find ("Heroes").transform;

				NetworkServer.Spawn (go2);

			}

		}		

    }
    #endregion

    #region Next Turn
	public void NextTurnFunction() {    

	    turn++;

        tileManager.RemoveAllFilters();

        foreach (SC_Character character in FindObjectsOfType<SC_Character>()) {

			if (character.IsHero) {

                character.Hero.Regen ();

				if (!CoalitionTurn) {

					if (character.Hero.PowerUsed)
                        character.Hero.PowerBacklash++;

					if (character.Hero.PowerBacklash >= 2)
                        character.Hero.DestroyCharacter ();  

				} else {

                    character.Hero.BerserkTurn = false;

				}

			}

			character.UnTired ();
            
			bool turn = character.coalition == CoalitionTurn;

            character.CanMove = turn;

        }

        SC_Character.attackingCharacter = null;

        /*foreach (SC_Convoy convoy in FindObjectsOfType<SC_Convoy>())
			convoy.MoveConvoy ();*/

        CurrentConstru = Constru.Bastion;

        if (!CoalitionTurn) {

            Player.Busy = true;

            SC_Qin.ChangeEnergy(SC_Qin.Qin.regenPerVillage * SC_Village.number);

            if (Player.qin) {

                Bastion = true;

                tileManager.DisplayConstructableTiles(false);

            }


		}

		uiManager.NextTurn (CoalitionTurn, turn);
        
    }
    #endregion

    public void SetAttackWeapon (bool usedActiveWeapon) {

        Player.CmdHeroAttack(usedActiveWeapon);

    }

    public void ConstructAt (SC_Tile tile) {

        Player.CmdConstructAt((int)tile.transform.position.x, (int)tile.transform.position.y);

    }

    public void ConstructAt(int x, int y) {

        SC_Tile tile = tileManager.GetTileAt (x, y);

        if(tile.Soldier) {

            tile.Soldier.gameObject.SetActive(false);

            SC_Qin.ChangeEnergy(SC_Qin.Qin.sacrificeValue);

        }

        SC_Construction.lastConstruSoldier = tile.Soldier;

        if(isServer) {

            GameObject go = Instantiate(Resources.Load<GameObject>("Prefabs/Constructions/P_" + CurrentConstru));
            go.transform.SetPos(tile.transform);

            NetworkServer.Spawn(go);

            Player.CmdSetLastConstru(go);

        }

        if(Player.qin) {

            tileManager.RemoveAllFilters();

            if (Bastion) {

                Player.Busy = false;

                foreach (SC_Character character in FindObjectsOfType<SC_Character>())
                    character.CanMove = !character.coalition;

                uiManager.construct.gameObject.SetActive(true);
                uiManager.qinPower.gameObject.SetActive(true);
                uiManager.sacrifice.gameObject.SetActive(true);
                uiManager.endTurn.SetActive(true);

            } else {

                SC_Qin.ChangeEnergy(-SC_Qin.GetConstruCost(CurrentConstru));

                Player.CmdChangeQinEnergyOnClient(-SC_Qin.GetConstruCost(CurrentConstru), false);

                tile.Locked = true;

                uiManager.UpdateConstructPanel();
                
                if ((SC_Qin.GetConstruCost(CurrentConstru) < SC_Qin.Energy) && (tileManager.GetConstructableTiles(CurrentConstru == Constru.Wall).Count > 0))  
                    tileManager.DisplayConstructableTiles(CurrentConstru == Constru.Wall);

            }

            uiManager.cancelLastConstructButton.SetActive(true);

        }

    }	

    public void CancelLastConstruction() {

        Player.CmdCancelLastConstru();

    }

	public void DisplaySacrifices() {

        if (!Player.Busy) {

            uiManager.StartQinAction("sacrifice");

            tileManager.DisplaySacrifices();

        }

	}

	public void DisplayResurrection() {

        if (!Player.Busy && LastHeroDead && (SC_Qin.Energy > SC_Qin.Qin.powerCost)) {

            uiManager.StartQinAction("qinPower");

            tileManager.DisplayResurrection();

        }

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

            uiManager.resetMovementButton.SetActive(true);

        }

        uiManager.villagePanel.SetActive(false);

        tileManager.CheckAttack();

        Player.Busy = false;

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

    public void CreateSoldier(Vector3 pos, int soldierID) {

        GameObject go = Instantiate(soldiersPrefabs[soldierID].gameObject, GameObject.Find("Soldiers").transform);
        go.transform.SetPos(pos);

        NetworkServer.Spawn(go);

        Player.CmdSetupNewSoldier(go);

    }
    #endregion    

    public void UseHeroPower() {

		SC_Hero hero = GameObject.Find (GameObject.Find ("PowerHero").GetComponentInChildren<Text> ().name).GetComponent<SC_Hero>();
		hero.PowerUsed = true;

		GameObject.Find ("PowerHero").SetActive (false);

        print("Implement Power");

	}	

    public void CancelMovement() {

        Player.CmdRemoveAllFilters();

        uiManager.cancelMovementButton.SetActive(false);

    }

    public void ResetMovement() {

        SC_Player.localPlayer.CmdResetMovement();

    }

}
