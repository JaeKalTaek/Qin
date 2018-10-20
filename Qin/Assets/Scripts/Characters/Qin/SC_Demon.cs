using UnityEngine;

public class SC_Demon : SC_BaseQinChara {

    [Header("Demon Variables")]
    [Tooltip("Range of the aura of this demon")]
    public int auraRange;

    [Tooltip("Modifiers applied by the aura of this demon")]
    public SC_Global.SC_CombatModifiers auraModifiers;

    [Tooltip("Number of turns for this demon to respawn at its castle after being killed")]
    public int respawnTime;

    delegate void Action (SC_Character parameter);

    void PerformAction (Action action) {

        foreach (SC_Character chara in FindObjectsOfType<SC_Character>()) {

            if (SC_Tile_Manager.TileDistance(transform.position, chara.transform.position) <= auraRange)
                action(chara);

        }

    }

    protected override void Start () {

        base.Start();

        PerformAction((SC_Character chara) => {

            chara.TryAddAura(characterName, auraModifiers);

            uiManager.TryRefreshInfos(chara.gameObject, chara.GetType());

        });

    }

    public override void DestroyCharacter () {

        PerformAction((SC_Character chara) => {

            chara.DemonAuras.Remove(new DemonAura(characterName, auraModifiers));

            uiManager.TryRefreshInfos(chara.gameObject, chara.GetType());

        });

        base.DestroyCharacter();

        /*if (isServer)
            SC_Player.localPlayer.CmdDestroyGameObject(gameObject);*/

    }

}
