using System;

using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

using GabE.Module.ECS;


[UpdateInGroup(typeof(ECS_Group_Initialization))]
public partial struct ECS_WorkerInitializationSystem : ISystem
{
    private bool isInitialized;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        isInitialized = false;
    }

    //[BurstDiscard]
    public void OnUpdate(ref SystemState state)
    {
        if (isInitialized)
            return;

        int workerCount = 512/**1024*/;

        // Get Work types array
        var workTypes = new NativeArray<ECS_WorkerFragment.WorkType>
        (
            (ECS_WorkerFragment.WorkType[])Enum.GetValues(typeof(ECS_WorkerFragment.WorkType)),
            Allocator.TempJob
        );

        // Map WorkType -> MaterialID
        var materialIDs = new NativeArray<byte>(new byte[]
        {
        0, // FoodHarvester
        1, // Tiberman
        2, // Miner
        3, // Mason
        4, // Vagabond
        }, Allocator.TempJob);


        EntityCommandBuffer commandBuffer = new EntityCommandBuffer(Allocator.TempJob);
        var writter = commandBuffer.AsParallelWriter();

        var workerInitJob = new WorkerInitializationJob
        {
            Seed = (uint)(SystemAPI.Time.DeltaTime + 1),
            EntityCount = workerCount,

            WorkTypes = workTypes,
            MaterialIDs = materialIDs,

            CommandBuffer = writter
        };

        state.Dependency = workerInitJob.ScheduleParallel(workerCount, 64, state.Dependency);
        state.Dependency.Complete();

        commandBuffer.Playback(state.EntityManager);
        commandBuffer.Dispose();

        workTypes.Dispose();
        materialIDs.Dispose();

        isInitialized = true;
    }

    [BurstCompile(CompileSynchronously = true, FloatPrecision = FloatPrecision.Low, OptimizeFor = OptimizeFor.FastCompilation)]
    private partial struct WorkerInitializationJob : IJobFor
    {
        public uint Seed;
        public int EntityCount;

        [ReadOnly] public NativeArray<ECS_WorkerFragment.WorkType> WorkTypes;
        [ReadOnly] public NativeArray<byte> MaterialIDs;

        public EntityCommandBuffer.ParallelWriter CommandBuffer;


        public void Execute(int index)
        {
            var random = new Unity.Mathematics.Random(Seed + (uint)index);
            var entity = CommandBuffer.CreateEntity(index);

            var x = random.NextFloat(-50, 50);
            var y = random.NextFloat(1, 5);
            var z = random.NextFloat(-50, 50);

            var workType = WorkTypes[random.NextInt(WorkTypes.Length)];
            var materialID = MaterialIDs[(int)workType];


            CommandBuffer.AddComponent(index, entity, new ECS_PersonFragment
            {
                Age = random.NextInt(18, 80),
                IsExhausted = false,
                IsHappy = true,
                IsAlive = true
            });

            CommandBuffer.AddComponent(index, entity, new ECS_WorkerFragment
            {
                Work = workType,
                HoldingCapacity= 1,
                IsWorking = false
            });

            CommandBuffer.AddComponent(index, entity, new ECS_PositionFragment
            {
                Position = new float3(x, y, z)
            });

            CommandBuffer.AddComponent(index, entity, new ECS_Frag_Velocity
            {
                Value = 10
            });

            CommandBuffer.AddComponent(index, entity, new ECS_WorkerMaterialFragment
            {
                MaterialID = materialID
            });

            CommandBuffer.AddComponent(index, entity, new ECS_MeshLODGroupFragment
            {
                CurrentLOD = 0
            });
        }
    }
}
