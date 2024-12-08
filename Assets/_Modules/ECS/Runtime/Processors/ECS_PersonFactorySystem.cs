using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;

using GabE.Module.ECS;
using System;
using Unity.Mathematics;


[UpdateInGroup(typeof(ECS_LifecycleSystemGroup))]
public partial struct ECS_PersonFactorySystem : ISystem
{
    private int lastProcessedDay;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        lastProcessedDay = -1;
    }

    public void OnUpdate(ref SystemState state)
    {
        var global = SystemAPI.GetSingleton<ECS_GlobalLifecyleFragment>();
        int currentDay = global.DayCount;

        if (currentDay % 5 != 0 || currentDay == lastProcessedDay || currentDay == 0)
            return;

        lastProcessedDay = currentDay;


        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.TempJob);
        var writter = ecb.AsParallelWriter();

        int entityCount = 1024;

        var job = new CreatePersonJob
        {
            CommandBuffer = writter,
            Seed = (uint)(SystemAPI.Time.DeltaTime + 1),
        };

        state.Dependency = job.ScheduleParallel(entityCount, 64, state.Dependency);
        state.CompleteDependency();

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }

    [BurstCompile(CompileSynchronously = true, FloatPrecision = FloatPrecision.Low, OptimizeFor = OptimizeFor.Performance)]
    private partial struct CreatePersonJob : IJobFor
    {
        public EntityCommandBuffer.ParallelWriter CommandBuffer;
        public uint Seed;

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
                Work = ECS_WorkerFragment.WorkType.Vagabond,
                IsWorking = false
            });

            CommandBuffer.AddComponent(index, entity, new ECS_PositionFragment
            {
                Position = new float3(0,0,0) 
            });

            CommandBuffer.AddComponent(index, entity, new ECS_Frag_Velocity
            {
                Value = 10
            });

            CommandBuffer.AddComponent(index, entity, new ECS_Frag_TargetPosition
            {
                Position = new float3(x,y,z)
            });
        }
    }
}
