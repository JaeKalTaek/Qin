using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class SC_Convoy : NetworkBehaviour {

	SC_Tile_Manager tileManager;

	void Start () {

		tileManager = GameObject.FindObjectOfType<SC_Tile_Manager> ();

		SC_Tile under = tileManager.GetTileAt (gameObject); //SC_GameManager.GetInstance().GetTileAt((int)transform.position.x, (int)transform.position.y);

        under.constructable = false;
        under.attackable = false;

	}

	public void OnMouseDown() {

		//SC_Tile under = SC_GameManager.GetInstance().GetTileAt((int)transform.position.x, (int)transform.position.y);

		if (tileManager.GetTileAt (gameObject).displayMovement)
			SC_Character.GetCharacterToMove().MoveTo ((int)transform.position.x, (int)transform.position.y);

	}

	public void MoveConvoy() {

		Vector3 targetPos = (transform.position + new Vector3 (-1, 0, 0));

		if(/*SC_GameManager.GetInstance().GetTileAt((int)targetPos.x, (int)targetPos.y)*/tileManager.GetTileAt (targetPos).IsEmpty()) {

			/*int x = (int)transform.position.x;
			int y = (int)transform.position.y;*/

			SC_Tile leavingTile = tileManager.GetTileAt (gameObject);//SC_GameManager.GetInstance().GetTileAt(x, y);
            leavingTile.constructable = !leavingTile.isPalace();
            leavingTile.canSetOn = true;
			leavingTile.attackable = true;

			transform.position = targetPos;

			if (targetPos.x >= 0) {

				SC_Tile posTile = tileManager.GetTileAt (targetPos); //SC_GameManager.GetInstance().GetTileAt((int)targetPos.x, (int)targetPos.y);
                posTile.constructable = false;
                posTile.canSetOn = false;
				posTile.attackable = false;

            } else {

				Destroy (gameObject);

            }

        }

	}

	public void DestroyConvoy() {

		tileManager.GetTileAt (gameObject).constructable = true;
        //SC_GameManager.GetInstance().GetTileAt((int)transform.position.x, (int)transform.position.y).constructable = true;

		SC_Qin.ChangeEnergy (50);

		Destroy (gameObject);

	}

}
