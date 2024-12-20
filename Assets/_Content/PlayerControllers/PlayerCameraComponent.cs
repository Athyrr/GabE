using System;
using _Modules.GE_Voxel;
using _Modules.GE_Voxel.Utils;
using GabE.Module.ECS;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine.UIElements;

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

    // Camera
    private Vector3 _targetPoint = Vector3.zero;
    private Vector3 _currentTargetPosition;

    //Voxel
    private byte _chunkLoop;
    private byte _chunkSize;
    private byte _yMax;

    //Building request
    private float3 _buildingRequestPosition = float3.zero;
    public float3 BuildingRequestPosition => _buildingRequestPosition;

    /// <summary>
    /// Initializes the camera's position, target, and preview mesh.
    /// </summary>
    void Start()
    {
        _currentTargetPosition = _initialPosition;
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

            //Vector3 scrollMove = _currentTargetPosition += -right;
            //scrollMove = _currentTargetPosition += -fwd;

            //scrollMove.Normalize();

            //_currentTargetPosition = scrollMove * sensitivityX;
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

        MouseAction();
    }

    private bool _clickPressed = false;
    private bool _selectedActif = false;
    private float _clickPressedTime = 0.0f;
    private float3 _clickPressedStartPosition;
    private void MouseAction()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

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

            // Check if ray intersects chunk's bounding box
            bool isIntersecting = GE_Math.IsRayIntersectingAABB(
                new float3(transform.position.x, transform.position.y, transform.position.z),
                new float3(ray.direction.x, ray.direction.y, ray.direction.z),
                new float3(chunkPosition.x, chunkPosition.y, chunkPosition.z),
                new float3(_chunkSize, _yMax, _chunkSize)
            );

            if (isIntersecting)
                _allChunksIndexActivable[_allChunksIndexLength++] = i;
        }

        Array.Resize(ref _allChunksIndexActivable, _allChunksIndexLength);
        byte intersectDebugCount = 0;

        foreach (byte chunkIndex in _allChunksIndexActivable)
        {
            int chunkX = chunkIndex % _chunkLoop;
            int chunkY = chunkIndex / _chunkLoop;
            Vector3 chunkPosition = new Vector3(
                (_chunkSize * chunkX - _chunkLoop * _chunkSize * 0.5f + _chunkSize * 0.5f),
                5f,
                _chunkSize * chunkY - _chunkLoop * _chunkSize * 0.5f + _chunkSize * 0.5f
            );
            
            for (byte x = 0; x < _chunkSize; ++x)
            {
                for (byte z = 0; z < _chunkSize; ++z)
                {
                    byte y = (byte)(_voxelRunner._chunks[chunkIndex]._nchunk[x + _chunkSize * z]);

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

                        var ct = _previewMesh.transform;

                        if (Input.GetMouseButton(0) && !_clickPressed)
                        {
                            _clickPressed = true;
                            _clickPressedStartPosition = ct.position;
                        }
                        else if (Input.GetMouseButton(0) && _clickPressed)
                        {
                            if (_clickPressedTime > 2)
                            {
                                float3 gap = _clickPressedStartPosition - (float3)cubePosition;
                                ct.position = _clickPressedStartPosition - gap * 0.5f;
                                ct.localScale = new float3(gap.x,10,gap.z);
                            }
                            _clickPressedTime = _clickPressedTime + 0.1f;
                        }
                        else if (!Input.GetMouseButton(0) && _clickPressed)
                        {
                            if (_clickPressedTime < 2)
                                sendEntitiesAtLOcation(cubePosition);
                            else
                            {
                                float3 gap = _clickPressedStartPosition - (float3)cubePosition;
                                GetAllEntities((float3)(_clickPressedStartPosition - gap * 0.5f), new float3(gap.x,10,gap.z));
                            }
                            _clickPressed = false;
                            _clickPressedTime = 0.0f;
                            
                        }
                        else
                        {
                            ct.localScale = new float3(1);
                            _previewMesh.transform.position = cubePosition;
                            
                            // To send data at ECS
                            _buildingRequestPosition = _previewMesh.transform.position;
                        }
                        
                        intersectDebugCount++;
                    }
                }
            }
        }
    }
    NativeArray<ECS_PositionFragment> tmpListPositions;
    public void GetAllEntities(float3 boxLocation, float3 boundsSize)
    {
        EntityManager EM = World.DefaultGameObjectInjectionWorld.EntityManager;
        EntityQuery tmp = EM.CreateEntityQuery(ComponentType.ReadWrite<ECS_PositionFragment>());
        NativeArray<ECS_PositionFragment> v = tmp.ToComponentDataArray<ECS_PositionFragment>(Allocator.Temp);

        tmpListPositions = new NativeArray<ECS_PositionFragment>(v.Length, Allocator.Persistent);

        int count = 0;
        for (int i = 0; i < v.Length; ++i)
        {
            ECS_PositionFragment e = v[i];
            float3 _p = (float3)e.Position;
            if (
                IsPositionInBox(boxLocation, boundsSize, _p)
            )
            {
                tmpListPositions[count] = e;
                ++count;
            }
        }
    }

    public void sendEntitiesAtLOcation(float3 p)
    {
        for (int i = 0; i < tmpListPositions.Length; ++i)
        {
            ECS_PositionFragment e = tmpListPositions[i];
            e.Position = p;
            Debug.Log(p);
        }
    }
    
    static bool IsPositionInBox(float3 boxCenter, float3 boxBounds, float3 entityPosition)
    {
        float3 boxMin = new float3(boxCenter.x - boxBounds.x, boxCenter.y - boxBounds.y, boxCenter.z - boxBounds.z);
        float3 boxMax = new float3(boxCenter.x + boxBounds.x, boxCenter.y + boxBounds.y, boxCenter.z + boxBounds.z);

        return entityPosition.x >= boxMin.x && entityPosition.x <= boxMax.x &&
            entityPosition.y >= boxMin.y && entityPosition.y <= boxMax.y &&
            entityPosition.z >= boxMin.z && entityPosition.z <= boxMax.z;
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
}
