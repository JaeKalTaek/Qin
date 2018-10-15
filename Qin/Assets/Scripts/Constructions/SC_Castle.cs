using UnityEngine;

public class SC_Castle : SC_Construction {

    public static int castlesNbr;

    public string CastleType { get; set; }

    protected override void Start () {

        base.Start();

        castlesNbr++;

    }

    public void SetCastle(string type) {

        CastleType = type;

        GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Sprites/Castles/" + type);

        foreach(SC_Tile t in tileManager.changingTiles)
            if(t.Region == Tile.Region)
                SC_Player.localPlayer.CmdChangeTileType(t.gameObject, type, Random.Range(0, Resources.LoadAll<Sprite>("Sprites/Tiles/" + type).Length));

    }

    public override void DestroyConstruction () {

        base.DestroyConstruction();

        castlesNbr--;

        if (castlesNbr < 1)
            uiManager.ShowVictory(false);

    }

}
