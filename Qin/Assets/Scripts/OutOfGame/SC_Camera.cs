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

		transform.position = new Vector3 (Mathf.RoundToInt((sizeX - 1) / 2), Mathf.RoundToInt((sizeY - 1) / 2), -1);

        TargetPosition = transform.position;

        zoomIndex = defaultZoomIndex;

        cam.orthographicSize = zooms[zoomIndex];

    }

    void Update() {        

        if (cam) {
  
            zoomIndex = Mathf.Clamp(zoomIndex - Mathf.RoundToInt(Input.GetAxisRaw("Mouse ScrollWheel")), 0, zooms.Length - 1);

            if (cam.orthographicSize != zooms[zoomIndex])
                cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, zooms[zoomIndex], zoomSpeed * Time.deltaTime);

            if (transform.position != TargetPosition)
                transform.position = Vector3.Lerp(transform.position, TargetPosition, moveSpeed * Time.deltaTime);

        }        

    }

}