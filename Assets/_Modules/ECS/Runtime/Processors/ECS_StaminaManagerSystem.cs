using GabE.Module.ECS;
using Unity.Burst;
using Unity.Entities;


[UpdateInGroup(typeof(ECS_LifecycleSystemGroup))]

partial struct NewISystemScript : ISystem
{
    public int _currentDay;

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        if (!SystemAPI.TryGetSingleton<ECS_GlobalLifecyleFragment>(out var lifecycle))
            return;

        if (lifecycle.DayCount <= _currentDay)
            return;

        _currentDay = lifecycle.DayCount;
    }
}

