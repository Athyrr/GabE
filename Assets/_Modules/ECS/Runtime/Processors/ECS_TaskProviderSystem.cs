using Unity.Entities;
using Unity.Collections;
using Unity.Burst;
using GabE.Module.ECS;


[UpdateInGroup(typeof(ECS_LifecycleSystemGroup))]
public partial struct ECS_TaskProviderSystem : ISystem
{
    private EntityQuery _workerQuery;
    private EntityQuery _resourceQuery;


    [BurstCompile]
    private partial struct TaskProviderJob : IJobEntity //@todo il en pense quoi David de la boucle dans le job
    {
        [ReadOnly] public NativeArray<ECS_PositionFragment> ResourcePositions;
        [ReadOnly] public NativeArray<ECS_ResourceZoneFragment> ResourceTypes;
        public EntityCommandBuffer.ParallelWriter CommandBuffer;

        public void Execute(Entity entity, ref ECS_WorkerFragment worker)
        {
            //if worker holding resources he goes to storage postion

            if (worker.IsWorking || worker.Work == ECS_WorkerFragment.WorkType.Vagabond)
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

    public void OnCreate(ref SystemState state)
    {
        // Available workers query 
        _workerQuery = state.GetEntityQuery(
            ComponentType.ReadOnly<ECS_WorkerFragment>(),
            ComponentType.Exclude<ECS_Frag_TargetPosition>()
        );

        // Resource zones query
        _resourceQuery = state.GetEntityQuery(
            ComponentType.ReadOnly<ECS_PositionFragment>(),
            ComponentType.ReadOnly<ECS_ResourceZoneFragment>()
        );
    }

    //[BurstCompile(OptimizeFor = OptimizeFor.Performance)]
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer commandBuffer = new EntityCommandBuffer(Allocator.TempJob);
        var writter = commandBuffer.AsParallelWriter();

        var resourcePositions = _resourceQuery.ToComponentDataArray<ECS_PositionFragment>(Allocator.TempJob);
        var resourceTypes = _resourceQuery.ToComponentDataArray<ECS_ResourceZoneFragment>(Allocator.TempJob);

        var taskProviderjob = new TaskProviderJob
        {
            ResourcePositions = resourcePositions,
            ResourceTypes = resourceTypes,
            CommandBuffer = writter
        };

        state.Dependency = taskProviderjob.ScheduleParallel(_workerQuery, state.Dependency);
        state.Dependency.Complete();

        commandBuffer.Playback(state.EntityManager);
        commandBuffer.Dispose();

        resourcePositions.Dispose();
        resourceTypes.Dispose();
    }
}
