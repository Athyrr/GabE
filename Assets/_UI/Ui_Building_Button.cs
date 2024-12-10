using GabE.Module.ECS;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class Ui_Building_Button : MonoBehaviour
{
    [SerializeField]
    private Button _createBuildingButton = null;

    [SerializeField]
    private UI_BuildingManager _buildingManager = null;

    [SerializeField]
    private BuildingType _buildingType;

    [SerializeField]
    private float3 _position = float3.zero;

    private EntityCommandBufferSystem _ecbSystem = null;


    private void OnEnable()
    {
        if (_createBuildingButton != null)
            _createBuildingButton.onClick.AddListener(NotifyCreateBuilding);
    }

    private void OnDisable()
    {
        if (_createBuildingButton != null)
            _createBuildingButton.onClick.RemoveListener(NotifyCreateBuilding);
    }

    private void Start()
    {
        _ecbSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<EntityCommandBufferSystem>();
    }


    private void NotifyCreateBuilding()
    {
        if (_buildingManager.WantBuild && _buildingManager.BuildingLoad == _buildingType)
        {
            _buildingManager.WantBuild = false;
            _buildingManager.BuildingLoad = BuildingType.None;
            return;
        }

        _buildingManager.BuildingLoad = _buildingType;
        _buildingManager.WantBuild = true;
    }
}
