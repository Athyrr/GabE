using GabE.Module.ECS;
using Unity.Entities;
using UnityEngine;

public partial struct ECS_Processor_GlobalLifecycle : ISystem
{
    private float _elapsedTime;

    public void OnUpdate(ref SystemState state)
    {
        var global = SystemAPI.GetSingletonRW<ECS_Frag_GameGlobal>();

        _elapsedTime += SystemAPI.Time.DeltaTime;

        if (_elapsedTime < 5)
            return;

        global.ValueRW.DayCount++;
        _elapsedTime = 0;

        Debug.Log("Day passed. Day Count: " + global.ValueRW.DayCount);
    }
}
