using System;

using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

using GabE.Module.ECS;
using Unity.Mathematics;


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

        var workTypes = new NativeArray<ECS_WorkerFragment.WorkType>
        (
            (ECS_WorkerFragment.WorkType[])Enum.GetValues(typeof(ECS_WorkerFragment.WorkType)),
            Allocator.TempJob
        );

        EntityCommandBuffer commandBuffer = new EntityCommandBuffer(Allocator.TempJob);
        var writter = commandBuffer.AsParallelWriter();

        var workerInitJob = new WorkerInitializationJob
        {
            EntityCount = workerCount,
            WorkTypes = workTypes,
            Seed = (uint)(SystemAPI.Time.DeltaTime + 1),
            CommandBuffer = writter
        };

        state.Dependency = workerInitJob.ScheduleParallel(workerCount, 64, state.Dependency);
        state.Dependency.Complete();

        commandBuffer.Playback(state.EntityManager);
        commandBuffer.Dispose();

        workTypes.Dispose();

        isInitialized = true;
    }

    [BurstCompile(CompileSynchronously = true, FloatPrecision = FloatPrecision.Low, OptimizeFor = OptimizeFor.FastCompilation)]
    private partial struct WorkerInitializationJob : IJobFor
    {
        public uint Seed;
        public int EntityCount;
        [ReadOnly] public NativeArray<ECS_WorkerFragment.WorkType> WorkTypes;
        public EntityCommandBuffer.ParallelWriter CommandBuffer;


        public void Execute(int index) 
        {
            var random = new Unity.Mathematics.Random(Seed + (uint)index);
            var entity = CommandBuffer.CreateEntity(index);

            var x = random.NextFloat(-50, 50);
            var y = random.NextFloat(1, 5);
            var z = random.NextFloat(-50, 50);


            CommandBuffer.AddComponent(index, entity, new ECS_PersonFragment
            {
                Age = random.NextInt(18, 80),
                IsExhausted = false,
                IsHappy = true,
                IsAlive = true
            });

            CommandBuffer.AddComponent(index, entity, new ECS_WorkerFragment
            {
                Work = WorkTypes[random.NextInt(WorkTypes.Length)],
                IsWorking = false
            });

            CommandBuffer.AddComponent(index, entity, new ECS_Frag_Position
            {
                Position = new float3(x, y, z)
            });

            CommandBuffer.AddComponent(index, entity, new ECS_Frag_Velocity
            {
                Value = 10
            });
        }

    }
}
