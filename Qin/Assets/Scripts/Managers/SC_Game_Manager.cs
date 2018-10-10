using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using static SC_EditorTile;

public class SC_Game_Manager : NetworkBehaviour {

	public SC_MapEditorScript baseMapPrefab;

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

		if (!Instance)
			Instance = this;

        uiManager = SC_UI_Manager.Instance;
		uiManager.SetupUI (FindObjectOfType<SC_Network_Manager>().IsQinHost() == isServer);

    }

	void GenerateMap() {

        foreach (Transform child in baseMapPrefab.transform) {

            SC_EditorTile eTile = child.GetComponent<SC_EditorTile>();

            GameObject go = Instantiate(Resources.Load<GameObject>("Prefabs/Tiles/P_" + eTile.tileType), child.position, Quaternion.identity, GameObject.Find("Tiles").transform);

            SC_Tile tile = go.GetComponent<SC_Tile>();

            tile.river = eTile.tileType == TileType.River;

            tile.riverSprite = (int)eTile.riverSprite;            

            NetworkServer.Spawn(go);

        }

	}

	void SetupTileManager() {

		GameObject tm = Instantiate (Resources.Load<GameObject>("Prefabs/P_Tile_Manager"));
		SC_Tile_Manager stm = tm.GetComponent<SC_Tile_Manager> ();

        stm.xSize = baseMapPrefab.SizeMapX;
        stm.ySize = baseMapPrefab.SizeMapY;	

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

            if (eTile.construction != ConstructionType.None) {

                GameObject constructionPrefab;

                constructionPrefab = Resources.Load<GameObject>(eTile.construction == ConstructionType.Ruin ? "Prefabs/P_Ruin" : "Prefabs/Constructions/P_" + eTile.construction);

                if (!constructionPrefab)
                    constructionPrefab = Resources.Load<GameObject>("Prefabs/Constructions/Production/P_" + eTile.construction);

                GameObject go = Instantiate (constructionPrefab, eTile.transform.position + new Vector3(0, 0, -.52f), Quaternion.identity);

                go.transform.parent = GameObject.Find(eTile.construction + "s").transform;

                NetworkServer.Spawn (go);

			}

            if ((eTile.soldier != SoldierType.None) || eTile.Qin || (eTile.Hero != HeroType.None)) {

                string path = "Prefabs/Characters/" + (eTile.soldier != SoldierType.None ? "Soldiers/P_" + eTile.soldier : (eTile.Qin ? "P_Qin" : "Heroes/P_" + eTile.Hero));

                GameObject go = Instantiate(Resources.Load<GameObject>(path));

                go.transform.SetPos(eTile.transform);

                go.transform.parent = eTile.soldier != SoldierType.None ? GameObject.Find("Soldiers").transform : (eTile.Qin ? null : GameObject.Find("Heroes").transform);

                NetworkServer.Spawn(go);

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

            QinTurnStarting = true;

            if (Player.Qin) {

                Player.Busy = true;                

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

        SC_Character.characterToMove = null;

        tileManager.RemoveAllFilters();

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

        SC_Cursor.Instance.Locked = false;

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

            go = Instantiate(go, tile.transform.position + new Vector3(0, 0, -.52f), Quaternion.identity);

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
