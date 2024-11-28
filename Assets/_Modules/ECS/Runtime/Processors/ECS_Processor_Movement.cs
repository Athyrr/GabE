using GabE.Module.ECS;
using Unity.Burst;
using Unity.Entities;

[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct ECS_Processor_Movement : ISystem
{

    #region Nested 

    [BurstCompile]
    partial struct MovementJob : IJobEntity
    {
        public float DeltaTime;

        public void Execute(ECS_Frag_Position entityPosition, ECS_Frag_Velocity velocity, ECS_Frag_Worker worker, BuildingAspect building)
        {
            //float3 buildingPosition = float3.zero;
            //bool foundBuilding = false;


            //if (building.Building.ValueRO.Type == worker.Job)
            //{
            //    buildingPosition = b.Position;
            //    foundBuilding = true;
            //    break;
            //}


            //if (!foundBuilding || !worker.IsWorking)
            //    return;

            //float3 direction = math.normalize(building.Position - entityPosition.Position);

            //entityPosition.Position += direction * velocity.Value * DeltaTime;

            //if (math.distance(entityPosition.Position, buildingPosition) < 0.1f)
            //{
            //    velocity.Value = 0;

            //}
        }
    }

    public readonly partial struct BuildingAspect : IAspect
    {
        public readonly RefRW<ECS_Frag_Position> Position;
        public readonly RefRO<ECS_Frag_Building> Building;
    }

    #endregion
    public void OnUpdate(ref SystemState state)
    {
        float deltaTime = SystemAPI.Time.DeltaTime;

        var job = new MovementJob
        {
            DeltaTime = deltaTime,
        };

        state.Dependency = job.ScheduleParallel(state.Dependency);
    }

  
}
