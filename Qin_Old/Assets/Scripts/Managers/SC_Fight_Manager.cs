using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_Fight_Manager : MonoBehaviour {

    public bool RangedAttack { get; set; }

    SC_Game_Manager gameManager;

    SC_UI_Manager uiManager;

    public SC_Tile_Manager TileManager { get; set; }

    SC_Common_Heroes_Variables CommonHeroesVariables { get { return gameManager.commonHeroesVariables; } }

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

            if (attacked) {

                bool killed = attacked.Hit(CalcDamages(attacker, attacked, false), false);
                SetCritDodge(attacker, attacked);

                if (attacker.IsHero && killed)
                    IncreaseRelationships(attacker.Hero);

                if ((RangedAttack && attacked.GetActiveWeapon().ranged) || (!RangedAttack && !attacked.GetActiveWeapon().IsBow())) {

                    killed = attacker.Hit(CalcDamages(attacked, attacker, true), false);
                    SetCritDodge(attacked, attacker);

                    if (attacked.IsHero && killed)
                        IncreaseRelationships(attacked.Hero);

                }

                //uiManager.TryRefreshInfos(attacked.gameObject, attacked.GetType());

            } else if (targetConstruction) {

                targetConstruction.health -= attacker.GetActiveWeapon().weaponOrQi ? attacker.strength : attacker.qi;

                targetConstruction.lifebar.UpdateGraph(targetConstruction.health, targetConstruction.maxHealth);

                if (targetConstruction.health <= 0)
                    targetConstruction.DestroyConstruction();
                else
                    uiManager.TryRefreshInfos(targetConstruction.gameObject, typeof(SC_Construction));

            } else if (attacker.AttackTarget.Qin) {

                SC_Qin.ChangeEnergy(-(attacker.GetActiveWeapon().weaponOrQi ? attacker.strength : attacker.qi));

            }

            //uiManager.TryRefreshInfos(attacker.gameObject, attacker.GetType());            

        }

        if (attacker.IsHero)
            attacker.Hero.berserkTurn = attacker.Hero.berserk;

        SC_Character.attackingCharacter = null;

    }

    void SetCritDodge (SC_Character attacker, SC_Character attacked) {

        attacker.CriticalHit = (attacker.CriticalHit == 0) ? attacker.technique : (attacker.CriticalHit - 1);
        attacked.DodgeHit = (attacked.DodgeHit == 0) ? attacked.speed : (attacked.DodgeHit - 1);

    }

    public int CalcDamages (SC_Character attacker, SC_Character attacked, bool counter) {

        int damages = attacker.GetActiveWeapon().weaponOrQi ? attacker.strength : attacker.qi;

        damages = Mathf.CeilToInt(damages * attacker.GetActiveWeapon().ShiFuMi(attacked.GetActiveWeapon()));

        if (attacker.IsHero)
            damages += Mathf.CeilToInt(damages * RelationBoost(attacker.Hero));

        if (attacker.IsHero && attacked.IsHero && !attacked.coalition)
            damages = Mathf.CeilToInt(damages * RelationMalus(attacker.Hero, attacked.Hero));

        if (attacker.CriticalHit == 0)
            damages *= 3;

        if (attacker.IsHero && attacker.Hero.berserk)
            damages = Mathf.CeilToInt(damages * CommonHeroesVariables.berserkDamageMultiplier);

        if (attacked.DodgeHit == 0)
            damages = 0;

        int boostedArmor = attacked.armor;
        int boostedResistance = attacked.resistance;

        if (attacked.IsHero) {

            float relationBoost = RelationBoost(attacked.Hero);
            boostedArmor += Mathf.CeilToInt(boostedArmor * relationBoost);
            boostedResistance += Mathf.CeilToInt(boostedResistance * relationBoost);

        }

        damages -= (attacker.GetActiveWeapon().weaponOrQi) ? boostedArmor : boostedResistance;

        if (counter)
            damages = Mathf.CeilToInt(damages / 2);

        return Mathf.Max(0, damages);

    }

    float RelationMalus (SC_Hero target, SC_Hero opponent) {

        int value;
        target.relationships.TryGetValue(opponent.characterName, out value);

        return 1 - RelationValue(value);

    }

    float RelationBoost (SC_Hero target) {

        float boost = 0;

        foreach (SC_Hero hero in TileManager.HeroesInRange(target)) {

            int value;
            target.relationships.TryGetValue(hero.characterName, out value);

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
                    toSave.relationships.TryGetValue(hero.characterName, out value);

                    int currentValue = -1;
                    if (saver)
                        toSave.relationships.TryGetValue(saver.characterName, out currentValue);

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

            killer.relationships[hero.characterName] += Mathf.CeilToInt(CommonHeroesVariables.killRelationValue / heroesInRange.Count);
            hero.relationships[killer.characterName] += Mathf.CeilToInt(CommonHeroesVariables.killRelationValue / heroesInRange.Count);

        }

    }    

}
