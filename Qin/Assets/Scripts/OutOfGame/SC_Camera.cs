
using UnityEngine;
using System.Collections;


[AddComponentMenu("Camera-Control/3dsMax Camera Style")]
public class SC_Camera : MonoBehaviour {

    public GameObject cameraParent;
    public Transform target;
    public Vector3 targetOffset;
    public float distance = 5.0f;
    public float maxDistance = 20;
    public float minDistance = .6f;
    public float xSpeed = 200.0f;
    public float ySpeed = 200.0f;
    public int yMinLimit = -80;
    public int yMaxLimit = 80;
    public int zoomRate = 40;
    public float panSpeed = 0.3f;
    public float zoomDampening = 5.0f;

    /*float xDeg = 0.0f;
    float yDeg = 0.0f;*/
    float currentDistance;
    float desiredDistance;
    /*Quaternion currentRotation;
    Quaternion desiredRotation;*/
    Quaternion rotation;
    Vector3 position;

    bool finishRotation;
    Quaternion targetRotation, targetRotation2;

    void OnEnable() { Init(); }

    public void Init() {
        //If there is no target, create a temporary target at 'distance' from the cameras current viewpoint
        if (!target) {

            GameObject go = new GameObject("Cam Target");
            go.transform.position = transform.position + (transform.forward * distance);
            target = go.transform;

        }

        distance = Vector3.Distance(transform.position, target.position);
        currentDistance = distance;
        desiredDistance = distance;

        //be sure to grab the current rotations as starting points.
        position = transform.position;
        rotation = transform.rotation;

        /*currentRotation = transform.rotation;
        desiredRotation = transform.rotation;

        xDeg = Vector3.Angle(Vector3.right, transform.right);
        yDeg = Vector3.Angle(Vector3.up, transform.up);*/

    }

    void Start() {

        Init();
        targetRotation = Quaternion.identity;
        targetRotation2 = Quaternion.identity;
        finishRotation = true;

    }

    void Update() {

        if (Input.GetKeyDown(KeyCode.A) && finishRotation) {

            targetRotation.eulerAngles = cameraParent.transform.rotation.eulerAngles + new Vector3(0, 0, -90);
            targetRotation2.eulerAngles = FindObjectOfType<SC_Lifebar>().transform.parent.rotation.eulerAngles + new Vector3(0, 0, -90);
            finishRotation = false;

        } else if (Input.GetKeyDown(KeyCode.E) && finishRotation) {

            targetRotation.eulerAngles = cameraParent.transform.rotation.eulerAngles + new Vector3(0, 0, 90);
            targetRotation2.eulerAngles = FindObjectOfType<SC_Lifebar>().transform.parent.rotation.eulerAngles + new Vector3(0, 0, 90);
            finishRotation = false;

        }

        if(!finishRotation) {

            cameraParent.transform.rotation = Quaternion.Lerp(cameraParent.transform.rotation, targetRotation, 0.1f);
            foreach(SC_Lifebar lifebar in FindObjectsOfType<SC_Lifebar>())
                lifebar.transform.parent.rotation = Quaternion.Lerp(lifebar.transform.parent.rotation, targetRotation2, 0.1f);

            if ((cameraParent.transform.eulerAngles.z > targetRotation.eulerAngles.z - 0.01) && (cameraParent.transform.eulerAngles.z < targetRotation.eulerAngles.z + 0.01))
                finishRotation = true;

        }

    }

    /*
     * Camera logic on LateUpdate to only update after all character movement logic has been handled. 
     */
    void LateUpdate() {
        // If Control and Alt and Middle button? ZOOM!
        if (Input.GetMouseButton(2) && Input.GetKey(KeyCode.LeftAlt) && Input.GetKey(KeyCode.LeftControl)) {

            desiredDistance -= Input.GetAxis("Mouse Y") * Time.deltaTime * zoomRate * 0.125f * Mathf.Abs(desiredDistance);

        }
        // If middle mouse and left alt are selected? ORBIT
        /*else if (Input.GetMouseButton(2) && Input.GetKey(KeyCode.LeftAlt))
        {
            xDeg += Input.GetAxis("Mouse X") * xSpeed * 0.02f;
            yDeg -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;

            ////////OrbitAngle

            //Clamp the vertical axis for the orbit
            yDeg = ClampAngle(yDeg, yMinLimit, yMaxLimit);
            // set camera rotation 
            desiredRotation = Quaternion.Euler(yDeg, xDeg, 0);
            currentRotation = transform.rotation;

            rotation = Quaternion.Lerp(currentRotation, desiredRotation, Time.deltaTime * zoomDampening);
            transform.rotation = rotation;
        }*/
        // otherwise if middle mouse is selected, we pan by way of transforming the target in screenspace
        else if (Input.GetMouseButton(2)) {

            //grab the rotation of the camera so we can move in a pseudo local XY space
            target.rotation = transform.rotation;
            target.Translate(Vector3.right * -Input.GetAxis("Mouse X") * panSpeed);
            target.Translate(transform.up * -Input.GetAxis("Mouse Y") * panSpeed, Space.World);

        }

        ////////Orbit Position

        // affect the desired Zoom distance if we roll the scrollwheel
        desiredDistance -= Input.GetAxis("Mouse ScrollWheel") * Time.deltaTime * zoomRate * Mathf.Abs(desiredDistance);
        //clamp the zoom min/max
        desiredDistance = Mathf.Clamp(desiredDistance, minDistance, maxDistance);
        // For smoothing of the zoom, lerp distance
        currentDistance = Mathf.Lerp(currentDistance, desiredDistance, Time.deltaTime * zoomDampening);

        // calculate position based on the new currentDistance 
        position = target.position - (rotation * Vector3.forward * currentDistance + targetOffset);
        transform.position = position;

    }

    private static float ClampAngle(float angle, float min, float max) {

		angle += (angle < -360) ? 360 : ((angle > 360) ? -360 : 0);

        return Mathf.Clamp(angle, min, max);

    }

}