public class SC_Wall : SC_Bastion {

	protected override void OnMouseDown() {

		base.OnMouseDown ();

		SC_Tile under = tileManager.GetTileAt (gameObject);

		if (under.displayConstructable && ( ((SC_Qin.GetEnergy() - 100) > 0) || gameManager.IsBastion()))
			gameManager.ConstructAt(under);

	}

}
