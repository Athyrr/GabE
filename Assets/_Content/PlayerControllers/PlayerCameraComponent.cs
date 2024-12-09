using System;
using _Modules.GE_Voxel;
using _Modules.GE_Voxel.Utils;
using UnityEngine;
using float3 = Unity.Mathematics.float3;

/// <summary>
/// Allows the camera to orbit around a target transform and move using WASD keys.
/// </summary>
public class PlayerCameraComponent : MonoBehaviour
{
    [Header("Init")]

    [SerializeField]
    private Vector3 _initialPosition = new Vector3(0, 50, -50);

    [Header("Target Settings")]

    [SerializeField]
    [Tooltip("The target transform to orbit around. If null, uses targetPoint.")]
    private Transform _target;

    [SerializeField]
    [Tooltip("The target point to orbit around.")]
    private Vector3 _targetPointOffset = Vector3.zero;

    [Header("Distance Settings")]

    [SerializeField]
    [Tooltip("The minimum distance from the target.")]
    [Min(0)]
    private float _maxZoom = 10f;

    [SerializeField]
    [Tooltip("The maximum distance from the target.")]
    [Min(0)]
    private float minZoom = 150;

    [SerializeField]
    [Min(0)]
    private float maxZoomAngle = 30;

    [SerializeField]
    [Min(0)]
    private float minZoomAngle = 70;


    [Header("Speed Settings")]
    [SerializeField]
    [Range(0, 10)]
    private float sensitivityX = 2.0f;

    [SerializeField]
    [Range(0, 10)]
    private float sensitivityY = 2.0f;

    [SerializeField]
    [Min(0)]
    [Tooltip("The speed at which the camera zooms in and out.")]
    private float _scrollSpeed = 800f;

    [SerializeField]
    [Min(0)]
    private float _rotationSpeed = 50f;


    [SerializeField]
    [Range(0, 1)]
    [Tooltip("The smoothing time for camera movements.")]
    private float _smoothTime = 0.2f;

    [SerializeField]
    [Min(0)]
    [Tooltip("The speed at which the camera moves with WASD.")]
    private float _WASDSpeed = 1f;

    [SerializeField]
    [Tooltip("Mesh for preview the over")]
    private GameObject _previewMesh;

    [SerializeField]
    [Tooltip("material for _previewMesh")]
    private Material _previewMeshMaterial;

    [SerializeField]
    [Tooltip("The voxel runner component.")]
    private GE_VoxelRunner _voxelRunner;

    private Vector3 _targetPoint = Vector3.zero;
    private float _currentVerticalAngle = 30f;
    private float _targetVerticalAngle;
    private float _currentHorizontalAngle = 0f;
    private float _targetHorizontalAngle;
    private Vector3 _currentTargetPosition;
    private byte _chunkLoop;
    private byte _chunkSize;
    private byte _yMax;

    private float3 _buildingRequestPosition = float3.zero;  

    public float3 BuildingRequestPosition => _buildingRequestPosition;


    /// <summary>
    /// Initializes the camera's position, target, and preview mesh.
    /// </summary>
    void Start()
    {
        // Initialisation de la position et du point cible
        _currentTargetPosition = _initialPosition; // Position initiale �lev�e
        transform.position = _initialPosition;

        _scrollSpeed = 800;



        /*
         * Init value
         */
        if (!_voxelRunner) throw new Exception("VoxelRunner not assigned.");

        _chunkLoop = _voxelRunner.chunkLoop;
        _chunkSize = _voxelRunner.chunkSize;
        _yMax = _voxelRunner.yMax;

        /*
         * Setup cursor preview location
         */
        Vector3 position = new Vector3(0, 5, 0);
        _previewMesh = Instantiate(_previewMesh, position, Quaternion.identity);
        Renderer renderer = _previewMesh.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material = _previewMeshMaterial;
        }
        else
            Debug.LogError("Renderer not found on the instantiated object!");
    }




    /// <summary>
    /// Updates the camera's position and rotation based on user input.
    /// </summary>
    void LateUpdate()
    {
        if (Input.GetMouseButton(1))
        {
            float translationX = Input.GetAxis("Mouse X") * sensitivityX;
            float translationY = Input.GetAxis("Mouse Y") * sensitivityY;

            Vector3 right = transform.right;
            Vector3 fwd = transform.forward;
            fwd.y = 0;

            _currentTargetPosition += -right * translationX;
            _currentTargetPosition += -fwd * translationY;

        }

        transform.position = new Vector3(_currentTargetPosition.x, _currentTargetPosition.y, _initialPosition.z);



        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0f)
        {
            Vector3 zoomDirection = transform.forward.normalized;

            _currentTargetPosition += zoomDirection * scroll * _scrollSpeed * Time.deltaTime;

            if (_currentTargetPosition.y < _maxZoom)
                _currentTargetPosition.y = _maxZoom;


            if (_currentTargetPosition.y > minZoom)
                _currentTargetPosition.y = minZoom;


            float angleToStraighten = Mathf.InverseLerp(_maxZoom, minZoom, _currentTargetPosition.y);
            float rotationAngle = Mathf.Lerp(maxZoomAngle, minZoomAngle, angleToStraighten);

            transform.rotation = Quaternion.Euler(rotationAngle, transform.eulerAngles.y, transform.eulerAngles.z);
        }

        // ZQSD
        Vector3 movement = Vector3.zero;

        if (Input.GetKey(KeyCode.S))
        {
            movement -= transform.forward;
            movement.y = 0;
        }

        if (Input.GetKey(KeyCode.W))
        {
            movement += transform.forward;
            movement.y = 0;
        }

        if (Input.GetKey(KeyCode.A)) movement -= transform.right;
        if (Input.GetKey(KeyCode.D)) movement += transform.right;

        // Rotation
        if (Input.GetKey(KeyCode.Q)) transform.Rotate(Vector3.up, -_rotationSpeed * Time.deltaTime, Space.World);
        if (Input.GetKey(KeyCode.E)) transform.Rotate(Vector3.up, _rotationSpeed * Time.deltaTime, Space.World);


        if (movement != Vector3.zero)
        {
            movement.Normalize();
            _currentTargetPosition += movement * _WASDSpeed * Time.deltaTime;

            _targetPoint = transform.position + transform.forward + _targetPointOffset;
            _targetPoint.y = Mathf.Max(_targetPoint.y, 0);
        }
        transform.position = Vector3.Lerp(transform.position, _currentTargetPosition, _smoothTime);






        // Cast a ray from the camera to the mouse position
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        // Debug draw the ray
        Vector3 p = ray.GetPoint(145f);
        Debug.DrawLine(
            transform.position,
            p,
            Color.red,
            0.5f
        );


        var vc = _voxelRunner._chunks;
        byte[] _allChunksIndexActivable;
        byte _allChunksIndexLength = 0;
        _allChunksIndexActivable = new byte[vc.Length];

        // Iterate through all chunks
        for (byte i = 0; i < _voxelRunner._chunks.Length; ++i)
        {
            // Calculate chunk position
            int x = i % _chunkLoop;
            int y = i / _chunkLoop;
            Vector3 chunkPosition = new Vector3(
                (_chunkSize * x - _chunkLoop * _chunkSize / 2 + _chunkSize / 2),
                3f,
                (_chunkSize * y - _chunkLoop * _chunkSize / 2 + _chunkSize / 2)
            );

            // Debug draw chunk positions
            Debug.DrawLine(chunkPosition, new Vector3(chunkPosition.x, chunkPosition.y + 100, chunkPosition.z), Color.magenta, 1f);

            // Check if ray intersects chunk's bounding box
            bool isIntersecting = GE_Math.IsRayIntersectingAABB(
                new float3(transform.position.x, transform.position.y, transform.position.z),
                new float3(ray.direction.x, ray.direction.y, ray.direction.z),
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
            }
            else
                _allChunksIndexActivable[_allChunksIndexLength++] = i;
        }

        Array.Resize(ref _allChunksIndexActivable, _allChunksIndexLength);
        byte intersectDebugCount = 0;

        foreach (byte chunkIndex in _allChunksIndexActivable)
        {
            int chunkX = chunkIndex % _chunkLoop;
            int chunkY = chunkIndex / _chunkLoop;
            Vector3 chunkPosition = new Vector3(
                (_chunkSize * chunkX - _chunkLoop * _chunkSize / 2 + _chunkSize / 2),
                5f,
                _chunkSize * chunkY - _chunkLoop * _chunkSize / 2 + _chunkSize / 2

            );
            //NativeArray<byte> nchunk = _voxelRunner._chunks[chunkIndex].GetChunkValue();
            for (byte x = 0; x < _chunkSize; ++x)
            {
                for (byte z = 0; z < _chunkSize; ++z)
                {
                    byte y = (byte)(_voxelRunner._chunks[chunkIndex].GetNChunkValueAtCoordinate(x, z));
                    //Debug.Log(y);
                    Vector3 cubePosition = new Vector3(
                        chunkPosition.x + x - (_chunkSize * 0.5f) + 0.5f,
                        y + 1.1f,
                        chunkPosition.z + z - (_chunkSize * 0.5f) + 0.5f
                    );

                    bool isCubeIntersecting = GE_Math.IsRayIntersectingAABB(
                        new float3(transform.position.x, transform.position.y, transform.position.z),
                        new float3(ray.direction.x, ray.direction.y, ray.direction.z),
                        new float3(cubePosition.x, cubePosition.y, cubePosition.z),
                        new float3(1, 1, 1)
                    );

                    if (isCubeIntersecting)
                    {

                        //if (Input.GetMouseButton(0)) //@todo enable terraforming 
                        //    _voxelRunner._chunks[chunkIndex].TerraformingLandscape(x, z, 10);

                        _previewMesh.transform.position = cubePosition;
                        _buildingRequestPosition = _previewMesh.transform.position;

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
            _targetPoint = transform.position + transform.forward;
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
