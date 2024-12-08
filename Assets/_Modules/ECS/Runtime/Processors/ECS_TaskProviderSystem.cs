using Unity.Entities;
using Unity.Collections;
using GabE.Module.ECS;
using Unity.Burst;


[UpdateInGroup(typeof(ECS_LifecycleSystemGroup))]
[UpdateAfter(typeof(ECS_TaskProgressionSystem))]
public partial struct ECS_TaskProviderSystem : ISystem
{
    private EntityQuery _workerQuery;
    private EntityQuery _resourceQuery;

    private ECS_PositionFragment _storagePosition;


    [BurstCompile]
    private partial struct ProvideTaskJob : IJobEntity //@todo il en pense quoi David de la boucle dans le job
    {
        [ReadOnly] public NativeArray<ECS_PositionFragment> ResourcePositions;
        [ReadOnly] public NativeArray<ECS_ResourceZoneFragment> ResourceTypes;

        [ReadOnly] public ECS_PositionFragment StoragePosition;

        public EntityCommandBuffer.ParallelWriter CommandBuffer;

        public void Execute(Entity entity, [EntityIndexInQuery] int index, ref ECS_WorkerFragment worker)
        {
            if (!worker.IsRetrieving && !worker.IsWorking && worker.HoldResourcesAmount >= worker.HoldingCapacity)
            {
                CommandBuffer.SetComponent(entity.Index, entity, new ECS_Frag_TargetPosition
                {
                    Position = StoragePosition.Position
                });

                worker.IsRetrieving = true;
            }

            if (worker.IsRetrieving)
                return;

            if (worker.IsWorking || worker.Work == ECS_WorkerFragment.WorkType.Vagabond)
                return;

            ResourceType targetType = worker.Work switch
            {
                ECS_WorkerFragment.WorkType.FoodHarvester => ResourceType.Food,
                ECS_WorkerFragment.WorkType.Tiberman => ResourceType.Wood,
                ECS_WorkerFragment.WorkType.Miner => ResourceType.Stone,
                ECS_WorkerFragment.WorkType.Mason => ResourceType.None,
                _ => ResourceType.None
            };

            for (int i = 0; i < ResourceTypes.Length; i++)
            {
                if (ResourceTypes[i].Type == targetType)
                {
                    CommandBuffer.AddComponent(entity.Index, entity, new ECS_Frag_TargetPosition
                    {
                        Position = ResourcePositions[i].Position
                    });

                    worker.IsRetrieving = false;

                    break;
                }
            }
        }
    }


    //[BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        // Available workers query 
        _workerQuery = state.GetEntityQuery(
            ComponentType.ReadOnly<ECS_WorkerFragment>()
        );

        // Resource zones query
        _resourceQuery = state.GetEntityQuery(
            ComponentType.ReadOnly<ECS_PositionFragment>(),
            ComponentType.ReadOnly<ECS_ResourceZoneFragment>()
        );

        // Get storage position
        _storagePosition = GetStoragePosition(ref state);
    }

    private ECS_PositionFragment GetStoragePosition(ref SystemState state)
    {
        var storageQuery = state.GetEntityQuery(
            ComponentType.ReadOnly<ECS_ResourceStorageFragment>()
        );

        Entity storageEntity = storageQuery.GetSingletonEntity();
        ECS_PositionFragment storagePosition = state.EntityManager.GetComponentData<ECS_PositionFragment>(storageEntity);

        return storagePosition;
    }


    [BurstCompile(OptimizeFor = OptimizeFor.Performance)]
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer commandBuffer = new EntityCommandBuffer(Allocator.TempJob);
        var writter = commandBuffer.AsParallelWriter();

        var resourcePositions = _resourceQuery.ToComponentDataArray<ECS_PositionFragment>(Allocator.TempJob);
        var resourceTypes = _resourceQuery.ToComponentDataArray<ECS_ResourceZoneFragment>(Allocator.TempJob);

        var taskProviderjob = new ProvideTaskJob
        {
            ResourcePositions = resourcePositions,
            ResourceTypes = resourceTypes,
            StoragePosition = _storagePosition,
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
