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

    void Start() {

        oldMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        newMousePos = oldMousePos;

        inputsMoveTimer = 0;

    }

    void Update () {

        #region Cursor Movement
        inputsMoveTimer -= Time.deltaTime;

        Vector3 newPos = Vector3.zero;

        oldMousePos = newMousePos;

        newMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        if (((Input.GetAxisRaw("Horizontal") != 0) || (Input.GetAxisRaw("Vertical") != 0)) && (inputsMoveTimer <= 0)) {

            inputsMoveTimer = inputsMoveDelay;

            Cursor.visible = false;
            
            newPos = transform.position + new Vector3(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"), 0);

        } else if (Cursor.visible || (Vector3.Distance(oldMousePos, newMousePos) >= mouseThreshold)) {

            //inputsMoveTimer = 0;

            Cursor.visible = true;

            newPos = Camera.main.ScreenToWorldPoint(Input.mousePosition) + new Vector3(.5f, .5f, 0);

        }

        int x = Mathf.CeilToInt(newPos.x);
        int y = Mathf.CeilToInt(newPos.y);

        if ((x > 0) && (y > 0) && (x < SC_Tile_Manager.Instance.xSize) && (y < SC_Tile_Manager.Instance.ySize))
            transform.SetPos(new Vector2(Mathf.Floor(newPos.x), Mathf.Floor(newPos.y)));
        #endregion

        #region Cursor Inputs
        if (Input.GetAxis("Fire1") != 0)
            SC_Tile_Manager.Instance?.GetTileAt(transform.position)?.CursorClick();
        else if (Input.GetAxis("Fire2") != 0)
            SC_Tile_Manager.Instance?.GetTileAt(transform.position)?.CursorSecondaryClick();
        #endregion
    }

}
