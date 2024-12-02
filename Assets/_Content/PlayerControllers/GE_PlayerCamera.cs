using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class GE_PlayerCamera : MonoBehaviour
{
    public float moveSpeed = 1f;
    public float rotationSpeed = 1f;
    private float verticalRotation = 0f; // Tracks cumulative vertical rotation
    private bool _orbitMode = false;

    void Update()
    {
        float ms = moveSpeed * 100f;
        float rs = rotationSpeed * 100f;
        
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        float x = Input.GetAxis("Horizontal"); // A/D or Left/Right
        float y = Input.GetAxis("Vertical");     // W/S or Up/Down
        float slideX = 0f;

        if (Input.GetKey(KeyCode.E)) slideX = 1f;  // E for up
        if (Input.GetKey(KeyCode.Q)) slideX = -1f; // Q for down
        
        if (Input.GetMouseButtonDown(2)) _orbitMode = true;
        if (Input.GetMouseButtonUp(2)) _orbitMode = false;
        
        float t = Mathf.Clamp01(2f * Time.deltaTime);
        
        float targetY = Mathf.Lerp(transform.position.y, scroll * -1 * 100f, t);

        Vector3 moveDirection = new Vector3(slideX,
            targetY,
            y);
        transform.position = moveDirection;
        
        //moveDirection = transform.rotation * moveDirection;
        /*
        Vector3 clampedPosition = transform.position;
        clampedPosition.y = Mathf.Clamp(clampedPosition.y, 10f, 100f);
        transform.position = clampedPosition;*/
        
        float mouseX = Input.GetAxisRaw("Mouse X") * rs * Time.deltaTime;
        float mouseY = Input.GetAxisRaw("Mouse Y") * rs * Time.deltaTime;

        if (_orbitMode)
            verticalRotation -= mouseY; // Subtract because moving the mouse up is a negative rotation
            verticalRotation = Mathf.Clamp(verticalRotation, 30f, 60f);
            transform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f); // Vertical rotation (pitch)
    }
}