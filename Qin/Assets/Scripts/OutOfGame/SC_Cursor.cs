﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_Cursor : MonoBehaviour {

	// Use this for initialization
	void Start () {
		


	}
	
	// Update is called once per frame
	void Update () {

        transform.SetPos(new Vector3(Mathf.RoundToInt(Input.mousePosition.x), Mathf.RoundToInt(Input.mousePosition.y), 0));

        if (Input.GetAxis("Fire1") != 0)
            SC_Tile_Manager.Instance?.GetTileAt(transform.position).CursorClick();

    }

}
