
using UnityEngine;
using System.Collections;

public class SC_Camera : MonoBehaviour {
		
	public float moveSpeed, zoomSpeed;

	public void Setup(SC_GameManager gm) {

		transform.position = new Vector3 ((gm.SizeMapX - 1) / 2, (gm.SizeMapY - 1) / 2, -16);

	}

    void Update() {

		transform.position += transform.up * Time.deltaTime * Input.GetAxis ("Vertical") * moveSpeed;

		transform.position += transform.right * Time.deltaTime * Input.GetAxis ("Horizontal") * moveSpeed;

		transform.position += transform.forward * Time.deltaTime * Input.GetAxis ("Mouse ScrollWheel") * zoomSpeed;

    }

}