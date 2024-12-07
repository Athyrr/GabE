using Unity.Entities;


public struct ECS_WorkerFragment : IComponentData
{
    public enum WorkType
    {
        FoodHarvester = 0,
        Tiberman = 1,
        Miner = 2,
        Mason = 3,
        Vagabond = 4
    }

    public WorkType Work;

    public bool IsWorking; //Use to learn too

    public int HoldingCapacity;
}

