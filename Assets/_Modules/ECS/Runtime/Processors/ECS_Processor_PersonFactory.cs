using Unity.Entities;
using Unity.Burst;
using Unity.Mathematics;

using GabE.Module.ECS;


/// <summary>
/// System responsible for creating new person entities.
/// </summary>
[BurstCompile]
[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct ECS_Processor_PersonFactory : ISystem
{
    #region Nested

    /// <summary>
    /// Job to create person entities in parallel.
    /// </summary>
    [BurstCompile]
    private partial struct CreatePersonJob : IJobEntity
    {
        /// <summary>
        /// Parallel writer for entity command buffer.
        /// </summary>
        public EntityCommandBuffer.ParallelWriter Buffer;

        public uint Seed;

        /// <summary>
        /// Executes the job for each entity in the query.
        /// </summary>
        /// <param name="entityInQueryIndex">Index of the entity in the query.</param>
        public void Execute([EntityIndexInQuery] int entityInQueryIndex)
        {
            // Random generation compatible [Burst]
            Random random = new Random(Seed + (uint)entityInQueryIndex);

            Entity newPerson = Buffer.CreateEntity(entityInQueryIndex);
            Buffer.AddComponent(entityInQueryIndex, newPerson, new ECS_Frag_Person
            {
                Age = random.NextInt(16, 71),
                Stamina = 100,
                IsHappy = true,
                IsAlive = true
            });
            Buffer.AddComponent(entityInQueryIndex, newPerson, new ECS_Frag_Worker
            {
                Work = ECS_Frag_Worker.WorkType.None,
                IsWorking = false
            });
        }
    }

    #endregion


    #region Fields 

    uint _seed;

    #endregion


    #region Lifecyle


    /// <summary>
    /// System update method. Creates new person entities every 10 game days. //@todo define the creation frequency
    /// </summary>
    /// <param name="state">System state.</param>
    public void OnUpdate(ref SystemState state)
    {
        if (_seed == 0) 
            _seed = (uint)math.max(1, (state.WorldUnmanaged.Time.ElapsedTime * 1000));

        EntityCommandBufferSystem _ecbs = state.World.GetOrCreateSystemManaged<BeginSimulationEntityCommandBufferSystem>();

        EntityCommandBuffer.ParallelWriter buffer = _ecbs.CreateCommandBuffer().AsParallelWriter();

        var global = SystemAPI.GetSingletonRW<ECS_Frag_GameGlobal>();

        if (global.ValueRO.DayCount % 10 != 0)
            return;

        //state.Dependency = new CreatePersonJob
        //{
        //    Seed = _seed,
        //    Buffer = buffer
        //}.ScheduleParallel(state.Dependency); //Job.Execute() called here

        _ecbs.AddJobHandleForProducer(state.Dependency);
    }

    #endregion
}
