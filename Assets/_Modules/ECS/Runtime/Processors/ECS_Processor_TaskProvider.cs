using Unity.Burst;
using Unity.Entities;

namespace GabE.Module.ECS
{
    partial struct ECS_Processor_TaskProvider : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach (var (worker, person) in SystemAPI.Query<RefRW<ECS_Frag_Worker>, RefRO<ECS_Frag_Person>>())
            {
                if (worker.ValueRO.IsWorking)
                    continue;

                if (person.ValueRO.Stamina < 40f) // Arbitrary value, Set max value in config
                {
                    worker.ValueRW.IsWorking = false;
                    continue;
                }

                worker.ValueRW.IsWorking = true;

                switch (worker.ValueRO.Job)
                {
                    case ECS_Frag_Worker.JobType.FoodHarvester:
                        //
                        break;
                        
                    case ECS_Frag_Worker.JobType.Miner:
                        //
                        break;
                        
                    case ECS_Frag_Worker.JobType.Tiberman:
                        //
                        break;
                        
                    case ECS_Frag_Worker.JobType.Mason:
                        //
                        break; 

                    case ECS_Frag_Worker.JobType.Vagabond:
                        //
                        break;
                }
            }
        }
    }
}
