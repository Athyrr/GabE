using Unity.Entities;

namespace GabE.Module.ECS
{
    public struct ECS_Frag_Worker : IComponentData
    {
        public enum JobType
        {
            FoodHarvester,
            Tiberman,
            Miner,
            Mason,
            vagabond
        }

        public JobType Job;

        public bool IsWorking; //Use to learn too
    }
}
