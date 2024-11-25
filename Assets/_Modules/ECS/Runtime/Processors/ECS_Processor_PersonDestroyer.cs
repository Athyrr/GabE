using GabE.Module.ECS;
using Unity.Burst;
using Unity.Entities;

[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial class ECS_Processor_PersonDestroyer : SystemBase
{
    #region Nested Aspects

    public readonly partial struct PersonPositionAspect : IAspect
    {
        public readonly RefRO<ECS_Frag_Person> Person;
        public readonly RefRW<ECS_Frag_Position> Position;
    }

    #endregion

    #region Nested Jobs

    [BurstCompile]
    private partial struct DestroyOldPersonsJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter ECB;

        public void Execute(Entity entity, PersonPositionAspect personAspect, [EntityIndexInQuery] int index)
        {
            if (personAspect.Person.ValueRO.Age > 70) //@todo use global settings to set max age
            {
                ECB.DestroyEntity(index, entity);
            }

            // If other conditions ..
        }
    }

    #endregion

    #region Fields

    private EntityCommandBufferSystem _ecbSystem;

    #endregion

    #region Lifecycle

    protected void OnCreate(ref SystemState state)
    {
        _ecbSystem = state.World.GetOrCreateSystemManaged<BeginSimulationEntityCommandBufferSystem>();
    }

    public void OnUpdate(ref SystemState state)
    {
        var ecb = _ecbSystem.CreateCommandBuffer().AsParallelWriter();

        var destroyJob = new DestroyOldPersonsJob
        {
            ECB = ecb
        };

        state.Dependency = destroyJob.ScheduleParallel(state.Dependency);

        _ecbSystem.AddJobHandleForProducer(state.Dependency); //@todo figure it out
    }

    protected override void OnUpdate()
    {
    }

    #endregion
}
