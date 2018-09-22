using UnityEngine;

public class SC_Workshop : SC_Construction {

	public void SelectWorkshop() {

        gameManager.CurrentWorkshopPos = transform.position;

        uiManager.DisplayWorkshopPanel();

	}

}
