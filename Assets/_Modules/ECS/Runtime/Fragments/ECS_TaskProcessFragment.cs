using Unity.Entities;


public struct ECS_TaskProcessFragment : IComponentData
{
    public float Duration;

    public float Progression;

    public bool HasFinished;

    public TaskType Task;

    public enum TaskType
    {
        Learning,
        Mining,
        HarvestingFood,
        HarvestingWood
    }
}

