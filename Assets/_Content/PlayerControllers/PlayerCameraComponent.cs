using UnityEngine;

/// <summary>
///Allows the camera to orbit around a target transform.
/// </summary>
public class PlayerCameraComponent : MonoBehaviour
{
    [Header("Target Settings")]
    [SerializeField]
    [Tooltip("The target transform to orbit around. If null, uses targetPoint.")]
    private Transform _target;

    [SerializeField]
    [Tooltip("The target point to orbit around if target is null.")]
    private Vector3 _targetPoint = Vector3.zero;

    [Header("Distance Settings")]
    [SerializeField]
    [Tooltip("The initial distance from the target.")]
    private float _initialDistance = 10f;

    [SerializeField]
    [Tooltip("The minimum distance from the target.")]
    [Min(0)]
    private float _minDistance = 2f;

    [SerializeField]
    [Tooltip("The maximum distance from the target.")]
    [Min(0)]
    private float _maxDistance = 20f;

    [Header("Speed Settings")]
    [SerializeField]
    [Min(0)]
    [Tooltip("The speed at which the camera zooms in and out.")]
    private float _scrollSpeed = 2f;

    [SerializeField]
    [Min(0)]
    [Tooltip("The speed at which the camera orbits horizontally (right-click).")]
    private float _orbitSpeed = 100f;

    [SerializeField]
    [Range(0,1)]
    [Tooltip("The smoothing time for camera movements.")]
    private float _smoothTime = 0.2f;

    /// <summary>
    /// Target distance 
    /// </summary>
    private float _targetDistance;

    /// <summary>
    /// Current vertical angle 
    /// </summary>
    private float _currentVerticalAngle = 30f;

    /// <summary>
    /// Target vertical angle 
    /// </summary>
    private float _targetVerticalAngle;

    /// <summary>
    /// Current horizontal angle 
    /// </summary>
    private float _currentHorizontalAngle = 0f;

    /// <summary>
    /// Target horizontal angle 
    /// </summary>
    private float _targetHorizontalAngle;

    /// <summary>
    /// Current target position
    /// </summary>
    private Vector3 _currentTargetPosition;


    /// <summary>
    /// Initializes the camera's position and settings.
    /// </summary>
    void Start()
    {
        // Use targetPoint if target is null
        _currentTargetPosition = _target != null ? _target.position : _targetPoint;

        _targetDistance = _initialDistance;
        _targetVerticalAngle = _currentVerticalAngle;
        _targetHorizontalAngle = _currentHorizontalAngle;
    }

    /// <summary>
    /// Updates the camera's position and rotation every frame after all other updates.
    /// </summary>
    void LateUpdate()
    {
        // Update the current target position
        _currentTargetPosition = _target != null ? _target.position : _targetPoint;

        // Handle zooming with the mouse scroll wheel
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0f)
        {
            // Adjust vertical angle and distance based on scroll input
            _targetVerticalAngle += scroll * _scrollSpeed * 10f;
            _targetVerticalAngle = Mathf.Clamp(_targetVerticalAngle, 5f, 85f); // Limit vertical angle

            _targetDistance -= scroll * _scrollSpeed;
            _targetDistance = Mathf.Clamp(_targetDistance, _minDistance, _maxDistance); // Limit distance
        }

        // Horizontal rotation with right-click
        if (Input.GetMouseButton(1))
        {
            float horizontalInput = Input.GetAxis("Mouse X");
            _targetHorizontalAngle += horizontalInput * _orbitSpeed * Time.deltaTime;
        }

        // Calculate the target position in spherical coordinates
        float verticalAngle = Mathf.Deg2Rad * _targetVerticalAngle; // Vertical angle 
        float horizontalAngle = Mathf.Deg2Rad * _targetHorizontalAngle; // Horizontal angle 

        Vector3 targetPosition = _currentTargetPosition + new Vector3(
            Mathf.Sin(verticalAngle) * Mathf.Cos(horizontalAngle), // x
            Mathf.Cos(verticalAngle),                   // y
            Mathf.Sin(verticalAngle) * Mathf.Sin(horizontalAngle)  // z
        ) * _targetDistance;


        // Smooth the camera's position
        transform.position = Vector3.Lerp(transform.position, targetPosition, _smoothTime);

        // Smooth the camera's rotation
        Quaternion targetRotation = Quaternion.LookRotation(_currentTargetPosition - transform.position);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, _smoothTime);
    }
}
