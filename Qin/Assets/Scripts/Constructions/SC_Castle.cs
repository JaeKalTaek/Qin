using UnityEngine;

public class SC_Castle : SC_Construction {

    public string CastleType { get; set; }

    public static bool[] castles;

    protected override void Start () {

        base.Start();

        castles[Tile.Region] = true;

        transform.parent = uiManager.castlesT;

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

        foreach (SC_Tile t in tileManager.regions[Tile.Region])
            t.Ruin?.DestroyRuin();

        castles[Tile.Region] = false;

        if (SC_Demon.demons[Tile.Region].Alive != -1)
            SC_Demon.demons[Tile.Region].DestroyCharacter();

        bool victory = true;

        foreach (bool b in castles)
            if (b)
                victory = false;

        if (victory)
            uiManager.ShowVictory(false);

    }

}
