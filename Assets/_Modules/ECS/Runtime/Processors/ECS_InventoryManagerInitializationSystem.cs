using Unity.Entities;
using Unity.Burst;

using GabE.Module.ECS;
using Unity.Mathematics;


[BurstCompile]
[UpdateInGroup(typeof(ECS_Group_Initialization))]
public partial struct ECS_InventoryManagerInitializationSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        var entity = state.EntityManager.CreateEntity();

        state.EntityManager.AddComponent<ECS_Frag_Position>(entity);
        state.EntityManager.SetComponentData<ECS_Frag_Position>(entity, new ECS_Frag_Position() { Position = new float3(0,0,z: 15) });

       DynamicBuffer<ECS_ResourceStorageFragment> buffer = state.EntityManager.AddBuffer<ECS_ResourceStorageFragment>(entity);
        //@todo set initial resources in asset to read
        buffer.Add(new ECS_ResourceStorageFragment { Type = ResourceType.Wood, Quantity = 0 });
        buffer.Add(new ECS_ResourceStorageFragment { Type = ResourceType.Stone, Quantity = 0 });
        buffer.Add(new ECS_ResourceStorageFragment { Type = ResourceType.Food, Quantity = 0 });
    }
}
