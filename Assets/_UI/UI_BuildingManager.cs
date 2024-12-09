using UnityEngine;
using Unity.Entities;

using GabE.Module.ECS;


public class UI_BuildingManager : MonoBehaviour
{
    [SerializeField]
    PlayerCameraComponent _camera = null;


    private EntityCommandBufferSystem _ecbSystem = null;

    public bool WantBuild = false;
    public BuildingType BuildingLoad;


    private void Start()
    {
        if (_camera == null)
            Debug.LogWarning("Camera is null in UI_Building!", this);

        _ecbSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<EntityCommandBufferSystem>();

        WantBuild = false;
        BuildingLoad = BuildingType.None;
    }


    private void Update()
    {
        if (Input.GetMouseButton(0) && WantBuild && BuildingLoad != BuildingType.None)
            OnCreateBuilding(BuildingLoad);
    }

    private void OnCreateBuilding(BuildingType type)
    {
        Debug.Log("Create building button clicked!");

        var ecb = _ecbSystem.CreateCommandBuffer();

        var buildingRequest = ecb.CreateEntity();
        ecb.AddComponent(buildingRequest, new ECS_CreateBuildingTag());
        ecb.AddComponent(buildingRequest, new ECS_PositionFragment
        {
            Position = _camera.BuildingRequestPosition
        });
        ecb.AddComponent(buildingRequest, new ECS_BuildingFragment { Type = BuildingLoad });

        _ecbSystem.AddJobHandleForProducer(default);


        BuildingLoad = BuildingType.None;
        WantBuild = false;
    }
}
