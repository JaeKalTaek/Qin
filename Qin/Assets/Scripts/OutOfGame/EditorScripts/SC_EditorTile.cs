using System.Collections.Generic;
using UnityEngine;

public class SC_EditorTile : MonoBehaviour {

	[Header("Editor Tile Variables")]
    [Tooltip("Type of this tile")]
    public TileType tileType;

    [Header("Construction on this tile")]
    [Tooltip("Type of construction on this tile")]
    public ConstructionType construction;    

    [Header("Character on this tile")]
    [Tooltip("Type of Soldier on this tile")]
    public SoldierType soldier;
    public SoldierType PrevSoldier { get; set; }

    [Tooltip("Is Qin on this tile ?")]
    public bool Qin;
    public bool PrevQin { get; set; }

    [Tooltip("Type of Hero on this tile")]
    public HeroType Hero;
    public HeroType PrevHero { get; set; }    

    public Sprite CharacterSprite {

        get { return transform.GetChild(0).GetComponent<SpriteRenderer>().sprite; }

        set { transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = value; }

    }

    public Sprite ConstructionSprite {

        get { return transform.GetChild(1).GetComponent<SpriteRenderer>().sprite; }

        set { transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = value; }

    }

    public static SC_EditorTile currentQinTile;

    public static List<HeroTile> heroesOnTiles = new List<HeroTile>();

    public static SC_EditorTile GetHeroTile (HeroType h) {

        SC_EditorTile tile = null;

        foreach (HeroTile hT in heroesOnTiles)
            if (hT.hero == h)
                tile = hT.tile;

        return tile;

    }

    public enum TileType {

        Plain, Forest, Mountain, Palace

    }

    public enum ConstructionType {

        None, Village, Workshop, Bastion, Wall, Ruin

    }

    public enum SoldierType {

        None, Adept, Archer, Builder, Cavalier, Hermit, Lancer, Monk, Scout, Swordsman, Warrior

    }

    public enum HeroType {

        None, Fei_Xue, Mulang_Ji, Sun_Shangxiang, Xing_Kong, Yu_Shu_Lien, Zhang_Fei

    }

    public struct HeroTile {

        public HeroType hero;

        public SC_EditorTile tile;

        public HeroTile (HeroType h, SC_EditorTile t) {

            hero = h;

            tile = t;

        }

    }

}