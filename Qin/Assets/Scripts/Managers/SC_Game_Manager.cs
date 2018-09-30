using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using static SC_Enums;
using System.Collections;

public class SC_Game_Manager : NetworkBehaviour {

	public GameObject baseMapPrefab;

    public SC_Common_Characters_Variables CommonCharactersVariables { get; set; }

    public static SC_Game_Manager Instance { get; set; }  

    public int Turn { get; set; }

    public bool Qin { get { return Turn % 3 == 0; } }

    public bool QinTurnStarting { get; set; }

	public SC_Hero LastHeroDead { get; set; }

    public string CurrentConstru { get; set; }

	public Vector3 CurrentWorkshopPos { get; set; }

	public SC_Player Player { get; set; }

	SC_UI_Manager uiManager;

	SC_Tile_Manager tileManager;

    #region Setup
    private void Awake () {

        SC_Village.number = 0;

    }

    void Start() {

        CommonCharactersVariables = Resources.Load<SC_Common_Characters_Variables>("Prefabs/Characters/P_Common_Characters_Variables");

        Turn = 1;

		if(GameObject.FindGameObjectWithTag ("Player")) {
			
			Player = GameObject.FindGameObjectWithTag ("Player").GetComponent<SC_Player> ();

            Player.SetSide();

			Player.SetGameManager (this);

		}

		if (isServer) {

			GenerateMap ();
			SetupTileManager ();

		}

		QinTurnStarting = true;

		if (!Instance)
			Instance = this;

        uiManager = SC_UI_Manager.Instance;
		uiManager.SetupUI (FindObjectOfType<SC_Network_Manager>().IsQinHost() == isServer);

    }

	void GenerateMap() {

		foreach (Transform child in baseMapPrefab.transform) {

			SC_EditorTile eTile = child.GetComponent<SC_EditorTile> ();

			GameObject tilePrefab = Resources.Load<GameObject>("Prefabs/Tiles/P_" + eTile.tileType);

            GameObject go = Instantiate (tilePrefab, new Vector3(eTile.transform.position.x, eTile.transform.position.y, 0), eTile.transform.rotation,  GameObject.Find ("Tiles").transform);

			NetworkServer.Spawn (go);

		}

	}

	void SetupTileManager() {

		GameObject tm = Instantiate (Resources.Load<GameObject>("Prefabs/P_Tile_Manager"));
		SC_Tile_Manager stm = tm.GetComponent<SC_Tile_Manager> ();

		stm.xSize = baseMapPrefab.GetComponent<SC_MapPrefab>().xSize;
		stm.ySize = baseMapPrefab.GetComponent<SC_MapPrefab>().ySize;		

		NetworkServer.Spawn (tm);

	}

	public void FinishSetup() {

        tileManager = SC_Tile_Manager.Instance;

		if (isServer)
			GenerateElements ();

	}

	void GenerateElements() {

		foreach (Transform child in baseMapPrefab.transform) {

			SC_EditorTile eTile = child.GetComponent<SC_EditorTile> ();

            GameObject[] soldiers = Resources.LoadAll<GameObject>("Prefabs/Characters/Soldiers");

            if (eTile.construction != ConstructionType.None) {

                GameObject constructionPrefab = Resources.Load<GameObject>("Prefabs/Constructions/P_" + eTile.construction);
                if (!constructionPrefab)
                    constructionPrefab = Resources.Load<GameObject>("Prefabs/Constructions/Production/P_" + eTile.construction);

                GameObject go = Instantiate (constructionPrefab, eTile.transform.position + new Vector3(0, 0, -.51f), Quaternion.identity);

                go.transform.parent = GameObject.Find(eTile.construction + "s").transform;

                NetworkServer.Spawn (go);

			} else if (eTile.spawnSoldier || eTile.qin || eTile.heroPrefab) {

                GameObject go = Instantiate(eTile.spawnSoldier ? soldiers[Random.Range(0, soldiers.Length)].gameObject : eTile.qin ? Resources.Load<GameObject>("Prefabs/Characters/P_Qin") : eTile.heroPrefab);

                go.transform.SetPos(eTile.transform);

                go.transform.parent = (eTile.spawnSoldier) ? GameObject.Find("Soldiers").transform : (eTile.qin) ? null : GameObject.Find("Heroes").transform;

                NetworkServer.Spawn(go);

            } else if (eTile.ruin) {

                NetworkServer.Spawn(Instantiate(Resources.Load<GameObject>("Prefabs/P_Ruin"), eTile.transform.position + new Vector3(0, 0, -.51f), Quaternion.identity));

            }

        }

	}

    public IEnumerator FinishLoading() {

        while(!Player)
            yield return null;

        Player.CmdFinishLoading();

    }
    #endregion

    #region Next Turn
    // Called by UI
    public void NextTurn () {

        if (!Player.Busy)
            Player.CmdNextTurn();

    }

    public void NextTurnFunction() {    

	    Turn++;

        tileManager.RemoveAllFilters();

        foreach (SC_Character character in FindObjectsOfType<SC_Character>()) {

			if (character.Hero) {

                character.Hero.Regen ();

				if (Qin) {

					if (character.Hero.PowerUsed)
                        character.Hero.PowerBacklash++;

					if (character.Hero.PowerBacklash >= 2)
                        character.Hero.DestroyCharacter ();  

				} else {

                    character.Hero.BerserkTurn = false;

				}

			}

			character.UnTired ();
            
            character.CanMove = character.Qin == Qin;

        }

        SC_Character.attackingCharacter = null;

        /*foreach (SC_Convoy convoy in FindObjectsOfType<SC_Convoy>())
			convoy.MoveConvoy ();*/

        CurrentConstru = "Bastion";

        if (Qin) {

            foreach (SC_Pump p in FindObjectsOfType<SC_Pump>())
                p.Drain();

            SC_Qin.ChangeEnergy(SC_Qin.Qin.regenPerVillage * SC_Village.number);

            if (Player.Qin) {

                Player.Busy = true;

                QinTurnStarting = true;

                tileManager.DisplayConstructableTiles(false);

            }


		}

		uiManager.NextTurn ();
        
    }
    #endregion

    #region Methods called by UI
    public void SetAttackWeapon (bool usedActiveWeapon) {

        Player.CmdHeroAttack(usedActiveWeapon);

    }

    public void CancelLastConstruction () {

        uiManager.cancelButton.gameObject.SetActive(false);

        Player.CmdCancelLastConstru();

    }

    /*public void UseHeroPower () {

        SC_Hero hero = GameObject.Find(GameObject.Find("PowerHero").GetComponentInChildren<Text>().name).GetComponent<SC_Hero>();
        hero.PowerUsed = true;

        GameObject.Find("PowerHero").SetActive(false);

        print("Implement Power");

    }*/

    public void DestroyProductionBuilding () {

        Player.CmdDestroyProductionBuilding();

    }

    public void UnselectCharacter () {

        Player.CmdRemoveAllFilters();

        uiManager.cancelButton.gameObject.SetActive(false);

    }

    public void ResetMovement () {

        SC_Player.localPlayer.CmdResetMovement();

    }
    #endregion

    #region Construction
    public void SoldierConstruct(int id) {

        uiManager.soldierConstructPanel.gameObject.SetActive(false);

        uiManager.cancelButton.gameObject.SetActive(false);

        tileManager.RemoveAllFilters();

        Player.CmdSetConstru(uiManager.soldiersConstructions[id].Name);

        Player.CmdConstructAt(Mathf.RoundToInt(SC_Character.attackingCharacter.transform.position.x), Mathf.RoundToInt(SC_Character.attackingCharacter.transform.position.y));

        Player.Busy = false;

    }

    public void ConstructAt (int x, int y) {

        tileManager.RemoveAllFilters();

        SC_Tile tile = tileManager.GetTileAt(x, y);

        bool qinConstru = !tile.Soldier || QinTurnStarting;

        if (tile.Soldier) {

            if (!tile.Ruin) {

                uiManager.HideInfosIfActive(tile.Soldier.gameObject);

                if (QinTurnStarting) {

                    SC_Construction.lastConstruSoldier = tile.Soldier;

                    tile.Soldier.gameObject.SetActive(false);

                    SC_Qin.ChangeEnergy(tile.Soldier.sacrificeValue);

                    tile.Character = null;

                } else {

                    tile.Soldier.DestroyCharacter();

                }

            } else {

                tile.Ruin.DestroyRuin();                

                uiManager.Wait();

            }

        }        

        if (isServer) {

            GameObject go = Resources.Load<GameObject>("Prefabs/Constructions/P_" + CurrentConstru);
            if(!go)
                go = Resources.Load<GameObject>("Prefabs/Constructions/Production/P_" + CurrentConstru);

            go = Instantiate(go, tile.transform.position + new Vector3(0, 0, -.51f), Quaternion.identity);

            NetworkServer.Spawn(go);

            if(qinConstru)
                Player.CmdSetLastConstru(go);

            Player.CmdFinishConstruction(qinConstru);

        }

    }

    public void FinishConstruction (bool qinConstru) {        

        if (QinTurnStarting) {

            Player.Busy = false;

            uiManager.construct.gameObject.SetActive(true);
            //uiManager.qinPower.gameObject.SetActive(true);
            uiManager.sacrifice.gameObject.SetActive(true);
            uiManager.endTurn.SetActive(true);

        } else {

            if (qinConstru) {

                SC_Qin.ChangeEnergy(-SC_Qin.GetConstruCost(CurrentConstru));

                Player.CmdChangeQinEnergyOnClient(-SC_Qin.GetConstruCost(CurrentConstru), false);

                uiManager.UpdateQinConstructPanel();

                if ((SC_Qin.GetConstruCost(CurrentConstru) < SC_Qin.Energy) && (tileManager.GetConstructableTiles(CurrentConstru == "Wall").Count > 0))
                    tileManager.DisplayConstructableTiles(CurrentConstru == "Wall");

            } else {

                Player.CmdChangeQinEnergy(-SC_Qin.GetConstruCost(CurrentConstru));

            }

        }

        if (qinConstru)
            uiManager.SetCancelButton(CancelLastConstruction);

    }
    #endregion

    #region Players Actions  
    public void DestroyOnCase () {

        SC_Tile tile = tileManager.GetTileAt(SC_Character.attackingCharacter.gameObject);

        tile.Construction?.DestroyConstruction();

        tile.Ruin?.DestroyRuin();

        uiManager.Wait();

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

    public void CreateSoldier(Vector3 pos, string soldierName) {

        GameObject go = Instantiate(Resources.Load<GameObject>("Prefabs/Characters/Soldiers/P_" + soldierName), GameObject.Find("Soldiers").transform);
        go.transform.SetPos(pos);

        NetworkServer.Spawn(go);

        Player.CmdSetupNewSoldier(go);

    }
    #endregion

}
