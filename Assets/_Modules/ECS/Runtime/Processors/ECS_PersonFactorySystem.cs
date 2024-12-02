using GabE.Module.ECS;
using Unity.Burst;
using Unity.Entities;
using UnityEngine;

[UpdateInGroup(typeof(ECS_Group_Lifecycle))]
public partial class ECS_Processor_PersonFactory : SystemBase
{
    private int _lastProcessedDay;

    protected override void OnCreate()
    {
        _lastProcessedDay = -1;
    }

    protected override void OnUpdate()
    {
        var global = SystemAPI.GetSingleton<ECS_Frag_GameGlobal>();
        int currentDay = global.DayCount;

        // Check for create
        if (currentDay % 10 != 0 || currentDay == _lastProcessedDay)
            return;

        _lastProcessedDay = currentDay;
        Debug.Log($"ECS : Factory creating a person on Day {currentDay}");

        // Create buffer using ECB system
        var bufferSystem = World.GetOrCreateSystemManaged<BeginSimulationEntityCommandBufferSystem>();
        var ecb = bufferSystem.CreateCommandBuffer().AsParallelWriter();

        //@todo generate seed
        uint seed = (uint)(10);

        // Schedule job and execute
        Dependency = new CreatePersonJob
        {
            CommandBuffer = ecb,
            Seed = seed
        }.ScheduleParallel(Dependency);

        // BeginSimulationEntityCommandBufferSystem to auto PlayBack and Dispose Job.
        bufferSystem.AddJobHandleForProducer(Dependency);
    }

    [BurstCompile]
    private partial struct CreatePersonJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter CommandBuffer;
        public uint Seed;

        public void Execute([EntityIndexInQuery] int entityInQueryIndex)
        {
            Unity.Mathematics.Random random = new Unity.Mathematics.Random(Seed + (uint)entityInQueryIndex);

            var newPerson = CommandBuffer.CreateEntity(entityInQueryIndex);
            CommandBuffer.AddComponent(entityInQueryIndex, newPerson, new ECS_Frag_Person
            {
                Age = random.NextInt(16, 71),
                Stamina = 100,
                IsHappy = true,
                IsAlive = true
            });
            CommandBuffer.AddComponent(entityInQueryIndex, newPerson, new ECS_Frag_Worker
            {
                Work = ECS_Frag_Worker.WorkType.None,
                IsWorking = false
            });
        }
    }
}
