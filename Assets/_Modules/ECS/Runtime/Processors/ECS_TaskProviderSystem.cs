using Unity.Entities;
using Unity.Collections;
using Unity.Burst;
using GabE.Module.ECS;

[UpdateInGroup(typeof(ECS_LifecycleSystemGroup))]
public partial struct ECS_TaskProviderSystem : ISystem
{
    private EntityQuery _workerQuery;
    private EntityQuery _resourceQuery;

    public void OnCreate(ref SystemState state)
    {
        // Available workers query 
        _workerQuery = state.GetEntityQuery(
            ComponentType.ReadOnly<ECS_WorkerFragment>(),
            ComponentType.Exclude<ECS_Frag_TargetPosition>()
        );

        // Resource zones query
        _resourceQuery = state.GetEntityQuery(
            ComponentType.ReadOnly<ECS_Frag_Position>(),
            ComponentType.ReadOnly<ECS_ResourceZoneFragment>()
        );
    }

    [BurstCompile]
    private partial struct TaskProviderJob : IJobEntity
    {
        [ReadOnly] public NativeArray<ECS_Frag_Position> ResourcePositions;
        [ReadOnly] public NativeArray<ECS_ResourceZoneFragment> ResourceTypes;
        public EntityCommandBuffer.ParallelWriter CommandBuffer;

        public void Execute(Entity entity, ref ECS_WorkerFragment worker)
        {
            //if worker holding resources he goes to storage

            if (worker.IsWorking || worker.Work == ECS_WorkerFragment.WorkType.None)
                return;

            ResourceType targetType = worker.Work switch
            {
                ECS_WorkerFragment.WorkType.FoodHarvester => ResourceType.Food,
                ECS_WorkerFragment.WorkType.Tiberman => ResourceType.Wood,
                ECS_WorkerFragment.WorkType.Miner => ResourceType.Stone,
                _ => ResourceType.Food
            };

            for (int i = 0; i < ResourceTypes.Length; i++)
            {
                if (ResourceTypes[i].Type == targetType)
                {
                    CommandBuffer.AddComponent(entity.Index, entity, new ECS_Frag_TargetPosition
                    {
                        Position = ResourcePositions[i].Position
                    });

                    worker.IsWorking = true;
                    break;
                }
            }
        }
    }

    public void OnUpdate(ref SystemState state)
    {
        var ecbSystem = state.World.GetExistingSystemManaged<BeginSimulationEntityCommandBufferSystem>();
        var commandBuffer = ecbSystem.CreateCommandBuffer().AsParallelWriter();

        var resourcePositions = _resourceQuery.ToComponentDataArray<ECS_Frag_Position>(Allocator.TempJob);
        var resourceTypes = _resourceQuery.ToComponentDataArray<ECS_ResourceZoneFragment>(Allocator.TempJob);

        var job = new TaskProviderJob
        {
            ResourcePositions = resourcePositions,
            ResourceTypes = resourceTypes,
            CommandBuffer = commandBuffer
        };

        state.Dependency = job.ScheduleParallel(_workerQuery, state.Dependency);

        ecbSystem.AddJobHandleForProducer(state.Dependency);

        resourcePositions.Dispose();
        resourceTypes.Dispose();
    }
}
