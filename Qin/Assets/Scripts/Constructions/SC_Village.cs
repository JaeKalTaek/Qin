﻿using UnityEngine;

public class SC_Village : SC_Construction {

	public static int number;

	protected override void Start() {

		base.Start ();

		number++;

	}

	public override void DestroyConstruction() {

		base.DestroyConstruction ();

		number--;

		//gameManager.SpawnConvoy (transform.position + new Vector3 (-1, 0, 0));

	}

}
