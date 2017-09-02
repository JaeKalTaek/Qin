using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_Convoy : MonoBehaviour {

	protected virtual void Start () {

        SC_Tile under = SC_GameManager.GetInstance().GetTileAt((int)transform.position.x, (int)transform.position.y);

        under.constructable = false;
        under.attackable = false;

	}

	public void OnMouseDown() {

		SC_Tile under = SC_GameManager.GetInstance().GetTileAt((int)transform.position.x, (int)transform.position.y);

		if (under.displayMovement)
			SC_Character.GetCharacterToMove().MoveTo ((int)transform.position.x, (int)transform.position.y);

	}

	public void MoveConvoy() {

		Vector3 targetPos = (transform.position + new Vector3 (-1, 0, 0));

		if(SC_GameManager.GetInstance().GetTileAt((int)targetPos.x, (int)targetPos.y).IsEmpty()) {

			int x = (int)transform.position.x;
			int y = (int)transform.position.y;

            SC_Tile leavingTile = SC_GameManager.GetInstance().GetTileAt(x, y);
            leavingTile.constructable = !leavingTile.isPalace();
            leavingTile.canSetOn = true;
			leavingTile.attackable = true;

			transform.position = targetPos;

			if (targetPos.x >= 0) {

                SC_Tile posTile = SC_GameManager.GetInstance().GetTileAt((int)targetPos.x, (int)targetPos.y);
                posTile.constructable = false;
                posTile.canSetOn = false;
				posTile.attackable = false;

            } else {

				Destroy (gameObject);

            }

        }

	}

	public void DestroyConvoy() {

        SC_GameManager.GetInstance().GetTileAt((int)transform.position.x, (int)transform.position.y).constructable = true;

		SC_Qin.IncreaseEnergy (50);

		Destroy (gameObject);

	}

}
