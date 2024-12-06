using GabE.Module.ECS;
using Unity.Burst;
using Unity.Entities;

[UpdateInGroup(typeof(ECS_LifecycleSystemGroup))]
public partial struct ECS_BuildingBuilderSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {

    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntityManager entityManager = state.EntityManager;

        foreach (var (buildRequest, entity) in SystemAPI.Query<RefRO<ECS_BuildListenerFragment>>().WithEntityAccess())
        {
            Entity buildingEntity = buildRequest.ValueRO.BuildingEntity;

            if (entityManager.HasComponent<ECS_BuildingFragment>(buildingEntity))
            {
                var buildingFrag = entityManager.GetComponentData<ECS_BuildingFragment>(buildingEntity);
                entityManager.SetComponentData(buildingEntity, buildingFrag);
            }

            // @todo TW use dependecnies instead
            entityManager.RemoveComponent<ECS_BuildListenerFragment>(entity);
        }
    }
}
