using Unity.Burst;
using Unity.Entities;


[UpdateInGroup(typeof(ECS_Group_Lifecycle))]
partial struct ECS_JobPrioritySystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {

    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {

    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {

    }
}
