using Unity.Entities;
using Unity.Burst;
using Unity.Mathematics;

using GabE.Module.ECS;
using UnityEngine;
using Unity.Collections;


/// <summary>
/// System responsible for creating new person entities.
/// </summary>
[UpdateInGroup(typeof(ECS_Group_Lifecycle))]
[DisableAutoCreation]
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
        //public EntityCommandBuffer.ParallelWriter Buffer;

        [ReadOnly] public EntityCommandBuffer CommandBuffer;

        public EntityManager EntityManager;

        public uint Seed;

        /// <summary>
        /// Executes the job for each entity in the query.
        /// </summary>
        /// <param name="entityInQueryIndex">Index of the entity in the query.</param>
        public void Execute([EntityIndexInQuery] int entityInQueryIndex)
        {
            // Random seed
            Unity.Mathematics.Random random = new Unity.Mathematics.Random(Seed + (uint)entityInQueryIndex);

            //Entity newPerson = CreateEntity(entityInQueryIndex);
            Entity newPerson = CommandBuffer.CreateEntity();
            CommandBuffer.AddComponent(newPerson, new ECS_Frag_Person
            {
                Age = random.NextInt(16, 71),
                Stamina = 100,
                IsHappy = true,
                IsAlive = true
            });
            CommandBuffer.AddComponent(newPerson, new ECS_Frag_Worker
            {
                Work = ECS_Frag_Worker.WorkType.None,
                IsWorking = false
            });

            CommandBuffer.Playback(EntityManager);
            CommandBuffer.Dispose();

            Debug.Log("Build Person !");
        }
    }

    #endregion


    #region Fields 

    //uint _seed;

    //public EntityCommandBufferSystem _ecbs;

    //private int _processDay = -1;


    #endregion



    #region Lifecyle

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        //base.OnCreate();
        //_ecbs = World.GetOrCreateSystemManaged<BeginSimulationEntityCommandBufferSystem>();

        Debug.Log("Success get Buffer System");

    }

    /// <summary>
    /// System update method. Creates new person entities every 10 game days. //@todo define the creation frequency
    /// </summary>
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        Debug.Log("Update Factory");

        uint _seed = 0;
        if (_seed == 0)
            //_seed = (uint)math.max(1, (World.Time.ElapsedTime * 1000));
            _seed = 10;
        


        //EntityCommandBuffer.ParallelWriter buffer = _ecbs.CreateCommandBuffer().AsParallelWriter();
        EntityCommandBuffer cb = new EntityCommandBuffer(Allocator.TempJob);

        var global = SystemAPI.GetSingletonRW<ECS_Frag_GameGlobal>();

        int _processDay = 0;

        if (global.ValueRO.DayCount % 10 != 0 || global.ValueRO.DayCount == _processDay)
            return;

        _processDay = global.ValueRO.DayCount;
        Debug.Log("Launch Create Job");

        state.Dependency = new CreatePersonJob
        {
            CommandBuffer = cb,
            EntityManager = state.EntityManager,
            Seed = _seed
        }.ScheduleParallel(state.Dependency); //Job.Execute() called here

        //_ecbs.AddJobHandleForProducer(Dependency);
    }



    #endregion
}
