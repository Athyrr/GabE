using GabE.Module.ECS;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[UpdateInGroup(typeof(ECS_LifecycleSystemGroup))]
public partial struct ECS_BuildingBuilderSystem : ISystem
{
    //[BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        var e = state.EntityManager.CreateEntity(typeof(ECS_PositionFragment), typeof(ECS_BuildingFragment));
        state.EntityManager.SetComponentData(e, new ECS_PositionFragment
        {
            Position = new float3(0f, 0f, 0f)
        });

        state.EntityManager.SetComponentData(e, new ECS_BuildingFragment
        {
            Type = BuildingType.School,
            Capacicty = 100,
            Occupants = 0
        });

        Debug.LogWarning("Create school");
    }


    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        foreach (var (createBuildingTag, entity) in SystemAPI.Query<RefRO<ECS_CreateBuildingTag>>().WithEntityAccess())
        {
            var position = SystemAPI.GetComponentRO<ECS_PositionFragment>(entity).ValueRO;
            var buildingModel = SystemAPI.GetComponentRO<ECS_BuildingFragment>(entity).ValueRO;

            var createdBuilding = ecb.CreateEntity();
            ecb.AddComponent(createdBuilding, new ECS_BuildingFragment
            {
                Type = buildingModel.Type,
                Capacicty = buildingModel.Capacicty,
                Occupants = 0
            });
            ecb.AddComponent(createdBuilding, new ECS_PositionFragment
            {
                Position = position.Position
            });

            UnityEngine.Debug.Log($"Building: {buildingModel.Type}  |  position: {position.Position}");

            ecb.DestroyEntity(entity);
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}
