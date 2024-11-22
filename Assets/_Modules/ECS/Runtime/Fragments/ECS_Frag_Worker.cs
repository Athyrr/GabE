using Unity.Entities;

namespace GabE.Module.ECS
{
    public struct ECS_Frag_Worker : IComponentData
    {
        public enum JobType
        {
            None = 0,
            FoodHarvester = 1,
            Tiberman = 2,
            Miner = 3,
            Mason = 4,
            Vagabond = 5
        }

        public JobType Job;

        public bool IsWorking; //Use to learn too
    }
}
