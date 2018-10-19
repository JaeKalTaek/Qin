using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SC_Global {

    public enum TDisplay { None, Movement, Attack, Construct, Sacrifice, PreviewAttack, PumpRange, PreviewMovement }

    public enum ShiFuMi { Rock, Paper, Scissors, Special }

    public enum Actions { Attack, Inventory, Wait, Build, Sacrifice, Destroy, EndTurn, Concede, Options, Cancel }

    //public enum CastleType { None = -1, Desert, Forest, River, Mountain, Snow, Plain }

    public static Vector3 WorldMousePos { get { return Camera.main.ScreenToWorldPoint(Input.mousePosition); } }

    [Serializable]
    public struct SC_CombatModifiers {

        [Header("Combat Modifiers")]
        [Tooltip("Strength Modifier")]
        public int strength;

        [Tooltip("Chi Modifier")]
        public int chi;

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
