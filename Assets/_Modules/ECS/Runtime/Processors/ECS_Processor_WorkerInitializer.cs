using System;

using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

using GabE.Module.ECS;


[UpdateInGroup(typeof(ECS_Group_Initialization))]
[DisableAutoCreation]
public partial struct ECS_Processor_WorkerInitializer : ISystem
{
    private bool isInitialized;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        isInitialized = false;
    }

    public void OnUpdate(ref SystemState state)
    {
        if (isInitialized) return;

        int workerCount = 100;
        var workTypes = new NativeArray<ECS_WorkerFragment.WorkType>
        (
            (ECS_WorkerFragment.WorkType[])Enum.GetValues(typeof(ECS_WorkerFragment.WorkType)),
            Allocator.TempJob
        );

        var ecbSystem = state.World.GetExistingSystemManaged<BeginSimulationEntityCommandBufferSystem>();
        var ecb = ecbSystem.CreateCommandBuffer().AsParallelWriter();

        var jobHandle = new WorkerInitializationJob
        {
            EntityCount = workerCount,
            WorkTypes = workTypes,
            Seed = (uint)(SystemAPI.Time.ElapsedTime * 1000 + 1),
            CommandBuffer = ecb
        }.ScheduleParallel(workerCount, 64, state.Dependency);

        ecbSystem.AddJobHandleForProducer(state.Dependency);
        isInitialized = true;
    }

    [BurstCompile]
    private partial struct WorkerInitializationJob : IJobFor
    {
        public int EntityCount;
        [ReadOnly] public NativeArray<ECS_WorkerFragment.WorkType> WorkTypes;
        public uint Seed;
        public EntityCommandBuffer.ParallelWriter CommandBuffer;

        public void Execute(int index)
        {
            var random = new Unity.Mathematics.Random(Seed + (uint)index);

            var entity = CommandBuffer.CreateEntity(index);

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
                Position = random.NextFloat3()
            });

            CommandBuffer.AddComponent(index, entity, new ECS_Frag_Velocity
            {
                Value = 5
            });

            CommandBuffer.AddComponent(index, entity, new ECS_Frag_TargetPosition
            {
                Position = random.NextFloat3()
            });
        }

    }
}
