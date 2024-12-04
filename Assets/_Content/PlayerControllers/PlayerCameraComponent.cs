using UnityEngine;

/// <summary>
/// Allows the camera to orbit around a target transform and move using WASD keys.
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
    private float _scrollSpeed = 5f;

    [SerializeField]
    [Min(0)]
    [Tooltip("The speed at which the camera orbits horizontally (right-click).")]
    private float _orbitSpeed = 5;

    [SerializeField]
    [Range(0, 1)]
    [Tooltip("The smoothing time for camera movements.")]
    private float _smoothTime = 0.2f;

    [SerializeField]
    [Min(0)]
    [Tooltip("The step at which the camera moves with WASD.")]
    private float _stepAngle = 1f;
    
    [SerializeField]
    [Min(0)]
    [Tooltip("The speed at which the camera moves with WASD.")]
    private float _WASDSpeed = 1f;

    private float _targetDistance;
    private float _currentVerticalAngle = 30f;
    private float _targetVerticalAngle;
    private float _currentHorizontalAngle = 0f;
    private float _targetHorizontalAngle;
    private Vector3 _currentTargetPosition;

    void Start()
    {
        _currentTargetPosition = _target != null ? _target.position : _targetPoint;
        _targetDistance = _initialDistance;
        _targetVerticalAngle = _currentVerticalAngle;
        _targetHorizontalAngle = _currentHorizontalAngle;
    }

    void LateUpdate()
    {
        // Update the current target position
        if (_target != null)
        {
            _currentTargetPosition = _target.position;
        }

        // Handle zooming with the mouse scroll wheel
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0f)
        {
            _targetVerticalAngle += scroll * _scrollSpeed * 10f;
            _targetVerticalAngle = Mathf.Clamp(_targetVerticalAngle, 5f, 85f);

            _targetDistance -= scroll * _scrollSpeed;
            _targetDistance = Mathf.Clamp(_targetDistance, _minDistance, _maxDistance);
        }

        // Horizontal rotation with right-click
        if (Input.GetMouseButton(1))
        {
            float horizontalInput = Input.GetAxis("Mouse X");
            _targetHorizontalAngle += horizontalInput * _orbitSpeed * -10 * Time.deltaTime;

            float verticalInput = Input.GetAxis("Mouse Y");
            _targetVerticalAngle += verticalInput * _orbitSpeed * -10 * Time.deltaTime;
        }


        // WASD controls for target movement
        Vector3 movement = Vector3.zero;
        if (Input.GetKey(KeyCode.W)) movement += transform.forward;  // Forward
        if (Input.GetKey(KeyCode.S)) movement -= transform.forward;  // Backward
        if (Input.GetKey(KeyCode.A)) movement -= transform.right;    // Left
        if (Input.GetKey(KeyCode.D)) movement += transform.right;    // Right

        if (Input.GetKey(KeyCode.Q)) _targetHorizontalAngle -= _stepAngle; // Rotate camera left
        if (Input.GetKey(KeyCode.E)) _targetHorizontalAngle += _stepAngle; // Rotate camera right


        // Move with WASD
        if (movement != Vector3.zero)
        {
            movement.Normalize();
            _currentTargetPosition += movement * _WASDSpeed * Time.deltaTime;
        }

        // Clamp angle
        _targetVerticalAngle = Mathf.Clamp(_targetVerticalAngle, 5f, 85f);

        // Calculate the target position in spherical coordinates
        float verticalAngle = Mathf.Deg2Rad * _targetVerticalAngle;
        float horizontalAngle = Mathf.Deg2Rad * _targetHorizontalAngle;

        Vector3 targetPosition = _currentTargetPosition + new Vector3(
            Mathf.Sin(verticalAngle) * Mathf.Cos(horizontalAngle),   // x
            Mathf.Cos(verticalAngle),                               // y
            Mathf.Sin(verticalAngle) * Mathf.Sin(horizontalAngle)  // z
        ) * _targetDistance;

        transform.position = Vector3.Lerp(transform.position, targetPosition, _smoothTime);

        Quaternion targetRotation = Quaternion.LookRotation(_currentTargetPosition - transform.position);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, _smoothTime);
    }

    /// <summary>
    /// Sets a new target for the camera.
    /// </summary>
    /// <param name="newTarget">The new target transform.</param>
    public void SetTarget(Transform newTarget)
    {
        _target = newTarget;
        if (_target == null)
        {
            _targetPoint = transform.position + transform.forward * _initialDistance;
        }
    }
}
