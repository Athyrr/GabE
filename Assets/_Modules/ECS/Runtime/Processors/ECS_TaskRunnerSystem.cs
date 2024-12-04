using GabE.Module.ECS;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[UpdateInGroup(typeof(ECS_LifecycleSystemGroup))]
[BurstCompile]
public partial struct ECS_TaskProgressSystem : ISystem
{
    [BurstCompile]
    private partial struct TaskProgressJob : IJobEntity
    {
        public float DeltaTime;
        public EntityCommandBuffer.ParallelWriter CommandBuffer;

        public void Execute(Entity entity, [EntityIndexInQuery] int entityInQueryIndex,
                            ref ECS_TaskProcessFragment task, ref ECS_WorkerFragment worker,
                            in ECS_Frag_Position position, in ECS_Frag_TargetPosition target)
        {
            if (!worker.IsWorking || task.HasFinished)
                return;

            if (math.distance(position.Position, target.Position) > 0.2f)
                return;

            task.Progression += DeltaTime;

            if (task.Progression >= task.Duration)
            {
                task.HasFinished = true;
                worker.IsWorking = false;

                CommandBuffer.RemoveComponent<ECS_Frag_TargetPosition>(entityInQueryIndex, entity); //Set new position > Storage
            }
        }
    }

    public void OnUpdate(ref SystemState state)
    {
        float deltaTime = SystemAPI.Time.DeltaTime;

        var ecbSystem = state.World.GetExistingSystemManaged<BeginSimulationEntityCommandBufferSystem>();
        var commandBuffer = ecbSystem.CreateCommandBuffer().AsParallelWriter();

        var taskProgressJob = new TaskProgressJob
        {
            DeltaTime = deltaTime,
            CommandBuffer = commandBuffer
        };

        state.Dependency = taskProgressJob.ScheduleParallel(state.Dependency);

        ecbSystem.AddJobHandleForProducer(state.Dependency);
    }
}
