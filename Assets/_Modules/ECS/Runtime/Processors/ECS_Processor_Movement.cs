using GabE.Module.ECS;
using System.Numerics;
using Unity.Entities;

[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct MovementSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        float deltaTime = SystemAPI.Time.DeltaTime;

        foreach (var (position, velocity, target) in
                 SystemAPI.Query<RefRW<ECS_Frag_Position>, RefRW<ECS_Frag_Velocity>, RefRO<ECS_Frag_Position>>()) //Use building instead
        {
            Vector3 direction = Vector3.Normalize(target.ValueRO.Position - position.ValueRW.Position);

            position.ValueRW.Position += direction * velocity.ValueRW.Value * deltaTime;

            if (Vector3.Distance(position.ValueRW.Position, target.ValueRO.Position) < 0.1f)
                velocity.ValueRW.Value = 0;
        }
    }
}
