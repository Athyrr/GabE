using GabE.Module.ECS;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[UpdateBefore(typeof(ECS_PathFindingSystem))]
[UpdateInGroup(typeof(ECS_Group_MovementManagement))]
public partial struct ECS_Processor_Movement : ISystem
{
    #region Nested

    [BurstCompile]
    partial struct MovementJob : IJobEntity //@todo Use JobChunk
    {
        public float DeltaTime;
        public float StopDistance;

        public void Execute(MoverAspect mover, ref ECS_Frag_TargetPosition target)
        {
            float3 currentPosition = mover.Position.ValueRO.Position;
            float3 direction = math.normalize(target.Position - currentPosition);

            float distance = math.distance(currentPosition, target.Position);

            if (distance <= StopDistance)
                return;

            mover.Position.ValueRW.Position += direction * mover.Velocity.ValueRO.Value * DeltaTime;
        }
    }

    public readonly partial struct MoverAspect : IAspect
    {
        public readonly RefRW<ECS_Frag_Position> Position;
        public readonly RefRO<ECS_Frag_Velocity> Velocity;
    }

    #endregion

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        float deltaTime = SystemAPI.Time.DeltaTime;

        var ecb = new EntityCommandBuffer(Allocator.TempJob);

        var job = new MovementJob
        {
            DeltaTime = deltaTime,
            StopDistance = 0.1f
        };

        state.Dependency = job.ScheduleParallel(state.Dependency);
        state.Dependency.Complete();
       
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}
