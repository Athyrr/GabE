using GabE.Module.ECS;
using Unity.Burst;
using Unity.Entities;


[UpdateInGroup(typeof(ECS_LifecycleSystemGroup))]
partial struct ECS_AgeSystem : ISystem
{
    int _currentDayCount;

    public void OnCreate(ref SystemState state)
    {
        _currentDayCount = 0;
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        SystemAPI.TryGetSingleton<ECS_GlobalLifecyleFragment>(out ECS_GlobalLifecyleFragment lifecycle);

        if (lifecycle.DayCount <= _currentDayCount)
            return;

        _currentDayCount = lifecycle.DayCount;

        foreach (var person in SystemAPI.Query<RefRW<ECS_PersonFragment>>())
            person.ValueRW.Age += 5; //@todo set the age incrementation per day.
    }
}

