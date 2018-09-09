using UnityEngine;

public class SC_Bastion : SC_Construction {

    protected override void Start() {

        base.Start();

        lifebar = Instantiate(Resources.Load<GameObject>("Prefabs/P_Lifebar"), transform).GetComponent<SC_Lifebar>();
        lifebar.transform.position += new Vector3(0, -.44f, 0);

        tileManager.UpdateWallGraph(gameObject);

        tileManager.UpdateNeighborWallGraph(tileManager.GetTileAt(gameObject));

    }

    public override void DestroyConstruction () {

		gameObject.SetActive (false);

		base.DestroyConstruction ();

        tileManager.UpdateNeighborWallGraph (tileManager.GetTileAt (gameObject));

	}

}
