using UnityEngine;

public class SC_Castle : SC_Construction {

    public string CastleType { get; set; }

    public void SetCastle(string type) {

        CastleType = type;

        GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Sprites/Castles/" + type);

        foreach(SC_Tile t in tileManager.changingTiles) {

            if(t.Region == Tile.Region) {

                SC_Player.localPlayer.CmdChangeTileType(t.gameObject, type, Random.Range(0, Resources.LoadAll<Sprite>("Sprites/Tiles/" + type).Length));

                /*t.infos.type = type;

                t.infos.sprite = Random.Range(0, Resources.LoadAll<Sprite>("Sprites/Tiles/" + type).Length);*/

                /*t.infos = new SC_Tile.TileInfos(
                    type,
                    Random.Range(0, Resources.LoadAll<Sprite>("Sprites/Tiles/" + type).Length),
                    t.infos.riverSprite,
                    t.infos.region,
                    t.infos.borders
                );*/

            }

        }

    }

}
