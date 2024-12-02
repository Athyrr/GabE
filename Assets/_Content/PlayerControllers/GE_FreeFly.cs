using UnityEngine;

public class CameraFly : MonoBehaviour
{
    
    /*
     * use this script only to debug and on camera :p
     */
    
    public float moveSpeed = 10f;
    public float rotationSpeed = 100f;

    void Update()
    {
        float horizontal = Input.GetAxis("Horizontal"); // A/D or Left/Right
        float vertical = Input.GetAxis("Vertical");     // W/S or Up/Down
        float upDown = 0f;

        if (Input.GetKey(KeyCode.E)) upDown = 1f;  // E for up
        if (Input.GetKey(KeyCode.Q)) upDown = -1f; // Q for down

        Vector3 moveDirection = new Vector3(horizontal, upDown, vertical);
        transform.Translate(moveDirection * moveSpeed * Time.deltaTime, Space.Self);

        float mouseX = Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * rotationSpeed * Time.deltaTime;

        transform.Rotate(Vector3.up, mouseX, Space.World);
        transform.Rotate(Vector3.left, mouseY, Space.Self);
    }
}