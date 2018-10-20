using UnityEngine;

public class SC_Demon : SC_BaseQinChara {

    [Header("Demon Variables")]
    [Tooltip("Range of the aura of this demon")]
    public int auraRange;

    public override void DestroyCharacter () {

        base.DestroyCharacter();

        /*if (isServer)
            SC_Player.localPlayer.CmdDestroyGameObject(gameObject);*/

    }

}
