using UnityEngine;

public class SC_Demon : SC_BaseQinChara {

    //[Header("Demon Variables")]

    public override void DestroyCharacter () {

        base.DestroyCharacter();

        /*if (isServer)
            SC_Player.localPlayer.CmdDestroyGameObject(gameObject);*/

    }

}
