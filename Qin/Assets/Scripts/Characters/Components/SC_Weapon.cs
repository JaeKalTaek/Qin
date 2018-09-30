using UnityEngine;
using static SC_Global;

public class SC_Weapon : MonoBehaviour {

    public bool weaponOrQi, ranged;

    public string weaponName;

    public ShiFuMi value;

    public bool IsBow { get { return value == ShiFuMi.Special; } }

    public float ShiFuMiModifier(SC_Weapon opponent) {

		if ((weaponOrQi == opponent.weaponOrQi) && (value != ShiFuMi.Special) && (opponent.value != ShiFuMi.Special) && (value != opponent.value))
            return ((value == ShiFuMi.Rock && opponent.value == ShiFuMi.Scissors) ||
                (value == ShiFuMi.Paper && opponent.value == ShiFuMi.Rock) ||
                (value == ShiFuMi.Scissors && opponent.value == ShiFuMi.Paper)) ?
                SC_Game_Manager.Instance.CommonCharactersVariables.shiFuMiAvantage : SC_Game_Manager.Instance.CommonCharactersVariables.shiFuMiDisavantage;
        else
            return 1;

    }
         
}
