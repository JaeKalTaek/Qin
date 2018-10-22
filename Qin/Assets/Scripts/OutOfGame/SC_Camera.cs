using UnityEngine;
using static SC_Game_Manager;

public class SC_Camera : MonoBehaviour {
		
    [Header("Camera Variables")]
    [Tooltip("Speed at which the camera lerps to its target position")]
	public float moveSpeed;

    [Tooltip("List of zooms possible for the camera")]
    public float[] zooms;

    int zoomIndex;

    [Tooltip("Index of the default zoom value in the zooms array")]
    public int defaultZoomIndex;

    [Tooltip("Speed at which the camera lerps to its target zoom")]
    public float zoomSpeed;

    [Tooltip("Speed at which the camera lerps to its target position when the player is zooming wider")]
    public float widerZoomSpeedMultiplier;

    /*[Tooltip("Margin between the board and the camera border")]
    public float boardMargin;*/

    public Vector3 TargetPosition { get; set; }

    /*[HideInInspector]
    public bool minX, maxX, minY, maxY;*/

    Camera cam;

    private void OnValidate () {

        /*if (boardMargin < 0)
            boardMargin = 0;*/

        defaultZoomIndex = Mathf.Clamp(defaultZoomIndex, 0, zooms.Length - 1);

    }

    public void Setup(int sizeX, int sizeY) {

        cam = GetComponent<Camera>();

		transform.position = new Vector3 (((sizeX - 1) / 2) * TileSize, ((sizeY - 1) / 2) * TileSize, -16);

        TargetPosition = transform.position;

        zoomIndex = defaultZoomIndex;

        cam.orthographicSize = zooms[zoomIndex];

    }

    void Update() {        

        if (cam) {

            int previousZoomIndex = zoomIndex;

            zoomIndex = Mathf.Clamp(zoomIndex - Mathf.RoundToInt(Input.GetAxisRaw("Mouse ScrollWheel")), 0, zooms.Length - 1);

            if (cam.orthographicSize != zooms[zoomIndex])
                cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, zooms[zoomIndex], zoomSpeed * Time.deltaTime);

            /*float xMax = SC_Tile_Manager.Instance.xSize * TileSize - cam.orthographicSize * cam.aspect - .5f + boardMargin;
            float xMin = cam.orthographicSize* cam.aspect - .5f - boardMargin;

            float x = Mathf.Clamp(TargetPosition.x, xMin, xMax);

            x = minX ? xMin : maxX ? xMax : x;

            float yMax = SC_Tile_Manager.Instance.ySize * TileSize - cam.orthographicSize - .5f + boardMargin;
            float yMin = cam.orthographicSize - .5f - boardMargin;

            float y = Mathf.Clamp(TargetPosition.y, yMin, yMax);

            y = minY ? yMin : maxY ? yMax : y;

            TargetPosition = new Vector3(x, y, -16);*/

            if (transform.position != TargetPosition) {

                float speed = moveSpeed * Time.deltaTime * (previousZoomIndex < zoomIndex ? widerZoomSpeedMultiplier : 1);

                transform.position = Vector3.Lerp(transform.position, TargetPosition, speed);

            }

        }        

    }

}