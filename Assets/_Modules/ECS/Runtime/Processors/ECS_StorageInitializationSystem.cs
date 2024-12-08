using Unity.Entities;
using Unity.Burst;
using Unity.Mathematics;

using GabE.Module.ECS;


[BurstCompile]
[UpdateInGroup(typeof(ECS_Group_Initialization))]
public partial struct ECS_StorageInitializationSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        var entity = state.EntityManager.CreateEntity();

        state.EntityManager.AddComponent<ECS_PositionFragment>(entity);
        state.EntityManager.SetComponentData<ECS_PositionFragment>(entity, new ECS_PositionFragment() { Position = new float3(0,0,z: 15) });

       DynamicBuffer<ECS_ResourceStorageFragment> buffer = state.EntityManager.AddBuffer<ECS_ResourceStorageFragment>(entity);
        //@todo set initial resources in asset to read
        buffer.Add(new ECS_ResourceStorageFragment { Type = ResourceType.Wood, Quantity = 0 });
        buffer.Add(new ECS_ResourceStorageFragment { Type = ResourceType.Stone, Quantity = 0 });
        buffer.Add(new ECS_ResourceStorageFragment { Type = ResourceType.Food, Quantity = 0 });
    }
}
