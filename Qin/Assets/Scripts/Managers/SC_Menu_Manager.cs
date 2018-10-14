using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using UnityEngine.EventSystems;
using static SC_Global;
using static SC_Player;

public class SC_Menu_Manager : MonoBehaviour {

    #region Variables

    static SC_UI_Manager uiManager;

    static SC_Tile_Manager tileManager;

    public static SC_Menu_Manager Instance { get; set; }

    public enum MenuType {Player, Character};

    GameObject menu; 

    #endregion

    #region Setup

    // Use this for initialization
    void Start () {
 
        uiManager = SC_UI_Manager.Instance;

        tileManager = uiManager.TileManager;

    }

    private void Awake()
    {
        Instance = this;
    }

    #endregion

    #region Menu Position

    //Move the menu next to the tile
    public void MenuPos(MenuType type)
    {
        switch (type)
        {
            case MenuType.Character:
                menu = uiManager.characterActionsPanel;
                break;
            case MenuType.Player:
                menu = uiManager.playerActionsPanel;
                break;
        }

        RectTransform Rect = menu.GetComponent<RectTransform>();

        //Get the viewport position of the tile
        Vector3 currentTileViewportPos = Camera.main.WorldToViewportPoint(tileManager.GetTileAt(SC_Cursor.Instance.gameObject).transform.position);

        //If tile on the left side of the screen, offset the menu on the right
        //If tile on the right side of the screen, offset the menu on the left
        int offset = currentTileViewportPos.x < 0.5 ? 1 : -1;

        Rect.anchorMin = new Vector3(currentTileViewportPos.x + (offset * (0.1f + (0.05f * (1 / (Mathf.Pow(Camera.main.orthographicSize, Camera.main.orthographicSize / 4)))))), currentTileViewportPos.y, currentTileViewportPos.z);
        Rect.anchorMax = Rect.anchorMin;

        menu.SetActive(true);

    }

    #endregion

}
