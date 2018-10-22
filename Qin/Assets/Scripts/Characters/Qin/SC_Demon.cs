using UnityEngine;
using static SC_Global;

public class SC_Demon : SC_BaseQinChara {

    [Header("Demon Variables")]
    [Tooltip("Range of the aura of this demon")]
    public int auraRange;

    [Tooltip("Modifiers applied by the aura of this demon")]
    public SC_CombatModifiers auraModifiers;

    [Tooltip("Number of turns for this demon to respawn at its castle after being killed")]
    public int respawnTime;

    public int Alive { get; set; }

    SC_Tile spawnTile;

    public static SC_Demon[] demons;

    public override void OnStartClient () {

        base.OnStartClient();

        auraRange = loadedCharacter.Demon.auraRange;

        auraModifiers = loadedCharacter.Demon.auraModifiers;

        respawnTime = loadedCharacter.Demon.respawnTime;

        Alive = -1;

    }

    protected override void Start () {

        base.Start();

        AddAura(true);

        spawnTile = Tile;

        demons[Tile.Region] = this;

    }

    delegate void Action (SC_Tile tile);

    void PerformAction (Action action, SC_Tile center = null) {

        foreach (SC_Tile tile in tileManager.GetRange(center ? center.transform.position : transform.position, auraRange))
            action(tile);

    }

    public void AddAura(bool refreshInfos) {

        PerformAction((SC_Tile tile) => {

            tile.TryAddAura(characterName, auraModifiers);

            if (refreshInfos && tile.Character)
                uiManager.TryRefreshInfos(tile.Character.gameObject, tile.Character.GetType());

        });

    }

    public void RemoveAura(bool refreshInfos, SC_Tile center = null) {

        PerformAction((SC_Tile tile) => {

            tile.DemonAuras.Remove(new DemonAura(characterName, auraModifiers));

            if (refreshInfos && tile.Character)
                uiManager.TryRefreshInfos(tile.Character.gameObject, tile.Character.GetType());

        }, center);

    }
    
    public void TryRespawn() {

        Alive++;

        if (Alive > respawnTime) {

            SC_Tile respawnTile = spawnTile.CanCharacterSetOn(this) ? spawnTile : tileManager.GetUnoccupiedNeighbor(this);

            if (respawnTile) {

                Alive = -1;

                Health = maxHealth;

                transform.SetPos(respawnTile.transform);

                respawnTile.Character = this;

                LastPos = respawnTile;

                AddAura(true);

                gameObject.SetActive(true);

            }

        }

    }

    public override void DestroyCharacter () {

        RemoveAura(true);

        base.DestroyCharacter();

        if (isServer && !SC_Castle.castles[spawnTile.Region]) {

            Alive = -1;

            SC_Player.localPlayer.CmdDestroyGameObject(gameObject);

        } else {

            Alive = 0;

            gameObject.SetActive(false);

        }

    }

}
