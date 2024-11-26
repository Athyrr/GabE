using Unity.Entities;
using Unity.Burst;

using GabE.Module.ECS;


[BurstCompile]
[UpdateInGroup(typeof(InitializationSystemGroup))]
public partial struct ECS_Processor_InventoryManagerInitializer : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        var entity = state.EntityManager.CreateEntity();
        var buffer = state.EntityManager.AddBuffer<ECS_Frag_Resource>(entity);

        //@todo set initial resources in asset to read
        buffer.Add(new ECS_Frag_Resource { ResourceType = ResourceType.Wood, Quantity = 0 });
        buffer.Add(new ECS_Frag_Resource { ResourceType = ResourceType.Stone, Quantity = 0 });
        buffer.Add(new ECS_Frag_Resource { ResourceType = ResourceType.Food, Quantity = 0 });
    }
}
