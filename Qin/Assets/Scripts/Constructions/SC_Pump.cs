using UnityEngine;

public class SC_Pump : SC_Construction {

    [Header("Pump Variables")]
    [Tooltip("Amount of Health drained to each hero in the range of that pump at the beginning of Qin's turn")]
    public int drainAmount;

    [Tooltip("Range in which the pump can drain energy")]
    public int range;

    [Tooltip("Heroes in range have their movement capability reduced by this amount")]
    public int slowAmount;

    delegate void Action (SC_Hero parameter);

    void PerformAction(Action action) {

        foreach (SC_Hero hero in FindObjectsOfType<SC_Hero>()) {

            if (tileManager.TileDistance(transform.position, hero.transform.position) <= range)
                action(hero);

        }

    }

    protected override void Start () {

        base.Start();

        Lifebar = Instantiate(Resources.Load<GameObject>("Prefabs/Characters/Components/P_Lifebar"), transform).GetComponent<SC_Lifebar>();
        Lifebar.transform.position += new Vector3(0, -.44f, 0);

        PerformAction((SC_Hero hero) => {

            TrySlowHero(hero);

            uiManager.TryRefreshInfos(hero.gameObject, typeof(SC_Hero));

        });

    }

    public void Drain() {

        PerformAction((SC_Hero hero) => {

            SC_Qin.ChangeEnergy(Mathf.Min(hero.Health, drainAmount));

            hero.Hit(drainAmount, false);

        });

    }

    public override void DestroyConstruction () {

        gameObject.SetActive(false);

        PerformAction((SC_Hero hero) => {

            if (hero.PumpSlow == slowAmount) {

                hero.Movement += slowAmount;

                hero.PumpSlow = 0;

                foreach (SC_Pump pump in FindObjectsOfType<SC_Pump>())
                    TrySlowHero(hero);

                uiManager.TryRefreshInfos(hero.gameObject, typeof(SC_Hero));

            }

        });

        base.DestroyConstruction();

    }

    public static void UpdateHeroSlow(SC_Hero hero) {

        int pumpSlow = 0;

        foreach (SC_Pump pump in FindObjectsOfType<SC_Pump>()) {

            if ((tileManager.TileDistance(hero.transform.position, pump.transform.position) <= pump.range) && (pumpSlow < pump.slowAmount))
                pumpSlow = pump.slowAmount;

        }

        if (pumpSlow != hero.PumpSlow)
            hero.Movement -= (pumpSlow - hero.PumpSlow);

    }

    void TrySlowHero(SC_Hero hero) {

        if (hero.PumpSlow < slowAmount) {

            hero.Movement -= slowAmount - hero.PumpSlow;

            hero.PumpSlow = slowAmount;

        }

    }

}
