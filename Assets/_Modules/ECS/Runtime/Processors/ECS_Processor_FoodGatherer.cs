using GabE.Module.ECS;
using System.Linq;
using Unity.Burst;
using Unity.Entities;

[UpdateInGroup(typeof(ECS_Group_Lifecycle))]
[BurstCompile]
public partial struct ECS_Processor_FoodGatherer : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        foreach (var (harvester, position) in SystemAPI.Query<RefRO<ECS_Frag_Worker>, RefRW<ECS_Frag_Position>>())
        {
            if (harvester.ValueRO.Work == ECS_Frag_Worker.WorkType.FoodHarvester)
            {
                float foodCollected = UnityEngine.Random.Range(1f, 5f);
                //SystemAPI.GetSingletonRW<ECS_Frag_Resources>().ValueRW.resourceTypes.Keys.First(r => r == ResourceType.Food) -= foodCollected;
            }
        }
    }
}

