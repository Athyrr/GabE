using Unity.Entities;
using Unity.Collections;
using GabE.Module.ECS;
using Unity.Burst;

[UpdateInGroup(typeof(ECS_LifecycleSystemGroup))]
[UpdateAfter(typeof(ECS_StorageManagerSystem))]
public partial struct ECS_TaskProviderSystem : ISystem
{
    private EntityQuery _workerQuery;
    private EntityQuery _resourceQuery;
    private ECS_PositionFragment _storagePosition;

    [BurstCompile]
    private partial struct ProvideTaskJob : IJobEntity
    {
        [ReadOnly] public NativeArray<ECS_PositionFragment> ResourcePositions;
        [ReadOnly] public NativeArray<ECS_ResourceZoneFragment> ResourceTypes;

        [ReadOnly] public ECS_PositionFragment StoragePosition;

        public EntityCommandBuffer.ParallelWriter CommandBuffer;

        public void Execute(Entity entity, [EntityIndexInQuery] int index, ref ECS_WorkerFragment worker)
        {
            // Handle deposit logic
            if (worker.HoldResourcesAmount >= worker.HoldingCapacity)
            {
                CommandBuffer.SetComponent(index, entity, new ECS_Frag_TargetPosition
                {
                    Position = StoragePosition.Position
                });
                worker.IsRetrieving = true;
                return;
            }

            // Skip workers already retrieving resources
            if (worker.IsRetrieving || worker.IsWorking || worker.Work == ECS_WorkerFragment.WorkType.Vagabond)
                return;

            // Determine resource type
            ResourceType targetType = worker.Work switch
            {
                ECS_WorkerFragment.WorkType.FoodHarvester => ResourceType.Food,
                ECS_WorkerFragment.WorkType.Tiberman => ResourceType.Wood,
                ECS_WorkerFragment.WorkType.Miner => ResourceType.Stone,
                _ => ResourceType.None
            };

            // Assign a new resource zone if matching type found
            for (int i = 0; i < ResourceTypes.Length; i++)
            {
                if (ResourceTypes[i].Type == targetType)
                {
                    CommandBuffer.AddComponent(index, entity, new ECS_Frag_TargetPosition
                    {
                        Position = ResourcePositions[i].Position
                    });
                    worker.IsRetrieving = false;
                    break;
                }
            }
        }
    }

    public void OnCreate(ref SystemState state)
    {
        // Initialize queries
        _workerQuery = state.GetEntityQuery(ComponentType.ReadWrite<ECS_WorkerFragment>());
        _resourceQuery = state.GetEntityQuery(ComponentType.ReadOnly<ECS_PositionFragment>(), ComponentType.ReadOnly<ECS_ResourceZoneFragment>());
        _storagePosition = GetStoragePosition(ref state);
    }

    private ECS_PositionFragment GetStoragePosition(ref SystemState state)
    {
        var storageQuery = state.GetEntityQuery(ComponentType.ReadOnly<ECS_ResourceStorageFragment>());
        Entity storageEntity = storageQuery.GetSingletonEntity();
        return state.EntityManager.GetComponentData<ECS_PositionFragment>(storageEntity);
    }

    [BurstCompile(OptimizeFor = OptimizeFor.Performance)]
    public void OnUpdate(ref SystemState state)
    {
        var resourcePositions = _resourceQuery.ToComponentDataArray<ECS_PositionFragment>(Allocator.TempJob);
        var resourceTypes = _resourceQuery.ToComponentDataArray<ECS_ResourceZoneFragment>(Allocator.TempJob);

        var commandBuffer = new EntityCommandBuffer(Allocator.TempJob);
        var writer = commandBuffer.AsParallelWriter();
        var taskProviderJob = new ProvideTaskJob
        {
            ResourcePositions = resourcePositions,
            ResourceTypes = resourceTypes,
            StoragePosition = _storagePosition,
            CommandBuffer = writer
        };

        state.Dependency = taskProviderJob.ScheduleParallel(_workerQuery, state.Dependency);

        state.Dependency.Complete();
        commandBuffer.Playback(state.EntityManager);
        commandBuffer.Dispose();

        resourcePositions.Dispose();
        resourceTypes.Dispose();
    }
}
