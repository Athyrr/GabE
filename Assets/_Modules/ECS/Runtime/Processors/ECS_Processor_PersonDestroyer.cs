using Unity.Entities;
using Unity.Burst;

using GabE.Module.ECS;


/// <summary>
/// System responsible for destroying person entities based on certain conditions.
/// </summary>
[BurstCompile]
[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct ECS_Processor_PersonDestroyer : ISystem
{
    #region Aspects

    /// <summary>
    /// Aspect representing a person entity with position data.
    /// </summary>
    public readonly partial struct PersonPositionAspect : IAspect
    {
        /// <summary>
        /// Person component.
        /// </summary>
        public readonly RefRO<ECS_Frag_Person> Person;
        /// <summary>
        /// Position component.
        /// </summary>
        public readonly RefRW<ECS_Frag_Position> Position;
    }

    #endregion

    #region Nested Jobs

    /// <summary>
    /// Job that destroys old person entities.
    /// </summary>
    [BurstCompile]
    private partial struct DestroyOldPersonsJob : IJobEntity
    {
        /// <summary>
        /// Entity command buffer for destroying entities.
        /// </summary>
        public EntityCommandBuffer.ParallelWriter Buffer;

        /// <summary>
        /// Executes the job for each person entity.
        /// </summary>
        /// <param name="entity">The current person entity.</param>
        /// <param name="personAspect">Aspect for accessing person data.</param>
        /// <param name="index">Index of the entity in the query.</param>
        public void Execute(Entity entity, PersonPositionAspect personAspect, [EntityIndexInQuery] int index)
        {
            if (personAspect.Person.ValueRO.Age > 70) //@todo use global settings to set max age
            {
                Buffer.DestroyEntity(index, entity);
            }

            // @todo other destroying conditions
        }
    }

    #endregion


    #region Lifecycle

    /// <summary>
    /// Updates the system. Destroys persons if needed.
    /// </summary>
    /// <param name="state">System state.</param>
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBufferSystem ecbs = state.World.GetOrCreateSystemManaged<BeginSimulationEntityCommandBufferSystem>();

        var buffer = ecbs.CreateCommandBuffer().AsParallelWriter();

        var destroyJob = new DestroyOldPersonsJob
        {
            Buffer = buffer
        };

        state.Dependency = destroyJob.ScheduleParallel(state.Dependency);

        ecbs.AddJobHandleForProducer(state.Dependency); //@todo figure it out
    }

    #endregion
}
