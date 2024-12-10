using GabE.Module.ECS;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

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

        var personQuery = SystemAPI.QueryBuilder().WithAll<ECS_PersonFragment>().Build();
        var buildingQuery = SystemAPI.QueryBuilder().WithAll<ECS_BuildingFragment>().Build();

        HandlePopulationGrowing(ref state, ref global, ref personQuery);
        HandleProsperity(ref state, ref global, ref buildingQuery,ref personQuery);
        ApplyDayPassedEffects(global);
    }

    #endregion

    #region Private API

    private void HandleProsperity(ref SystemState state, ref RefRW<ECS_GlobalLifecyleFragment> global, ref EntityQuery buildingQuery, ref EntityQuery personQuery)
    {
        int buildingReward = 0;

        var buildings = buildingQuery.ToComponentDataArray<ECS_BuildingFragment>(Allocator.Temp);

        foreach (var building in buildings)
        {
            if (building.Type != BuildingType.Bookshop && building.Type != BuildingType.Museum)
                continue;

            switch (building.Type)
            {
                case BuildingType.Bookshop:
                    buildingReward += 5; //@todo define amount of proepserity for building
                    break;

                case BuildingType.Museum:
                    buildingReward += 15;
                    break;
            }
        }

        buildings.Dispose();


        var persons = personQuery.ToComponentDataArray<ECS_PersonFragment>(Allocator.Temp);

        int personCount = personQuery.CalculateEntityCount();
        int happyCount = 0;

        foreach (var perso in persons)
        {
            if (perso.IsHappy)
                happyCount++;
        }

        global.ValueRW.Prosperity = buildingReward + happyCount / 1000 /*personCount + buildingReward*/;

//        Debug.Log($"Prosperity: {buildingReward}.");
    }


    private void HandlePopulationGrowing(ref SystemState state, ref RefRW<ECS_GlobalLifecyleFragment> global, ref EntityQuery query)
    {
        int populationCount = query.CalculateEntityCount();
        global.ValueRW.Population = populationCount;
    }

    /// <summary>
    /// Applies the effects of a day passing in the game.
    /// </summary>
    /// <returns>True if a day has passed, false otherwise.</returns>
    private bool ApplyDayPassedEffects(RefRW<ECS_GlobalLifecyleFragment> global)
    {
        if (_elapsedTime < 5)  //@todo set day duration every 5 seconds
            return false;

        global.ValueRW.DayCount++;
        _elapsedTime = 0;

        return true;
    }

    #endregion
}
