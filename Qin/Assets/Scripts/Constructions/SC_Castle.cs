using UnityEngine;

public class SC_Castle : SC_Construction {

    public static int castlesNbr;

    public string CastleType { get; set; }

    protected override void Start () {

        base.Start();

        castlesNbr++;

    }

    public void SetCastle (string type) {

        SC_Player.localPlayer.CmdChangeCastleType(gameObject, type, Random.Range(0, Resources.LoadAll<Sprite>("Sprites/Tiles/" + type).Length));

    }

    public void SetCastle (string type, int sprite) {

        CastleType = type;

        if(SC_Player.localPlayer.Qin)
            Setup();

        foreach (SC_Tile t in tileManager.changingTiles) {

            if (t.Region == Tile.Region) {

                t.GetComponent<SC_Tile>().infos.type = CastleType;

                t.GetComponent<SC_Tile>().infos.sprite = sprite;

            }

        }

    }

    public void Setup() {

        Name = CastleType + " Castle";

        GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Sprites/Constructions/Castles/" + CastleType);

    }

    public override void DestroyConstruction () {

        base.DestroyConstruction();

        SC_Tile_Manager.constructableRegions[Tile.Region] = false;

        foreach (SC_Tile t in tileManager.regions[Tile.Region])
            t.Ruin?.DestroyRuin();

        castlesNbr--;

        if (castlesNbr < 1)
            uiManager.ShowVictory(false);

    }

}
