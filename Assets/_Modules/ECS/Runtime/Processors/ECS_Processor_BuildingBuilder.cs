using GabE.Module.ECS;
using Unity.Entities;

public partial struct ECS_BuildingBuilderSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        EntityManager entityManager = state.EntityManager;

        foreach (var (buildRequest, entity) in SystemAPI.Query<RefRO<ECS_Frag_BuildListener>>().WithEntityAccess())
        {
            Entity buildingEntity = buildRequest.ValueRO.BuildingEntity;

            if (entityManager.HasComponent<ECS_Frag_Building>(buildingEntity))
            {
                var buildingFrag = entityManager.GetComponentData<ECS_Frag_Building>(buildingEntity);
                entityManager.SetComponentData(buildingEntity, buildingFrag);
            }

            // @todo TW use dependecnies instead
            entityManager.RemoveComponent<ECS_Frag_BuildListener>(entity);
        }
    }
}
