using UnityEngine;

public static class SC_ExtensionMethods {

	public static void SetPos(this Transform trans, Transform other) {

		trans.SetPos(other.position);

	}

	public static void SetPos(this Transform trans, Vector3 v3) {

		trans.SetPos (new Vector2 (v3.x, v3.y));

	}

	public static void SetPos(this Transform trans, Vector2 v2) {

		trans.position = new Vector3 (v2.x, v2.y, trans.position.z);

	}
		
}
