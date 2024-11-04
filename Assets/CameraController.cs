using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float moveSpeed = 10f;        // Speed of movement
    public float lookSensitivity = 2f;   // Sensitivity of the mouse look
    public float maxLookAngle = 90f;     // Maximum angle the camera can look up/down
    public bool disableMovement = false; // Disables movement if text inputs are focused

    private float rotationX = 0f;
    private float rotationY = 0f;

    void Start() {
        Cursor.lockState = CursorLockMode.Locked;
    }
    void Update()
    {   
        if (disableMovement) return;

        // Movement
        float moveForward = Input.GetAxis("Vertical") * moveSpeed * Time.deltaTime;
        float moveSideways = Input.GetAxis("Horizontal") * moveSpeed * Time.deltaTime;
        float moveUp = Input.GetKey(KeyCode.Space) ? moveSpeed * Time.deltaTime : 0f;
        moveUp += Input.GetKey(KeyCode.LeftShift) ? -moveSpeed * Time.deltaTime : 0f;

        Vector3 move = ElimY(transform.forward) * moveForward
            + ElimY(transform.right) * moveSideways
            + Vector3.up * moveUp;
        transform.position += move;

        // Rotation
        float mouseX = Input.GetAxis("Mouse X") * lookSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * lookSensitivity;

        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, -maxLookAngle, maxLookAngle);
        rotationY += mouseX;
        rotationY %= 360;

        Camera.main.transform.localRotation = Quaternion.Euler(rotationX, rotationY, 0);
    }

    private Vector3 ElimY(Vector3 v) {
        v.y = 0;
        v.Normalize();
        return v;
    }
}

