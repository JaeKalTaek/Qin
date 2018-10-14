using System;
using UnityEngine;

[Serializable]
public class SC_Global {

    public enum TDisplay { None, Movement, Attack, Construct, Sacrifice, PreviewAttack, PumpRange, PreviewMovement }

    public enum ShiFuMi { Rock, Paper, Scissors, Special }

    [Serializable]
    public struct CombatModifiers {

        [Header("Combat Modifiers")]
        [Tooltip("Strength Modifier")]
        public int strength;

        [Tooltip("Qi Modifier")]
        public int qi;

        [Tooltip("Armor Modifier")]
        public int armor;

        [Tooltip("resistance Modifier")]
        public int resistance;

        [Tooltip("Technique Modifier")]
        public int technique;

        [Tooltip("Reflexes Modifier")]
        public int reflexes;

    }

}
