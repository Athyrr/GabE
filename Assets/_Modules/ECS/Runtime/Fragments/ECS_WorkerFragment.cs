using GabE.Module.ECS;
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

    public bool IsRetrieving;

    public int HoldingCapacity;

    public int HoldResourcesAmount;

    public ResourceType GetResourceType(WorkType work)
    {
        return work switch
        {
            WorkType.FoodHarvester => ResourceType.Food,
            WorkType.Tiberman => ResourceType.Wood,
            WorkType.Miner => ResourceType.Stone,
            WorkType.Mason => ResourceType.None, 
            _ => ResourceType.None  
        };
    }
}