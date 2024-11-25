using GabE.Module.ECS;
using Unity.Burst;
using Unity.Entities;

[BurstCompile]
//[UpdateInGroup(typeof(UpdateInGroupAttribute))]
public partial class ECS_Processor_PersonFactory : SystemBase
{
    #region Fields

    private EntityCommandBufferSystem _ecbs;

    #endregion

    #region Lifecycle

    public void OnCreate(ref SystemState state)
    {
        _ecbs = state.World.GetOrCreateSystemManaged<BeginSimulationEntityCommandBufferSystem>();
    }

    public void OnUpdate(ref SystemState state)
    {
        var ecb = _ecbs.CreateCommandBuffer();

        var global = SystemAPI.GetSingletonRW<ECS_Frag_GameGlobal>();
        if (global.ValueRO.DayCount % 10 == 0)
        {
            Entity newPerson = ecb.CreateEntity();
            ecb.AddComponent(newPerson, new ECS_Frag_Person
            {
                Age = UnityEngine.Random.Range(16, 71),
                Stamina = 100,
                IsHappy = true,
                IsAlive = true
            });
            ecb.AddComponent(newPerson, new ECS_Frag_Worker
            {
                Work = ECS_Frag_Worker.WorkType.None,
                IsWorking = false
            });
        }

        _ecbs.AddJobHandleForProducer(state.Dependency);
    }

    protected override void OnUpdate()
    {
    }

    #endregion
}
