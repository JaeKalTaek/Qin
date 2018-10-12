using UnityEngine;

public static class SC_ExtensionMethods {

	public static void SetPos(this Transform trans, Transform other) {

		trans.SetPos(other.position);

	}

	public static void SetPos(this Transform trans, Vector3 v3) {

		trans.SetPos (new Vector2 (v3.x, v3.y));

	}

	public static void SetPos(this Transform trans, Vector2 v2) {

		trans.position = new Vector3 (Mathf.Round(v2.x), Mathf.Round(v2.y), trans.position.z);

	}
		
    public static void ShowHideInfos(this MonoBehaviour MB) {

        SC_UI_Manager.Instance.ShowInfos(MB.gameObject, MB.GetType());

    }

    public static float F (this float f) {

        return f / SC_Game_Manager.TileSize;

    }

    public static int I (this float f) {

        return Mathf.RoundToInt(f.F());

    }

}
