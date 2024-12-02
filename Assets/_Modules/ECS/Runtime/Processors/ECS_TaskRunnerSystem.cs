using Unity.Burst;
using Unity.Entities;

namespace GabE.Module.ECS
{
    [UpdateInGroup(typeof(ECS_Group_Lifecycle))]

    partial struct ECS_Processor_TaskRunner : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            EntityManager entityManager = state.EntityManager;
            float delta = SystemAPI.Time.DeltaTime;

            foreach (var (task, building, entity) in SystemAPI.Query<RefRW<ECS_Frag_TaskProcess>, RefRW<ECS_Frag_Building>>().WithEntityAccess())
            {
                if (task.ValueRO.HasFinished)
                {
                    var buildingRequest = new ECS_Frag_BuildListener(entity);

                    entityManager.AddComponentData(entity, buildingRequest);
                    entityManager.RemoveComponent<ECS_Frag_TaskProcess>(entity);
                    continue;
                }

                task.ValueRW.Progression += delta / task.ValueRO.Duration;

                if (task.ValueRO.Progression >= 1)
                    task.ValueRW.HasFinished = true;
            }
        }
    }
}
