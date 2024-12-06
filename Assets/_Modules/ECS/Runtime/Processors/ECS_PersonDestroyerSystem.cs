using Unity.Entities;
using Unity.Burst;
using UnityEngine;

/// <summary>
/// System responsible for destroying person entities based on certain conditions.
/// </summary>
[UpdateInGroup(typeof(ECS_LifecycleSystemGroup))]
[BurstCompile]
//[DisableAutoCreation]
public partial struct ECS_Processor_PersonDestroyer : ISystem
{
    /// <summary>
    /// Job that destroys old person entities.
    /// </summary>
    [BurstCompile]
    private partial struct DestroyPersonJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter CommandBuffer;

        [BurstCompile]
        public void Execute([EntityIndexInQuery] int entityInQueryIndex, Entity entity, in ECS_PersonFragment person)
        {
            // Check age condition
            if (person.Age > 90)
            {
                CommandBuffer.DestroyEntity(entityInQueryIndex, entity);
            }
        }
    }

    public void OnUpdate(ref SystemState state)
    {
        // Get the command buffer system
        var ecbSystem = state.World.GetOrCreateSystemManaged<BeginSimulationEntityCommandBufferSystem>();
        var ecb = ecbSystem.CreateCommandBuffer().AsParallelWriter();

        // Schedule the DestroyPersonJob
        state.Dependency = new DestroyPersonJob
        {
            CommandBuffer = ecb
        }.ScheduleParallel(state.Dependency);

        // Add the job dependency to the command buffer system, auto Complete and Dispose 
        ecbSystem.AddJobHandleForProducer(state.Dependency);
    }
}
