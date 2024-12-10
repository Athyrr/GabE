using GabE.Module.ECS;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

[UpdateInGroup(typeof(ECS_LifecycleSystemGroup))]
public partial struct ECS_BuildingBuilderSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        foreach (var (buildingFragment, entity) in SystemAPI.Query<RefRW<ECS_BuildingFragment>>()
                                                           .WithAll<ECS_CreateBuildingTag>()
                                                           .WithEntityAccess())
        {
            buildingFragment.ValueRW.BuildDuration -= SystemAPI.Time.DeltaTime;

            if (buildingFragment.ValueRO.BuildDuration <= 0)
            {
                var position = SystemAPI.GetComponentRO<ECS_PositionFragment>(entity).ValueRO;

                var createdBuilding = ecb.CreateEntity();
                ecb.AddComponent(createdBuilding, new ECS_BuildingFragment
                {
                    Type = buildingFragment.ValueRO.Type,
                    Capacicty = buildingFragment.ValueRO.Capacicty,
                    Occupants = 0
                });
                ecb.AddComponent(createdBuilding, new ECS_PositionFragment
                {
                    Position = position.Position
                });

                ecb.DestroyEntity(entity);
            }
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}
