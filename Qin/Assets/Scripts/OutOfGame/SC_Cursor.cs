using UnityEngine;
using UnityEngine.Networking;

public class SC_Cursor : NetworkBehaviour {

    [Header("Cursor variables")]
    [Tooltip("Distance needed for the mouse to move to make the mouse cursor visible again, and snap the cursor to it")]
    public float mouseThreshold;

    [Tooltip("Delay between two movements of the cursor when using keys")]
    public float inputsMoveDelay;
    float inputsMoveTimer;

    Vector3 oldMousePos, newMousePos;

    bool cameraMoved;

    SC_Camera cam;

    void Start() {

        cam = FindObjectOfType<SC_Camera>();

        oldMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        newMousePos = oldMousePos;

        inputsMoveTimer = 0;

        SC_Tile_Manager.Instance.GetTileAt(transform.position).CursorOn = true;

    }

    void Update () {

        #region Cursor Movement
        inputsMoveTimer -= Time.deltaTime;

        Vector3 oldPos = transform.position;

        Vector3 newPos = -Vector3.one;

        oldMousePos = newMousePos;

        newMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        if ((Input.GetButton("Horizontal") || Input.GetButton("Vertical")) && (inputsMoveTimer <= 0)) {

            inputsMoveTimer = inputsMoveDelay;

            Cursor.visible = false;
            
            newPos = transform.position + new Vector3(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"), 0);

        } else if (Cursor.visible || ((Vector3.Distance(oldMousePos, newMousePos) >= mouseThreshold) && !cameraMoved)) {

            Cursor.visible = true;

            newPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        }

        cameraMoved = false;

        if ((Mathf.RoundToInt(newPos.x) >= 0) && (Mathf.RoundToInt(newPos.y) >= 0) && (Mathf.RoundToInt(newPos.x) < SC_Tile_Manager.Instance.xSize) && (Mathf.RoundToInt(newPos.y) < SC_Tile_Manager.Instance.ySize))
            transform.SetPos(new Vector2(Mathf.Round(newPos.x), Mathf.Round(newPos.y)));

        if(oldPos != transform.position) {

            SC_Tile_Manager.Instance?.GetTileAt(oldPos)?.OnCursorExit();

            SC_Tile_Manager.Instance?.GetTileAt(transform.position)?.OnCursorEnter();

            float x2 = CamPos(true, true).x > 1 ? 1 : CamPos(true, false).x < 0 ? -1 : 0;

            float y2 = CamPos(false, true).y > 1 ? 1 : CamPos(false, false).y < 0 ? -1 : 0;

            Vector3 oldCamPos = cam.TargetPosition;

            cam.TargetPosition += new Vector3(x2, y2, 0);

            cameraMoved = oldCamPos != cam.TargetPosition;

        }
        #endregion

        #region Cursor Inputs
        if (Input.GetButtonDown("Fire1"))
            SC_Tile_Manager.Instance?.GetTileAt(transform.position)?.CursorClick();
        else if (Input.GetButtonDown("Fire2"))
            SC_Tile_Manager.Instance?.GetTileAt(transform.position)?.CursorSecondaryClick();
        #endregion
    }

    Vector3 CamPos(bool x, bool sign) {

        float f = sign ? .5f : -.5f;

        return Camera.main.WorldToViewportPoint(transform.position + new Vector3(x ? f : 0, x ? 0 : f, 0));

    }

}
