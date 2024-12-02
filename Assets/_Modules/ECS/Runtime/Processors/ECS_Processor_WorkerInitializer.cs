using GabE.Module.ECS;
using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

/// <summary>
/// System responsible for initializing worker entities.
/// </summary>
[UpdateInGroup(typeof(ECS_Group_Initialization))]
public partial class ECS_Processor_WorkerInitializer : SystemBase
{
    /// <summary>
    /// Called when the system is created.
    /// </summary>
    protected override void OnCreate()
    {
        int workerCount = 800; //@todo define number of start entities
        var workTypes = (ECS_Frag_Worker.WorkType[])Enum.GetValues(typeof(ECS_Frag_Worker.WorkType));

        // Schedule a job to create and initialize the entities
        var job = new WorkerInitializationJob
        {
            EntityCount = workerCount,

            WorkTypes = new NativeArray<ECS_Frag_Worker.WorkType>(workTypes, Allocator.Persistent),

            Seed = (uint)UnityEngine.Random.Range(1, int.MaxValue),

            EntityCommandBuffer = new EntityCommandBuffer(Allocator.TempJob),
        };

        // Execute the job
        Dependency = job.Schedule(Dependency);
        Dependency.Complete();

        // Playback and cleanup
        job.EntityCommandBuffer.Playback(EntityManager);
        job.EntityCommandBuffer.Dispose();
        job.WorkTypes.Dispose();
    }


    /// <summary>
    /// Called every frame. Disabled after initialization.
    /// </summary>
    protected override void OnUpdate() { Enabled = false; }

    /// <summary>
    /// Burst-compiled job for initializing worker entities.
    /// </summary>
    [BurstCompile]
    private struct WorkerInitializationJob : IJob
    {
        /// <summary>
        /// Number of entities to create.
        /// </summary>
        public int EntityCount;

        /// <summary>
        /// Array of available work types.
        /// </summary>
        public NativeArray<ECS_Frag_Worker.WorkType> WorkTypes;

        /// <summary>
        /// Seed for random number generation.
        /// </summary>
        public uint Seed;

        /// <summary>
        /// Entity command buffer for deferred entity creation and component addition.
        /// </summary>
        public EntityCommandBuffer EntityCommandBuffer;

        /// <summary>
        /// Executes the entity initialization job.
        /// </summary>
        public void Execute()
        {
            Unity.Mathematics.Random random = new Unity.Mathematics.Random(Seed);

            for (int i = 0; i < EntityCount; i++)
            {
                Entity entity = EntityCommandBuffer.CreateEntity();
                ECS_Frag_Worker.WorkType workType = WorkTypes[random.NextInt(0, WorkTypes.Length)];

                // Add components
                EntityCommandBuffer.AddComponent(entity, new ECS_Frag_Person
                {
                    Age = random.NextInt(18, 65),
                    Stamina = 100,
                    IsHappy = true,
                    IsAlive = true
                });

                EntityCommandBuffer.AddComponent(entity, new ECS_Frag_Worker
                {
                    Work = workType,
                    IsWorking = false
                });
                
                EntityCommandBuffer.AddComponent(entity, new ECS_Frag_Position
                {
                    Position = random.NextFloat3(),
                });

                EntityCommandBuffer.AddComponent(entity, new ECS_Frag_Velocity
                {
                    Value = 2f
                });

                EntityCommandBuffer.AddComponent(entity, new ECS_Frag_TargetPosition
                {
                    Position = new float3(random.NextFloat(0, 10), 0, random.NextFloat(0, 10))
                });
            }
        }
    }
}
