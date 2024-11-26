using System;

using Unity.Entities;
using Unity.Burst;
using Unity.Mathematics;

using GabE.Module.ECS;


/// <summary>
/// Initializes worker entities.
/// </summary>
[BurstCompile]
[UpdateInGroup(typeof(InitializationSystemGroup))]
public partial struct ECS_Processor_WorkerInitializer : ISystem
{
    /// <summary>
    /// Creates worker entities with initial data.
    /// </summary>
    private void OnCreate(ref SystemState state)
    {
        // Random generation compatible [Burst]
        uint seed = (uint)math.max(1, (state.WorldUnmanaged.Time.ElapsedTime * 1000));
        Unity.Mathematics.Random random = new Unity.Mathematics.Random(seed);


        ECS_Frag_GlobalStats stats = new(/*SystemAPI.GetSingleton<ECS_Frag_GlobalStatsInitialisation>(*/);

        EntityArchetype workerArch = state.EntityManager.CreateArchetype
            (
            typeof(ECS_Frag_Person),
            typeof(ECS_Frag_Worker),
            typeof(ECS_Frag_Position),
            typeof(ECS_Frag_Velocity)
            );

        foreach (ECS_Frag_Worker.WorkType type in Enum.GetValues(typeof(ECS_Frag_Worker.WorkType)))
        {
            Entity entity = state.EntityManager.CreateEntity(workerArch);

            // Person
            state.EntityManager.SetComponentData(entity, new ECS_Frag_Person
            {
                Age = random.NextInt(stats.MinAge, stats.LifeExpectancy),
                Stamina = 100,
                IsHappy = true,
                IsAlive = true
            });

            //Work
            state.EntityManager.SetComponentData(entity, new ECS_Frag_Worker
            {
                Work = type,
                IsWorking = false
            });

            //velocity
            state.EntityManager.SetComponentData(entity, new ECS_Frag_Velocity
            {
                Value = stats.BaseVelocity
            });
        }

    }
}

