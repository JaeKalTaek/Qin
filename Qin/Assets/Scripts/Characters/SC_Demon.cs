using UnityEngine;

public class SC_Demon : SC_Character {

    [Header("Demon Variables")]
    [Tooltip("Weapon of this demon")]
    public SC_Weapon weapon;

    public override void OnStartClient () {

        base.OnStartClient();

        weapon = loadedCharacter.Demon.weapon;

    }

    public override void TryCheckMovements () {

        if (CanMove) {

            SC_Player.localPlayer.CmdSetQinTurnStarting(false);

            base.TryCheckMovements();

        }

    }

    public override bool Hit (int damages, bool saving) {

        base.Hit(damages, saving);

        if (Health <= 0)
            DestroyCharacter();
        else
            UpdateHealth();

        return (Health <= 0);

    }

    public override void DestroyCharacter () {

        base.DestroyCharacter();

        /*if (isServer)
            SC_Player.localPlayer.CmdDestroyGameObject(gameObject);*/

    }

    public override Vector2 GetRange () {

        return weapon.Range;

    }

}
