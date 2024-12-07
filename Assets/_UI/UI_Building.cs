using UnityEngine;
using UnityEngine.UI;
using Unity.Entities;
using Unity.Mathematics;
using GabE.Module.ECS;

public class UI_Building : MonoBehaviour
{
    [SerializeField]
    private Button _createBuildingButton;

    [SerializeField]
    private BuildingType _buildingType;

    [SerializeField]
    private float3 _position = float3.zero;

    private EntityCommandBufferSystem _ecbSystem;

    private void OnEnable()
    {
        if (_createBuildingButton != null)
            _createBuildingButton.onClick.AddListener(OnCreateBuildingClicked);
    }

    private void OnDisable()
    {
        if (_createBuildingButton != null)
            _createBuildingButton.onClick.RemoveListener(OnCreateBuildingClicked);
    }

    private void Start()
    {
        _ecbSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<EntityCommandBufferSystem>();
    }

    private void OnCreateBuildingClicked()
    {
        Debug.Log("Create building button clicked!");

        var ecb = _ecbSystem.CreateCommandBuffer();

        var buildingRequest = ecb.CreateEntity();
        ecb.AddComponent(buildingRequest, new ECS_CreateBuildingTag());
        ecb.AddComponent(buildingRequest, new ECS_PositionFragment { Position = _position });
        ecb.AddComponent(buildingRequest, new ECS_BuildingFragment { Type = _buildingType });

        _ecbSystem.AddJobHandleForProducer(default);
    }
}
