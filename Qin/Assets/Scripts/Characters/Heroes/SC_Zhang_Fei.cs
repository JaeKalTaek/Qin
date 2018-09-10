public class SC_Zhang_Fei : SC_Hero {

    /*public GameObject trapPrefab, trap2Prefab;
    GameObject trap, trap2;
    List<SC_Tile> column;
    Vector3 upEnd, bottomEnd;
    bool fxTrapStart;

    protected override void Start() {

        base.Start();

        column = new List<SC_Tile>();

    }

    void Update() {

        if(fxTrapStart) {

            trap.transform.position = Vector3.Lerp(trap.transform.position, upEnd, 0.05f);
            trap2.transform.position = Vector3.Lerp(trap2.transform.position, bottomEnd, 0.05f);

            if ((trap.transform.position.y >= (upEnd.y - 0.1f)) && (trap2.transform.position.y <= (bottomEnd.y + 0.1f)))
                fxTrapStart = false;

        }

    }

    public override void DestroyCharacter () {

		foreach (SC_Tile tile in SC_GameManager.GetInstance().tiles)
			if (tile.transform.position.x == transform.position.x) column.Add (tile);

        upEnd = column[column.Count - 1].transform.position;
        bottomEnd = column[0].transform.position;

        trap = Instantiate(trapPrefab, transform);
        trap2 = Instantiate(trap2Prefab, transform);

        trap.transform.position = new Vector3(transform.position.x, transform.position.y , -1.5f);
        trap2.transform.position = new Vector3(transform.position.x, transform.position.y, -1.5f);

        upEnd.z = trap.transform.position.z;
        bottomEnd.z = trap2.transform.position.z;

        foreach (ParticleSystem ps in trap.GetComponentsInChildren<ParticleSystem>()) {

                ParticleSystem.EmissionModule em = ps.emission;
                em.enabled = true;

        }

        foreach(ParticleSystem ps in trap2.GetComponentsInChildren<ParticleSystem>()) {

                ParticleSystem.EmissionModule em = ps.emission;
                em.enabled = true;

        }
        
        fxTrapStart = true;

        DestroyColumn();

        Invoke("DestroyAfterTrap", 1f);

    }

    void DestroyAfterTrap() {

        Destroy(trap);
        Destroy(trap2);
        fxTrapStart = false;

        base.DestroyCharacter();

    }

    void DestroyColumn () {

		foreach (SC_Tile tile in column) {
            
			if (!tile.name.Contains ("Plain") && !tile.name.Contains ("Palace")) {
	
				GameObject go = Instantiate (SC_GameManager.GetInstance ().PlainPrefab);
				go.transform.position = new Vector3(tile.transform.position.x, tile.transform.position.y, 0);
                SC_GameManager.GetInstance().tiles[(int)go.transform.position.x, (int)go.transform.position.y] = go.GetComponent<SC_Tile>();
                Destroy(tile.gameObject);

            } else {

                tile.transform.position = new Vector3(tile.transform.position.x, tile.transform.position.y, 0);

            }

			if (SC_GameManager.GetInstance ().GetCharacterAt (tile) != null) {

                if (!SC_GameManager.GetInstance().GetCharacterAt(tile).coalition)
                    SC_GameManager.GetInstance().GetCharacterAt(tile).DestroyCharacter();

			} else if (SC_GameManager.GetInstance ().GetConstructionAt (tile) != null) {
				
				SC_GameManager.GetInstance ().GetConstructionAt (tile).DestroyConstruction();

			}

		}

    }*/

}
