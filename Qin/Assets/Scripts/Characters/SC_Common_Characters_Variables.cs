﻿using System;
using UnityEngine;

public class SC_Common_Characters_Variables : MonoBehaviour {

    [Header("Variables common to all characters")]
    [Tooltip("Damage is multiplied by this amount when landing a critical hit")]
    public float critMultiplier;

    [Tooltip("Amount of critical jauge needed to trigger a critical hit")]
    public int critTrigger;

    [Tooltip("Percentage of damage reduced when a character dodges")]
    public float dodgeReductionPercentage;

    [Tooltip("Amount of dodge jauge needed to trigger a dodge")]
    public int dodgeTrigger;

    [Tooltip("Damage is divided by this amount if this is a counter-attack")]
    public float counterFactor;

    [Tooltip("Damage is multiplied by this amount if the character is winning the ShiFuMi")]
    public float shiFuMiAvantage;

    [Tooltip("Damage is divided by this amount if the character is losing the ShiFuMi")]
    public float shiFuMiDisavantage;

    [Header("Variables common to all heroes")]
    [Tooltip("Health regenerated by a hero at the start of a turn if he's on a village")]
    public int villageRegen;

    [Tooltip("Amount of health kept by a hero who was saved")]
    public int savedHealthAmount;

    [Tooltip("Amount of health under which a hero becomes berserk")]
    public int berserkTriggerHealth;

    [Tooltip("Amount of relation value required between two heroes for one to go berserk when the other dies")]
    public int berserkTriggerRelation;

    [Tooltip("Damage are multiplied by that amount when the hero is berserker")]
    public float berserkDamageMultiplier;

    [Tooltip("Attack/Defense boosts added for each corresponding relation value with another nearby hero")]
    public RelationBoostValues relationBoostValues;

    [Tooltip("Amount of relation value required between two heroes for one to save the other when he's about to die")]
    public int saveTriggerRelation;

    [Tooltip("Amount of relation value gained by two heroes when one of them kills a unit while nearby the other")]
    public int killRelationValue;

    [Serializable]
    public struct RelationBoostValues {

        public int[] relations;
        public float[] values;

    }

    private void OnValidate () {

        Array.Resize(ref relationBoostValues.values, relationBoostValues.relations.Length);

    }

}
