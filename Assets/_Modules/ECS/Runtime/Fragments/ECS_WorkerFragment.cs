using Unity.Entities;


public struct ECS_WorkerFragment : IComponentData
{
    public enum WorkType
    {
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

