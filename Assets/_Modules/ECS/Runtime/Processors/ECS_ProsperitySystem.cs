using Unity.Burst;
using Unity.Entities;


[UpdateInGroup(typeof(ECS_LifecycleSystemGroup))]

partial struct ECS_ProsperitySystem : ISystem
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

