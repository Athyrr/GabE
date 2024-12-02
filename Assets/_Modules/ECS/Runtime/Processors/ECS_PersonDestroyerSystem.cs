using Unity.Entities;
using Unity.Burst;

using GabE.Module.ECS;
using Unity.Collections;


/// <summary>
/// System responsible for destroying person entities based on certain conditions.
/// </summary>
[BurstCompile]
//[UpdateInGroup(typeof(ECS_Group_Lifecycle))]

[DisableAutoCreation]
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
    private partial struct DestroyOldPersonsJob : IJobEntity
    {
        /// <summary>
        /// Entity command buffer for destroying entities.
        /// </summary>
        //public EntityCommandBuffer.ParallelWriter Buffer;
        [ReadOnly] public EntityCommandBuffer Buffer;
        public EntityManager Manager;

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
                //Buffer.DestroyEntity(index, entity);     
                Buffer.DestroyEntity(entity);
            }
            Buffer.Playback(Manager);
            Buffer.Dispose();

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
        //EntityCommandBufferSystem ecbs = state.World.GetOrCreateSystemManaged<BeginSimulationEntityCommandBufferSystem>();
        EntityCommandBuffer buffer = new EntityCommandBuffer(Allocator.TempJob);

        //var buffer = ecbs.CreateCommandBuffer().AsParallelWriter();

        var destroyJob = new DestroyOldPersonsJob
        {
            Manager = state.EntityManager,
            Buffer = buffer
        };

        state.Dependency = destroyJob.ScheduleParallel(state.Dependency);

        //ecbs.AddJobHandleForProducer(state.Dependency); //@todo figure it out


    }

    #endregion
}
