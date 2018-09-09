using UnityEngine;
using UnityEngine.Networking;

public class SC_Lifebar : NetworkBehaviour {

	Transform health, health2;
	GameObject lifebar;

    void Start() {

		lifebar = transform.GetChild(0).gameObject;
		health = lifebar.transform.GetChild(0);
		health2 = lifebar.transform.GetChild(1);

    }

    public void UpdateGraph(int h, int maxH) {

        float percentage = h / maxH;

        health.localScale = new Vector3(percentage, 1, 1);
        health2.localScale = new Vector3(percentage, 1, 1);

        Vector3 pos = health.localPosition;
        float posX = -0.6f + (0.6f * percentage);
        health.localPosition = new Vector3(posX, pos.y, pos.z);
        health2.localPosition = new Vector3(posX, pos.y, pos.z);

    }

	public void Toggle() {

		lifebar.SetActive(!lifebar.activeSelf);

	}

}
