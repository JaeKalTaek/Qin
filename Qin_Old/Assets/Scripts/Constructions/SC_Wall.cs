public class SC_Wall : SC_Bastion {

    protected override void Start() {

        base.Start();

        if(SC_Player.localPlayer.IsQin())
            tileManager.DisplayConstructableTiles();

    }

}
