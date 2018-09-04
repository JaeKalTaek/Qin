using UnityEngine;

public class SC_Bastion : SC_Construction {

    protected override void Start() {

        base.Start();

        lifebar = Instantiate(Resources.Load<GameObject>("Prefabs/P_Lifebar"), transform).GetComponent<SC_Lifebar>();
        lifebar.transform.position += new Vector3(0, -.44f, 0);

        gameManager.UpdateWallGraph(gameObject);

        gameManager.UpdateNeighborWallGraph(tileManager.GetTileAt(gameObject));

    }

    public override void DestroyConstruction () {

		gameObject.SetActive (false);

		base.DestroyConstruction ();

		gameManager.UpdateNeighborWallGraph (tileManager.GetTileAt (gameObject));

	}

}
