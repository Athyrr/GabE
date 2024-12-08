using GabE.Module.ECS;
using Unity.Burst.Intrinsics;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile]
[UpdateInGroup(typeof(ECS_Group_ResourceManagement))]
public partial struct ECS_StorageManagerSystem : ISystem
{
    private Entity _storageEntity;
    private EntityQuery _workerQuery;
    private ECS_PositionFragment _storagePosition;
    private NativeList<ECS_ResourceStorageFragment> _globalStorage;
    private int _lastProcessedDay;

    // Declare ComponentTypeHandles
    private ComponentTypeHandle<ECS_WorkerFragment> _workerHandle;
    private ComponentTypeHandle<ECS_PositionFragment> _positionHandle;

    public void OnCreate(ref SystemState state)
    {
        if (!SystemAPI.HasSingleton<ECS_ResourceStorageFragment>())
        {
            Debug.Log("Create new storage");
            var entity = state.EntityManager.CreateEntity();

            state.EntityManager.AddComponent<ECS_PositionFragment>(entity);
            state.EntityManager.SetComponentData<ECS_PositionFragment>(entity, new ECS_PositionFragment() { Position = new float3(0, 0, z: 15) });

            DynamicBuffer<ECS_ResourceStorageFragment> buffer = state.EntityManager.AddBuffer<ECS_ResourceStorageFragment>(entity);
            //@todo set initial resources in asset to read
            buffer.Add(new ECS_ResourceStorageFragment { Type = ResourceType.Wood, Quantity = 10 });
            buffer.Add(new ECS_ResourceStorageFragment { Type = ResourceType.Stone, Quantity = 0 });
            buffer.Add(new ECS_ResourceStorageFragment { Type = ResourceType.Food, Quantity = 0 });
        }

        _storageEntity = SystemAPI.GetSingletonEntity<ECS_ResourceStorageFragment>();

        _globalStorage = new NativeList<ECS_ResourceStorageFragment>(1024 * 1024, Allocator.Persistent);

        _workerQuery = state.GetEntityQuery(
            ComponentType.ReadOnly<ECS_WorkerFragment>(),
            ComponentType.ReadOnly<ECS_PositionFragment>()
        );

        _storagePosition = state.EntityManager.GetComponentData<ECS_PositionFragment>(_storageEntity);

        // Initialize type handles
        _workerHandle = state.GetComponentTypeHandle<ECS_WorkerFragment>(true);
        _positionHandle = state.GetComponentTypeHandle<ECS_PositionFragment>(true);
    }

    public void OnDestroy(ref SystemState state)
    {
        if (_globalStorage.IsCreated)
            _globalStorage.Dispose();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // Update the component type handles
        _workerHandle.Update(ref state);
        _positionHandle.Update(ref state);

        var lifecycle = SystemAPI.GetSingleton<ECS_GlobalLifecyleFragment>();
        int currentDay = lifecycle.DayCount;

        var addResourceJob = new AddResourceJob
        {
            StoragePosition = _storagePosition.Position,
            StorageBuffer = _globalStorage.AsParallelWriter(),
            WorkerHandle = _workerHandle,
            PositionHandle = _positionHandle
        };

        state.Dependency = addResourceJob.ScheduleParallel(_workerQuery, state.Dependency);

        if (currentDay != _lastProcessedDay && currentDay != 0)
        {
            _lastProcessedDay = currentDay;

            var consumeResourceJob = new ConsumeResourceJob
            {
                ResourceType = ResourceType.Food,
                Quantity = lifecycle.Population,
                StorageBuffer = _globalStorage
            };

            state.Dependency = consumeResourceJob.Schedule(state.Dependency);
        }

        state.Dependency.Complete();

        //for (int i = 0; i < _globalStorage.Length; i++)
        //{
        //    var resource = _globalStorage[i];
        //    Debug.Log($"Resource: {resource.Type}, Quantity: {resource.Quantity}");
        //}
    }

    [BurstCompile]
    private struct AddResourceJob : IJobChunk
    {
        public float3 StoragePosition;

        [NativeDisableParallelForRestriction]
        public NativeList<ECS_ResourceStorageFragment>.ParallelWriter StorageBuffer;

        [ReadOnly] public ComponentTypeHandle<ECS_WorkerFragment> WorkerHandle;
        [ReadOnly] public ComponentTypeHandle<ECS_PositionFragment> PositionHandle;

        public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
        {
            var workers = chunk.GetNativeArray(ref WorkerHandle);
            var positions = chunk.GetNativeArray(ref PositionHandle);

            for (int i = 0; i < chunk.Count; i++)
            {
                var worker = workers[i];
                var position = positions[i];

                if (worker.HoldResourcesAmount <= 0 || math.distance(position.Position, StoragePosition) > 1.0f)
                    continue;

                StorageBuffer.AddNoResize(new ECS_ResourceStorageFragment
                {
                    Type = worker.GetResourceType(worker.Work),
                    Quantity = worker.HoldResourcesAmount
                });
            }
        }
    }

    [BurstCompile]
    private struct ConsumeResourceJob : IJob
    {
        public ResourceType ResourceType;
        public int Quantity;

        [NativeDisableParallelForRestriction]
        public NativeList<ECS_ResourceStorageFragment> StorageBuffer;

        public void Execute()
        {
            for (int i = 0; i < StorageBuffer.Length; i++)
            {
                if (StorageBuffer[i].Type != ResourceType)
                    continue;

                StorageBuffer[i] = new ECS_ResourceStorageFragment
                {
                    Type = StorageBuffer[i].Type,
                    Quantity = math.max(0, StorageBuffer[i].Quantity - Quantity)
                };
                break;
            }
        }
    }
}
