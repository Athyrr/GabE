using GabE.Module.ECS;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using System;
using Unity.Burst.Intrinsics;

[UpdateInGroup(typeof(ECS_LifecycleSystemGroup))]
public partial struct ECS_StorageManagerSystem : ISystem
{
    private Entity _storageEntity;
    private ECS_PositionFragment _storagePosition;
    private EntityQuery _workerQuery;
    private int _lastProcessedDay;

    private NativeArray<ECS_ResourceStorageFragment> _globalStorage;

    // Handles for components
    private ComponentTypeHandle<ECS_WorkerFragment> _workerHandle;
    private ComponentTypeHandle<ECS_PositionFragment> _positionHandle;

    public void OnCreate(ref SystemState state)
    {
        // Create or retrieve the storage entity
        if (!SystemAPI.HasSingleton<ECS_ResourceStorageFragment>())
        {
            var entity = state.EntityManager.CreateEntity();
            state.EntityManager.AddComponentData(entity, new ECS_PositionFragment { Position = new float3(-10, 15, 15) });

            _globalStorage = new NativeArray<ECS_ResourceStorageFragment>(3, Allocator.Persistent);
            _globalStorage[0] = new ECS_ResourceStorageFragment { Type = ResourceType.Wood, Quantity = 10 };
            _globalStorage[1] = new ECS_ResourceStorageFragment { Type = ResourceType.Stone, Quantity = 0 };
            _globalStorage[2] = new ECS_ResourceStorageFragment { Type = ResourceType.Food, Quantity = 0 };

            state.EntityManager.AddComponentData(entity, new ECS_ResourceStorageFragment { Type = ResourceType.Wood, Quantity = 10 });
        }

        // Retrieve the storage entity
        _storageEntity = SystemAPI.GetSingletonEntity<ECS_ResourceStorageFragment>();
        _storagePosition = state.EntityManager.GetComponentData<ECS_PositionFragment>(_storageEntity);

        // Set up the worker query and component handles
        _workerQuery = state.GetEntityQuery(
            ComponentType.ReadOnly<ECS_WorkerFragment>(),
            ComponentType.ReadOnly<ECS_PositionFragment>()
        );

        _workerHandle = state.GetComponentTypeHandle<ECS_WorkerFragment>(true);
        _positionHandle = state.GetComponentTypeHandle<ECS_PositionFragment>(true);
    }

    public void OnDestroy(ref SystemState state)
    {
        if (_globalStorage.IsCreated)
        {
            _globalStorage.Dispose();
        }
    }

    public void OnUpdate(ref SystemState state)
    {
        //// Update component handles
        //_workerHandle.Update(ref state);
        //_positionHandle.Update(ref state);

        //var lifecycle = SystemAPI.GetSingleton<ECS_GlobalLifecyleFragment>();
        //int currentDay = lifecycle.DayCount;

        //// Collect resources from workers
        //var addResourceJob = new AddResourceJob
        //{
        //    StoragePosition = _storagePosition.Position,
        //    Storage = _globalStorage,
        //    WorkerHandle = _workerHandle,
        //    PositionHandle = _positionHandle
        //};
        //state.Dependency = addResourceJob.ScheduleParallel(_workerQuery, state.Dependency);

        //// Consume resources once per day
        //if (currentDay != _lastProcessedDay && currentDay > 0)
        //{
        //    _lastProcessedDay = currentDay;

        //    var consumeResourceJob = new ConsumeResourceJob
        //    {
        //        ResourceType = ResourceType.Food,
        //        Quantity = lifecycle.Population,
        //        Storage = _globalStorage
        //    };
        //    state.Dependency = consumeResourceJob.Schedule(state.Dependency);
        //}

        //state.Dependency.Complete();

        //// Log the resource quantities
        //for (int i = 0; i < _globalStorage.Length; i++)
        //{
        //    var resource = _globalStorage[i];
        //    Debug.Log($"Resource: {resource.Type}, Quantity: {resource.Quantity}");
        //}
    }

    private struct AddResourceJob : IJobChunk
    {
        public float3 StoragePosition;

        public NativeArray<ECS_ResourceStorageFragment> Storage;

        [ReadOnly] public ComponentTypeHandle<ECS_WorkerFragment> WorkerHandle;
        [ReadOnly] public ComponentTypeHandle<ECS_PositionFragment> PositionHandle;

        public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
        {
            if (!chunk.Has(ref WorkerHandle) || !chunk.Has(ref PositionHandle))
                return;

            var workers = chunk.GetNativeArray(ref WorkerHandle);
            var positions = chunk.GetNativeArray(ref PositionHandle);

            // Iterate over the workers in the chunk
            for (int i = 0; i < chunk.Count; i++)
            {
                var worker = workers[i];
                var position = positions[i];

                // Only process workers that have resources and are close to storage
                if (worker.HoldResourcesAmount <= 0 || math.distance(position.Position, StoragePosition) > 1.0f)
                    continue;

                // Iterate through the storage array to find the correct resource type
                for (int j = 0; j < Storage.Length; j++)
                {
                    if (Storage[j].Type == worker.GetResourceType(worker.Work))
                    {
                        // Add the worker's resource to the storage quantity
                        Storage[j] = new ECS_ResourceStorageFragment
                        {
                            Type = Storage[j].Type,
                            Quantity = Storage[j].Quantity + worker.HoldResourcesAmount
                        };
                        break;
                    }
                }

                Debug.Assert(Enum.IsDefined(typeof(ResourceType), worker.GetResourceType(worker.Work)), "Invalid ResourceType detected.");
            }
        }
    }

    private struct ConsumeResourceJob : IJob
    {
        public ResourceType ResourceType;
        public int Quantity;

        public NativeArray<ECS_ResourceStorageFragment> Storage;

        public void Execute()
        {
            for (int i = 0; i < Storage.Length; i++)
            {
                if (Storage[i].Type != ResourceType)
                    continue;

                Storage[i] = new ECS_ResourceStorageFragment
                {
                    Type = Storage[i].Type,
                    Quantity = math.max(0, Storage[i].Quantity - Quantity)
                };
                break;
            }
        }
    }
}
