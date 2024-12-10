using Unity.Entities;
using Unity.Mathematics;

using GabE.Module.ECS;


[UpdateInGroup(typeof(InitializationSystemGroup))]
public partial struct ECS_ResourceZoneInitializationSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        for (int i = 0; i < 3; i++)
        {
            var entity = state.EntityManager.CreateEntity(
                typeof(ECS_PositionFragment),
                typeof(ECS_ResourceZoneFragment)
            );

            ECS_PositionFragment position = new ECS_PositionFragment { Position = new float3(i * 10f, 15f, 0f) };
            state.EntityManager.SetComponentData(entity, position);

            ECS_ResourceZoneFragment resource = i switch
            {
                0 => new ECS_ResourceZoneFragment { Type = ResourceType.Wood },
                1 => new ECS_ResourceZoneFragment { Type = ResourceType.Stone },
                2 => new ECS_ResourceZoneFragment { Type = ResourceType.Food },
                _ => new ECS_ResourceZoneFragment { Type = ResourceType.Food }
            };
            state.EntityManager.SetComponentData<ECS_ResourceZoneFragment>(entity, resource);
        }
    }
}



