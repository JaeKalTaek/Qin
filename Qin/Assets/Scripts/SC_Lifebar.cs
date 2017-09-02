using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_Lifebar : MonoBehaviour {

    GameObject health, health2, Graphs;
    float maxSize;

    void Start() {

        health = transform.Find("Graphs").Find("lifebar_health").gameObject;
        health2 = transform.Find("Graphs").Find("lifebar_health_2").gameObject;
        Graphs = transform.Find("Graphs").gameObject;
        Graphs.SetActive(false);

    }

    public void UpdateGraph(int h, int maxH) {

        float percentage = (float)h / (float)maxH;

        health.transform.localScale = new Vector3(percentage, 1, 1);
        health2.transform.localScale = new Vector3(percentage, 1, 1);

        Vector3 pos = health.transform.localPosition;
        float posX = -0.6f + (0.6f * percentage);
        health.transform.localPosition = new Vector3(posX, pos.y, pos.z);
        health2.transform.localPosition = new Vector3(posX, pos.y, pos.z);

    }

    public void Show() {

        if (transform.parent.parent.name.Contains("Zhang")) {

            SC_Hero z = GameObject.Find("P_Zhang_Fei(Clone)").GetComponent<SC_Hero>();
            UpdateGraph(z.health, z.maxHealth);

        }

        Graphs.SetActive(true);

    }

    public void Hide() {

        Graphs.SetActive(false);

    }

}
