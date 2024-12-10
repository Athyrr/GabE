using GabE.Module.ECS;
using Unity.Collections;
using Unity.Entities;


public partial struct ECS_LearningProcessSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        var schoolQuery = SystemAPI.QueryBuilder().WithAll<ECS_BuildingFragment>().WithAll<ECS_PositionFragment>().Build();
        var buildings = schoolQuery.ToComponentDataArray<ECS_BuildingFragment>(Allocator.Temp);
        var buildingsPositions = schoolQuery.ToComponentDataArray<ECS_PositionFragment>(Allocator.Temp);

        foreach (var (person, learner, worker, entity) in SystemAPI.Query<
            RefRO<ECS_PersonFragment>,
            RefRO<ECS_LearningTag>,
            RefRO<ECS_WorkerFragment>>().WithEntityAccess())
        {
            if (person.ValueRO.IsExhausted)
                continue;

            bool assigned = false;

            for (int i = 0; i < buildings.Length; i++)
            {
                if (buildings[i].Type == BuildingType.School && buildings[i].Occupants < buildings[i].Capacicty)
                {
                    var b = buildings[i];
                    var buildingPosition = buildingsPositions[i].Position;

                    state.EntityManager.AddComponent<ECS_Frag_TargetPosition>(entity);
                    state.EntityManager.SetComponentData(entity, new ECS_Frag_TargetPosition
                    {
                        Position = buildingPosition
                    });

                    state.EntityManager.SetComponentData(entity, new ECS_WorkerFragment
                    {
                        Work = learner.ValueRO.NewWork
                    });


                    b.Occupants++;

                    assigned = true;

                    state.EntityManager.RemoveComponent<ECS_LearningTag>(entity);

                    break;
                }
            }

            if (!assigned)
            {
                // Default behavior
            }
        }

        buildings.Dispose();
        buildingsPositions.Dispose();
    }
}

