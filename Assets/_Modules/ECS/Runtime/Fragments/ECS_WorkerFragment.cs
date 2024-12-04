using Unity.Entities;

namespace GabE.Module.ECS
{
    public struct ECS_WorkerFragment : IComponentData
    {
        public enum WorkType
        {
            None = 0,
            FoodHarvester = 1,
            Tiberman = 2,
            Miner = 3,
            Mason = 4,
            Vagabond = 5
        }

        public WorkType Work;

        public bool IsWorking; //Use to learn too

        public int HoldingCapacity;
    }
}
