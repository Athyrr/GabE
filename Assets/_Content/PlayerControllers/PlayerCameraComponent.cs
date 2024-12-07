using System;
using _Modules.GE_Voxel;
using _Modules.GE_Voxel.Utils;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using float3 = Unity.Mathematics.float3;

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
    
    // TODO : Delete this or clean
    [SerializeField]
    [Tooltip("Prefab to debug when we click")]
    private GameObject _debugPrefab;

    [SerializeField]
    [Tooltip("Prefab to debug when we click")]
    private GE_VoxelRunner _voxelRunner;

    private float _targetDistance;
    private float _currentVerticalAngle = 30f;
    private float _targetVerticalAngle;
    private float _currentHorizontalAngle = 0f;
    private float _targetHorizontalAngle;
    private Vector3 _currentTargetPosition;
    private GameObject _previewMesh;
    private byte _chunkLoop;
    private byte _chunkSize;
    private byte _yMax;
    
    void Start()
    {
        _currentTargetPosition = _target != null ? _target.position : _targetPoint;
        _targetDistance = _initialDistance;
        _targetVerticalAngle = _currentVerticalAngle;
        _targetHorizontalAngle = _currentHorizontalAngle;

        /*
         * Init value
         */
        if (!_voxelRunner) throw new Exception("any voxelRunner sending");
        
        _chunkLoop = _voxelRunner.chunkLoop;
        _chunkSize = _voxelRunner.chunkSize;
        _yMax = _voxelRunner.yMax;
        
        /*
         * Setup curser preview location
         */
        Vector3 position = new Vector3(0, 5, 0);
        _previewMesh = Instantiate(_debugPrefab, position, Quaternion.identity);
        //_previewMesh.transform.parent = this.transform;    
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
        
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        
        Vector3 p = ray.GetPoint(145f);
        Debug.DrawLine(
                transform.position,
                p,
                Color.red,
                0.5f
            );
        
        /*float2 a = new float2((float)math.cos(0), (float)math.cos(2f));
        float2 pHeight = new float2(0.5f, 0.5f) - 0.5f * a;
        
        // draw plane when we move curvor
        System.Numerics.Vector2 coord = new System.Numerics.Vector2((int)p.x, (int)p.y);
        float noiseY = GE_Math.Voronoise(
            coord, pHeight.x, pHeight.y
        )+2;*/

        var vc = _voxelRunner._chunks;
        byte[] _allChunksIndexActivable;
        byte _allChunksIndexLength = 0;
        _allChunksIndexActivable = new byte[vc.Length];
        
        for (byte i = 0; i < _voxelRunner._chunks.Length; ++i)
        {
            int x = i % _chunkLoop;
            int y = i / _chunkLoop;
            Vector3 chunkPosition = new Vector3(
                (x * _chunkSize + _chunkSize * 0.5f - _chunkLoop * _chunkSize * 0.5f) + 2,
                3f,
                (y * _chunkSize + _chunkSize * 0.5f - _chunkLoop * _chunkSize * 0.5f) + 2
            );

            bool isIntersecting = GE_Math.IsRayIntersectingAABB(
                new float3(transform.position.x, transform.position.y, transform.position.z),
                new float3(p.x, p.y, p.z),
                new float3(chunkPosition.x, chunkPosition.y, chunkPosition.z),
                new float3(_chunkSize, _yMax, _chunkSize)
            );

            if (!isIntersecting)
            {
                GE_Debug.DrawBox(
                    chunkPosition,
                    Quaternion.identity,
                    new Vector3(_chunkSize, _yMax, _chunkSize),
                    Color.black
                );
            } else 
                _allChunksIndexActivable[_allChunksIndexLength++] = i;

        }

        Array.Resize(ref _allChunksIndexActivable, _allChunksIndexLength);

        byte intersectDebugCount = 0;

        foreach (byte chunkIndex in _allChunksIndexActivable)
        {
            int chunkX = chunkIndex % _chunkLoop;
            int chunkY = chunkIndex / _chunkLoop;
            Vector3 chunkPosition = new Vector3(
                (chunkX * _chunkSize + _chunkSize * 0.5f - _chunkLoop * _chunkSize * 0.5f) + 2,
                5f,
                chunkY * _chunkSize + _chunkSize * 0.5f - _chunkLoop * _chunkSize * 0.5f + 2
                
            );

            float2 a = new float2((float)math.cos(0), (float)math.cos(2f));
            float2 pHeight = new float2(0.5f, 0.5f) - 0.5f * a;

            float2 offset = math.distance(new float2(_chunkLoop * 0.5f), new float2( chunkIndex * 0.1f, chunkIndex % 10));
            for (byte x = 0; x < _chunkSize; ++x)
            {
                for (byte z = 0; z < _chunkSize; ++z)
                {
                    //System.Numerics.Vector2 ps = (new System.Numerics.Vector2(x* 1, z* 1) + new System.Numerics.Vector2(chunkPosition.x, chunkPosition.y)*2)*.1f;
                    byte y = _voxelRunner._chunks[chunkIndex]._nchunkQuiFonctionne[x + _chunkSize * z];
                    //float noiseValue = GE_Math.Voronoise(ps, pHeight.x, pHeight.y);
                    //byte y = (byte)math.clamp((noiseValue * _yMax), 0, _yMax);
                    
                    Vector3 cubePosition = new Vector3(
                        chunkPosition.x + x - (_chunkSize * 0.5f) + 0.5f,
                        y+ 0.5f,
                        chunkPosition.z + z - (_chunkSize * 0.5f) + 0.5f
                    );

                    bool isCubeIntersecting = GE_Math.IsRayIntersectingAABB(
                        new float3(transform.position.x, transform.position.y, transform.position.z),
                        new float3(p.x, p.y, p.z),
                        new float3(cubePosition.x, cubePosition.y, cubePosition.z),
                        new float3(1, 1, 1)
                    );
                    
                    if (isCubeIntersecting)
                    {

                        _previewMesh.transform.position = cubePosition;
                        GE_Debug.DrawBox(
                            cubePosition,
                            Quaternion.identity,
                            new Vector3(1, 1, 1),
                            Color.cyan
                        );
                        GE_Debug.DrawBox(
                            transform.position,
                            Quaternion.identity,
                            new Vector3(6, 6, 6),
                            Color.red
                        );
                        GE_Debug.DrawBox(
                            cubePosition,
                            Quaternion.identity,
                            new Vector3(6, 6, 6),
                            Color.red
                        );
                        intersectDebugCount++;
                    }
                    else
                    {
                        GE_Debug.DrawBox(
                            cubePosition,
                            Quaternion.identity,
                            new Vector3(1, 1, 1),
                            Color.grey
                        );                        
                    }
                }
            }

            GE_Debug.DrawBox(
                chunkPosition,
                Quaternion.identity,
                new Vector3(_chunkSize, _yMax, _chunkSize),
                Color.green
            );
        }

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
    
    // Function to check if a ray intersects an AABB
    private bool RayIntersectsBox(Ray ray, Bounds bounds)
    {
        float tMin = (bounds.min.x - ray.origin.x) / ray.direction.x;
        float tMax = (bounds.max.x - ray.origin.x) / ray.direction.x;

        if (tMin > tMax) Swap(ref tMin, ref tMax);

        float tyMin = (bounds.min.y - ray.origin.y) / ray.direction.y;
        float tyMax = (bounds.max.y - ray.origin.y) / ray.direction.y;

        if (tyMin > tyMax) Swap(ref tyMin, ref tyMax);

        if ((tMin > tyMax) || (tyMin > tMax))
            return false;

        if (tyMin > tMin)
            tMin = tyMin;

        if (tyMax < tMax)
            tMax = tyMax;

        float tzMin = (bounds.min.z - ray.origin.z) / ray.direction.z;
        float tzMax = (bounds.max.z - ray.origin.z) / ray.direction.z;

        if (tzMin > tzMax) Swap(ref tzMin, ref tzMax);

        if ((tMin > tzMax) || (tzMin > tMax))
            return false;

        return true;
    }

    // Utility function to swap two values
    private void Swap(ref float a, ref float b)
    { // TODO : replace by swap with a,b = b,a
        float temp = a;
        a = b;
        b = temp;
    }
}
