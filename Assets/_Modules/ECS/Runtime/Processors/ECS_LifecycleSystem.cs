using GabE.Module.ECS;
using Unity.Burst;
using Unity.Entities;

[UpdateInGroup(typeof(ECS_LifecycleSystemGroup))]
[BurstCompile]
public partial struct ECS_LifecycleSystem : ISystem
{
    #region Fields 

    /// <summary>
    /// Time since the last day passed.
    /// </summary>
    private float _elapsedTime;

    #endregion

    #region Lifecyle 

    /// <summary>
    /// Creates the singleton entity for global game data.
    /// </summary>
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        Entity e = state.EntityManager.CreateEntity();
        state.EntityManager.AddComponent<ECS_GlobalLifecyleFragment>(e);
    }

    /// <summary>
    /// Updates the global game state, including day progression.
    /// </summary>
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        _elapsedTime += SystemAPI.Time.DeltaTime;

        if (!SystemAPI.TryGetSingletonRW<ECS_GlobalLifecyleFragment>(out var global))
            return;

        var query = SystemAPI.QueryBuilder().WithAll<ECS_PersonFragment>().Build();
        int populationCount = query.CalculateEntityCount();

        global.ValueRW.Population = populationCount;

        ApplyDayPassedEffects(global);
    }

    #endregion

    #region Private API

    /// <summary>
    /// Applies the effects of a day passing in the game.
    /// </summary>
    /// <returns>True if a day has passed, false otherwise.</returns>
    private bool ApplyDayPassedEffects(RefRW<ECS_GlobalLifecyleFragment> global)
    {
        if (_elapsedTime < 5)  // every 5 seconds
            return false;

        global.ValueRW.DayCount++;
        _elapsedTime = 0;

        return true;
    }

    #endregion
}
