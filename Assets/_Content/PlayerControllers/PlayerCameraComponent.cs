using UnityEngine;

/// <summary>
/// A camera script that orbits around a target transform or a fixed point.
/// </summary>
public class PlayerCameraComponent : MonoBehaviour
{
    [Header("Target Settings")]
    [SerializeField]
    [Tooltip("The target transform to orbit around. If null, uses targetPoint.")]
    private Transform target;

    [SerializeField]
    [Tooltip("The target point to orbit around if target is null.")]
    private Vector3 targetPoint = Vector3.zero;

    [Header("Distance Settings")]
    [SerializeField]
    [Tooltip("The initial distance from the target.")]
    private float distance = 10f;

    [SerializeField]
    [Tooltip("The minimum distance from the target.")]
    private float minDistance = 2f;

    [SerializeField]
    [Tooltip("The maximum distance from the target.")]
    private float maxDistance = 20f;

    [Header("Speed Settings")]
    [SerializeField]
    [Tooltip("The speed at which the camera zooms in and out.")]
    private float scrollSpeed = 2f;

    [SerializeField]
    [Tooltip("The speed at which the camera orbits horizontally (right-click).")]
    private float orbitSpeed = 100f;

    [SerializeField]
    [Tooltip("The smoothing time for camera movements.")]
    private float smoothTime = 0.2f;

    // Target distance 
    private float targetDistance;

    // Current vertical angle 
    private float currentVerticalAngle = 30f;

    // Target vertical angle 
    private float targetVerticalAngle;

    // Current horizontal angle 
    private float currentHorizontalAngle = 0f;

    // Target horizontal angle 
    private float targetHorizontalAngle;

    // Velocity for SmoothDamp
    private Vector3 velocity = Vector3.zero;

    // Current target position
    private Vector3 currentTargetPosition;


    /// <summary>
    /// Initializes the camera's position and settings.
    /// </summary>
    void Start()
    {
        // Use targetPoint if target is null
        currentTargetPosition = target != null ? target.position : targetPoint;

        // Initialize variables
        targetDistance = distance;
        targetVerticalAngle = currentVerticalAngle;
        targetHorizontalAngle = currentHorizontalAngle;
    }

    /// <summary>
    /// Updates the camera's position and rotation every frame after all other updates.
    /// </summary>
    void LateUpdate()
    {
        // Update the current target position
        currentTargetPosition = target != null ? target.position : targetPoint;

        // Handle zooming with the mouse scroll wheel
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0f)
        {
            // Adjust vertical angle and distance based on scroll input
            targetVerticalAngle += scroll * scrollSpeed * 10f;
            targetVerticalAngle = Mathf.Clamp(targetVerticalAngle, 5f, 85f); // Limit vertical angle

            targetDistance -= scroll * scrollSpeed;
            targetDistance = Mathf.Clamp(targetDistance, minDistance, maxDistance); // Limit distance
        }

        // Horizontal rotation with right-click
        if (Input.GetMouseButton(1))
        {
            float horizontalInput = Input.GetAxis("Mouse X");
            targetHorizontalAngle += horizontalInput * orbitSpeed * Time.deltaTime;
        }

        // Calculate the target position in spherical coordinates
        float verticalAngle = Mathf.Deg2Rad * targetVerticalAngle; // Vertical angle 
        float horizontalAngle = Mathf.Deg2Rad * targetHorizontalAngle; // Horizontal angle 

        Vector3 targetPosition = currentTargetPosition + new Vector3(
            Mathf.Sin(verticalAngle) * Mathf.Cos(horizontalAngle), // x
            Mathf.Cos(verticalAngle),                   // y
            Mathf.Sin(verticalAngle) * Mathf.Sin(horizontalAngle)  // z
        ) * targetDistance;


        // Smooth the camera's position
        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothTime);

        // Smooth the camera's rotation
        Quaternion targetRotation = Quaternion.LookRotation(currentTargetPosition - transform.position);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, smoothTime);
    }
}
