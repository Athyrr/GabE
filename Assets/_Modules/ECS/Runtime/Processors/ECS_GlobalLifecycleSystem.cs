using Unity.Burst;
using Unity.Entities;
using UnityEngine;

using GabE.Module.ECS;


[UpdateInGroup(typeof(ECS_Group_Lifecycle))]
public partial struct ECS_Processor_GlobalLifecycle : ISystem
{
    #region Fields 

    /// <summary>
    /// Singleton component for global game state.
    /// </summary>
    private RefRW<ECS_Frag_GameGlobal> _global;

    /// <summary>
    /// Time since the last day passed.
    /// </summary>
    private float _elapsedTime;

    #endregion

    #region Lifecyle 

    /// <summary>
    /// Creates the singleton entity for global game data.
    /// </summary>
    //[BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.EntityManager.CreateEntity(typeof(ECS_Frag_GameGlobal));
    }

    /// <summary>
    /// Updates the global game state, including day progression.
    /// </summary>
    //[BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        if (!SystemAPI.TryGetSingletonRW<ECS_Frag_GameGlobal>(out _global))
        {
            //Debug.Log("ECS : Global System not reachable");
            return;
        }

        //Debug.Log("ECS : Global System Sucess");

        _elapsedTime += SystemAPI.Time.DeltaTime;

        ApplyDayPassedEffects();
    }

    #endregion

    #region Private API

    /// <summary>
    /// Applies the effects of a day passing in the game.
    /// </summary>
    /// <returns>True if a day has passed, false otherwise.</returns>
    private bool ApplyDayPassedEffects()
    {
        if (_elapsedTime < 5)  // every 5 seconds
            return false;

        _global.ValueRW.DayCount++;

        _elapsedTime = 0;
        //Debug.Log("Day passed. Day Count: " + _global.ValueRW.DayCount);

        return true;
    }

    #endregion
}
