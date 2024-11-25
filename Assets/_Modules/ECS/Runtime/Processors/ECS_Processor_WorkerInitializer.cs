using System;
using Unity.Burst;
using Unity.Entities;
using UnityEngine;

namespace GabE.Module.ECS
{
    /// <summary>
    /// Initializes worker entities.
    /// </summary>
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    partial class ECS_Processor_WorkerInitializer : SystemBase
    {
        /// <summary>
        /// Creates worker entities with initial data.
        /// </summary>
        /// <param name="state">The current system state.</param>
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {

            Debug.Log("Enter Work Initializer : ISytem");

            EntityManager entityManager = state.EntityManager;


            ECS_Frag_GlobalStatsInitialisation stats = SystemAPI.GetSingleton<ECS_Frag_GlobalStatsInitialisation>();

            //NativeArray<ComponentType> d = new NativeArray<ComponentType>(10, Allocator.Temp);
            ////{
            ////    ECS_Frag_Person,
            ////    ECS_Frag_Worker,
            ////    ECS_Frag_Position,
            ////    ECS_Frag_Velocity
            ////};

            //var caca = entityManager.CreateArchetype(d);




            var workerArch = entityManager.CreateArchetype
                (
                    typeof(ECS_Frag_Person),
                    typeof(ECS_Frag_Worker),
                    typeof(ECS_Frag_Position),
                    typeof(ECS_Frag_Velocity)
                 );

            // BurstCompile compatible way to generate random number
            uint seed = (uint)UnityEngine.Random.Range(1, int.MaxValue);
            Unity.Mathematics.Random random = new Unity.Mathematics.Random(seed);

            foreach (ECS_Frag_Worker.WorkType type in Enum.GetValues(typeof(ECS_Frag_Worker.WorkType)))
            {
                Entity entity = entityManager.CreateEntity(workerArch);

                // Person
                entityManager.SetComponentData(entity, new ECS_Frag_Person
                {
                    Age = random.NextInt(stats.MinAge, stats.LifeExpectancy),
                    Stamina = 100,
                    IsHappy = true,
                    IsAlive = true
                });

                //Work
                entityManager.SetComponentData(entity, new ECS_Frag_Worker
                {
                    Work = type,
                    IsWorking = false
                });

                //velocity
                entityManager.SetComponentData(entity, new ECS_Frag_Velocity
                {
                    Value = stats.BaseVelocity
                });
            }
        }


        protected override void OnCreate()
        {
            base.OnCreate();
            Debug.Log("Enter Work Initializer :override SystemBase");
        }

        protected override void OnUpdate()
        {
            Debug.Log("Update Work Initializer");
        }
    }
}
