﻿using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SC_Global {

    public enum TDisplay { None, Movement, Attack, Construct, Sacrifice, PreviewAttack, PumpRange, PreviewMovement }

    public enum ShiFuMi { Rock, Paper, Scissors, Special }

    public enum Actions { Attack, Inventory, Wait, Build, Sacrifice, Destroy, EndTurn, Concede, Options, Cancel }

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

    public static List<Actions> ActionsUpdate(Actions action, List<Actions> actions, bool add)
    {
        if(add)
            if(!actions.Contains(action))
                actions.Add(action);
        else
            if (actions.Contains(action))
                actions.Remove(action);

        return actions;
    }

}
