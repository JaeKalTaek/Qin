using UnityEngine;

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

    public Vector3 TargetPosition { get; set; }

    Camera cam;

    public void Setup(int sizeX, int sizeY) {

        cam = GetComponent<Camera>();

		transform.position = new Vector3 (Mathf.RoundToInt((sizeX - 1) / 2), Mathf.RoundToInt((sizeY - 1) / 2), -16);

        TargetPosition = transform.position;

        zoomIndex = defaultZoomIndex;

        cam.orthographicSize = zooms[zoomIndex];

    }

    void Update() {        

        if (cam) {
  
            zoomIndex = Mathf.Clamp(zoomIndex - Mathf.RoundToInt(Input.GetAxisRaw("Mouse ScrollWheel")), 0, zooms.Length - 1);

            if (cam.orthographicSize != zooms[zoomIndex])
                cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, zooms[zoomIndex], zoomSpeed * Time.deltaTime);

            float x = Mathf.Clamp(TargetPosition.x, cam.orthographicSize * cam.aspect - .5f, SC_Tile_Manager.Instance.xSize - cam.orthographicSize * cam.aspect - .5f);

            float y = Mathf.Clamp(TargetPosition.y, cam.orthographicSize - .5f, SC_Tile_Manager.Instance.ySize - cam.orthographicSize - .5f);

            Vector3 correctPos = new Vector3(x, y, -16);

            if (transform.position != correctPos)
                transform.position = Vector3.Lerp(transform.position, correctPos, moveSpeed * Time.deltaTime);

        }        

    }

}