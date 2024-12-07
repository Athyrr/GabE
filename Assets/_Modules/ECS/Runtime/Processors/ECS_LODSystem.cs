using GabE.Module.ECS;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[UpdateInGroup(typeof(PresentationSystemGroup))]
public partial class ECS_LODSystem : SystemBase
{
    private Camera mainCamera;

    protected override void OnCreate()
    {
        mainCamera = Camera.main;

        if (mainCamera == null)
        {
            Debug.Log("Camera not found in system");
        }
        else
            Debug.Log("Camera  found in system");
    }

    protected override void OnUpdate()
    {
        Debug.Log("LOD system");
        //if (mainCamera == null) return;

        //@todo delete and get camrea
        foreach (var (lodComponent, position) in SystemAPI.Query<RefRW<ECS_MeshLODGroupFragment>, RefRW<ECS_PositionFragment>>())
            lodComponent.ValueRW.CurrentLOD = 0;
        return;
        //until here


        foreach (var (lodComponent, position) in SystemAPI.Query<RefRW<ECS_MeshLODGroupFragment>, RefRW<ECS_PositionFragment>>())
        {
            float distance = math.distance(position.ValueRO.Position, mainCamera.transform.position);

            if (distance < 10f)
                lodComponent.ValueRW.CurrentLOD = 0;
            else if (distance < 30f)
                lodComponent.ValueRW.CurrentLOD = 1;
            else
                lodComponent.ValueRW.CurrentLOD = 2;

        }
    }
}
