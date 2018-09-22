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

    public bool QinTurnBeginning { get; set; }

	public SC_Hero LastHeroDead { get; set; }

    public Constru CurrentConstru { get; set; }

	public Vector3 CurrentWorkshopPos { get; set; }

	public bool CantCancelMovement { get; set; }

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

		QinTurnBeginning = true;

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

		if (isServer) {

			GenerateBuildings ();
			GenerateCharacters ();

		}

	}

	void GenerateBuildings() {

		foreach (Transform child in baseMapPrefab.transform) {

			SC_EditorTile eTile = child.GetComponent<SC_EditorTile> ();

			if (eTile.construction != constructionType.None) {

                GameObject constructionPrefab = Resources.Load<GameObject>("Prefabs/Constructions/P_" + eTile.construction);

				GameObject go2 = Instantiate (constructionPrefab);

				go2.transform.SetPos (eTile.transform);

                go2.transform.parent = GameObject.Find(eTile.construction + "s").transform;

                NetworkServer.Spawn (go2);

			}

		}

		foreach (SC_Construction construction in FindObjectsOfType<SC_Construction>())
			NetworkServer.Spawn (construction.gameObject);

	}

    void GenerateCharacters() {

        GameObject[] soldiers = Resources.LoadAll<GameObject>("Prefabs/Characters/Soldiers");

		foreach (Transform child in baseMapPrefab.transform) {

			SC_EditorTile eTile = child.GetComponent<SC_EditorTile> ();

			if (eTile.spawnSoldier || eTile.qin || eTile.heroPrefab) {

                GameObject go2 = Instantiate(eTile.spawnSoldier ? soldiers[Random.Range(0, soldiers.Length)].gameObject : eTile.qin ? Resources.Load<GameObject>("Prefabs/Characters/P_Qin") : eTile.heroPrefab);

				go2.transform.SetPos (eTile.transform);

				go2.transform.parent = (eTile.spawnSoldier) ? GameObject.Find ("Soldiers").transform : (eTile.qin) ? null : GameObject.Find ("Heroes").transform;

				NetworkServer.Spawn (go2);

			}

		}		

    }

    public IEnumerator FinishLoading() {

        while(!Player)
            yield return null;

        Player.CmdFinishLoading(); ;

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

        CurrentConstru = Constru.Bastion;

        if (Qin) {

            foreach (SC_Pump p in FindObjectsOfType<SC_Pump>())
                p.Drain();

            SC_Qin.ChangeEnergy(SC_Qin.Qin.regenPerVillage * SC_Village.number);

            if (Player.Qin) {

                Player.Busy = true;

                QinTurnBeginning = true;

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

        Player.CmdCancelLastConstru();

    }

    /*public void UseHeroPower () {

        SC_Hero hero = GameObject.Find(GameObject.Find("PowerHero").GetComponentInChildren<Text>().name).GetComponent<SC_Hero>();
        hero.PowerUsed = true;

        GameObject.Find("PowerHero").SetActive(false);

        print("Implement Power");

    }*/

    public void ActionVillage (bool destroy) {

        Player.CmdActionVillage(destroy);

    }

    public void CancelMovement () {

        Player.CmdRemoveAllFilters();

        uiManager.cancelMovementButton.SetActive(false);

    }

    public void ResetMovement () {

        SC_Player.localPlayer.CmdResetMovement();

    }
    #endregion

    #region Construction
    public void ConstructAt (SC_Tile tile) {

        Player.CmdConstructAt(Mathf.RoundToInt(tile.transform.position.x), Mathf.RoundToInt(tile.transform.position.y));

    }

    public void ConstructAt (int x, int y) {

        SC_Tile tile = tileManager.GetTileAt(x, y);

        if (tile.Soldier) {

            uiManager.HideInfosIfActive(tile.Soldier.gameObject);

            tile.Soldier.gameObject.SetActive(false);

            SC_Qin.ChangeEnergy(tile.Soldier.sacrificeValue);

        }

        SC_Construction.lastConstruSoldier = tile.Soldier;

        if (isServer) {

            GameObject go = Instantiate(Resources.Load<GameObject>("Prefabs/Constructions/P_" + CurrentConstru));
            go.transform.SetPos(tile.transform);

            NetworkServer.Spawn(go);

            Player.CmdSetLastConstru(go);

            Player.CmdFinishConstruction();

        }

    }

    public void FinishConstruction () {

        tileManager.RemoveAllFilters();

        if (QinTurnBeginning) {

            Player.Busy = false;

            foreach (SC_Character character in FindObjectsOfType<SC_Character>())
                character.CanMove = character.Qin;

            uiManager.construct.gameObject.SetActive(true);
            //uiManager.qinPower.gameObject.SetActive(true);
            uiManager.sacrifice.gameObject.SetActive(true);
            uiManager.endTurn.SetActive(true);

        } else {

            SC_Qin.ChangeEnergy(-SC_Qin.GetConstruCost(CurrentConstru));

            Player.CmdChangeQinEnergyOnClient(-SC_Qin.GetConstruCost(CurrentConstru), false);

            uiManager.UpdateConstructPanel();

            if ((SC_Qin.GetConstruCost(CurrentConstru) < SC_Qin.Energy) && (tileManager.GetConstructableTiles(CurrentConstru == Constru.Wall).Count > 0))  
                tileManager.DisplayConstructableTiles(CurrentConstru == Constru.Wall);

        }

        uiManager.cancelLastConstructButton.SetActive(true);

    }
    #endregion

    #region Players Actions
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

    public void CreateSoldier(Vector3 pos, string soldierName) {

        GameObject go = Instantiate(Resources.Load<GameObject>("Prefabs/Characters/Soldiers/P_" + soldierName), GameObject.Find("Soldiers").transform);
        go.transform.SetPos(pos);

        NetworkServer.Spawn(go);

        Player.CmdSetupNewSoldier(go);

    }
    #endregion

}
