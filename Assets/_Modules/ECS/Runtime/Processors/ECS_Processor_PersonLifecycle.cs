using GabE.Module.ECS;
using Unity.Entities;

public partial struct ECS_Processor_PersonLifecycle : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        var global = SystemAPI.GetSingletonRW<ECS_Frag_GameGlobal>();
        EntityManager entityManager = state.EntityManager;

        // Destroy old and usless persons
        {
            foreach (var (personData, entity) in SystemAPI.Query<RefRO<ECS_Frag_Person>>().WithEntityAccess())
            {
                if (personData.ValueRO.Age >= 70) // Set a death age
                    continue;

                entityManager.DestroyEntity(entity);
            }
        }


        // Create new entity
        {
            if (global.ValueRO.DayCount % 10 != 0)
                return;

            Entity person = entityManager.CreateEntity();
            entityManager.AddComponentData(person, new ECS_Frag_Person
            {
                Age = UnityEngine.Random.Range(16, 71),
                Stamina = 100,
                IsHappy = true,
                IsAlive = true
            });
            entityManager.AddComponentData(person, new ECS_Frag_Worker
            {
                Job = ECS_Frag_Worker.JobType.None,
                IsWorking = false
            });
        }
    }
}


