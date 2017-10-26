using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_Weapon : MonoBehaviour {

    public bool weaponOrQi, ranged;
    public string weaponName;
    public int value;

    public float ShiFuMi(SC_Weapon opponent) {

		if ((weaponOrQi == opponent.weaponOrQi) && (value != -1) && (opponent.value != -1) && (value != opponent.value)) {
			
            switch (value) {

                case 0:
					return (opponent.value == 1) ? 1.25f : 0.75f;

                case 1:
					return (opponent.value == 2) ? 1.25f : 0.75f;

                case 2:
					return (opponent.value == 0) ? 1.25f : 0.75f;

                default:
                    return -1;

            }

        } else {

            return 1;

        }

    }

	public bool IsBow() {

		return (value == -1);

	}
         
}
