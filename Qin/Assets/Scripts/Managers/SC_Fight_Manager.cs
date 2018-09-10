using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_Fight_Manager : MonoBehaviour {

    public bool RangedAttack { get; set; }

    SC_Game_Manager gameManager;

    SC_UI_Manager uiManager;

    public SC_Tile_Manager TileManager { get; set; }

    SC_Common_Characters_Variables CommonHeroesVariables { get { return gameManager.commonCharactersVariables; } }

    public static SC_Fight_Manager Instance;

    void Awake () {

        Instance = this;

    }

    void Start () {

        gameManager = SC_Game_Manager.Instance;

        uiManager = SC_UI_Manager.Instance;

    }

    public void Attack () {

        uiManager.HideWeapons();
        uiManager.cancelMovementButton.SetActive(false);
        uiManager.cancelAttackButton.SetActive(false);

        SC_Character attacker = SC_Character.attackingCharacter;

        attacker.Tire();

        if (!attacker.AttackTarget.Empty) {

            SC_Character attacked = attacker.AttackTarget.Character;
            SC_Construction targetConstruction = attacker.AttackTarget.Construction;
            SC_Construction currentConstruction = TileManager.GetTileAt(attacker.gameObject).Construction;

            if (attacked) {

                CharacterAttack(attacker, attacked, targetConstruction, false);

                if ((RangedAttack && attacked.GetActiveWeapon().ranged) || (!RangedAttack && !attacked.GetActiveWeapon().IsBow))
                    CharacterAttack(attacked, attacker, currentConstruction, true);

            } else if (targetConstruction) {

                HitConstruction(attacker, targetConstruction, false);

            } else if (attacker.AttackTarget.Qin) {

                SC_Qin.ChangeEnergy(-(attacker.GetActiveWeapon().weaponOrQi ? attacker.strength : attacker.qi));

            }        

        }

        if (attacker.IsHero)
            attacker.Hero.BerserkTurn = attacker.Hero.Berserk;

        SC_Character.attackingCharacter = null;

    }

    void CharacterAttack(SC_Character attacker, SC_Character attacked, SC_Construction attackedConstru, bool counter) {

        bool killed = false;

        if (attackedConstru && attackedConstru as SC_Bastion)
            HitConstruction(attacker, attackedConstru, counter);
        else
            killed = attacked.Hit(CalcDamages(attacker, attacked, counter), false);

        attacker.CriticalHit = (attacker.CriticalHit >= attacker.technique) ? 0 : (attacker.CriticalHit + 1);
        attacked.DodgeHit = (attacked.DodgeHit >= attacked.speed) ? 0 : (attacked.DodgeHit + 1);

        if (attacker.IsHero && killed)
            IncreaseRelationships(attacker.Hero);

    }

    void HitConstruction(SC_Character attacker, SC_Construction construction, bool counter) {

        construction.Health -= Mathf.CeilToInt((attacker.GetActiveWeapon().weaponOrQi ? attacker.strength : attacker.qi) / (counter ? CommonHeroesVariables.counterFactor : 1));

        construction.Lifebar.UpdateGraph(construction.Health, construction.maxHealth);

        if (construction.Health <= 0)
            construction.DestroyConstruction();
        else
            uiManager.TryRefreshInfos(construction.gameObject, typeof(SC_Construction));

    }

    public int CalcDamages (SC_Character attacker, SC_Character attacked, bool counter) {

        int damages = attacker.GetActiveWeapon().weaponOrQi ? attacker.strength : attacker.qi;

        damages = Mathf.CeilToInt(damages * attacker.GetActiveWeapon().ShiFuMiModifier(attacked.GetActiveWeapon()));

        if (attacker.IsHero)
            damages += Mathf.CeilToInt(damages * RelationBoost(attacker.Hero));

        if (attacker.IsHero && attacked.IsHero && !attacked.coalition)
            damages = Mathf.CeilToInt(damages * RelationMalus(attacker.Hero, attacked.Hero));

        if (attacker.CriticalHit == attacker.technique)
            damages = Mathf.CeilToInt(damages * CommonHeroesVariables.critMultiplier);

        if (attacker.IsHero && attacker.Hero.Berserk)
            damages = Mathf.CeilToInt(damages * CommonHeroesVariables.berserkDamageMultiplier);

        if (attacked.DodgeHit == attacked.speed)
            damages = Mathf.FloorToInt(damages * ((100 - CommonHeroesVariables.dodgeReductionPercentage) / 100));

        int boostedArmor = attacked.armor;
        int boostedResistance = attacked.resistance;

        if (attacked.IsHero) {

            float relationBoost = RelationBoost(attacked.Hero);
            boostedArmor += Mathf.CeilToInt(boostedArmor * relationBoost);
            boostedResistance += Mathf.CeilToInt(boostedResistance * relationBoost);

        }

        damages -= (attacker.GetActiveWeapon().weaponOrQi) ? boostedArmor : boostedResistance;

        if (counter)
            damages = Mathf.CeilToInt(damages / CommonHeroesVariables.counterFactor);

        return Mathf.Max(0, damages);

    }

    float RelationMalus (SC_Hero target, SC_Hero opponent) {

        int value;
        target.Relationships.TryGetValue(opponent.characterName, out value);

        return 1 - RelationValue(value);

    }

    float RelationBoost (SC_Hero target) {

        float boost = 0;

        foreach (SC_Hero hero in TileManager.HeroesInRange(target)) {

            int value;
            target.Relationships.TryGetValue(hero.characterName, out value);

            boost += RelationValue(value);

        }

        return boost;

    }

    float RelationValue (int value) {

        float v = 0;

        for (int i = 0; i < CommonHeroesVariables.relationBoostValues.relations.Length; i++)
            if (value >= CommonHeroesVariables.relationBoostValues.relations[i])
                v = CommonHeroesVariables.relationBoostValues.values[i];

        return v;

    }

    public SC_Hero CheckHeroSaved (SC_Hero toSave, bool alreadySaved) {

        SC_Hero saver = null;

        if (!alreadySaved) {

            foreach (SC_Hero hero in FindObjectsOfType<SC_Hero>()) {

                if (hero.coalition) {

                    int value = 0;
                    toSave.Relationships.TryGetValue(hero.characterName, out value);

                    int currentValue = -1;
                    if (saver)
                        toSave.Relationships.TryGetValue(saver.characterName, out currentValue);

                    if ((value >= CommonHeroesVariables.saveTriggerRelation) && (value > currentValue))
                        saver = hero;

                }

            }

            SC_Tile nearestTile = TileManager.NearestTile(toSave);

            if (saver && nearestTile) {

                TileManager.GetTileAt(saver.gameObject).Character = null;

                saver.transform.SetPos(TileManager.NearestTile(toSave).transform);

                TileManager.GetTileAt(saver.gameObject).Character = saver;

            } else {

                saver = null;

            }

        }

        return saver;

    }

    void IncreaseRelationships (SC_Hero killer) {

        List<SC_Hero> heroesInRange = TileManager.HeroesInRange(killer);

        foreach (SC_Hero hero in heroesInRange) {

            killer.Relationships[hero.characterName] += Mathf.CeilToInt(CommonHeroesVariables.killRelationValue / heroesInRange.Count);
            hero.Relationships[killer.characterName] += Mathf.CeilToInt(CommonHeroesVariables.killRelationValue / heroesInRange.Count);

        }

    }    

}
