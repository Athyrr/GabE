using GabE.Module.ECS;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;


[UpdateInGroup(typeof(ECS_LifecycleSystemGroup))]
public partial struct ECS_TaskProgressionSystem : ISystem
{

    [BurstCompile(FloatPrecision = FloatPrecision.Low, OptimizeFor = OptimizeFor.Performance)]
    private partial struct TaskCheckProgressJob : IJobEntity
    {
        public float DeltaTime;
        public EntityCommandBuffer.ParallelWriter CommandBuffer;

        private float progression;

        public void Execute(Entity entity, [EntityIndexInQuery] int index, ref ECS_WorkerFragment worker,
                            in ECS_PositionFragment position, in ECS_Frag_TargetPosition target)
        {
            if (worker.HoldResourcesAmount >= worker.HoldingCapacity)
                return;

            if (math.distance(position.Position, target.Position) > 0.2f) //@todo set resources zones range for work
                return;

            if (!worker.IsWorking)
            {
                CommandBuffer.AddComponent<ECS_TaskProcessFragment>(index, entity);
                CommandBuffer.SetComponent<ECS_TaskProcessFragment>(index, entity, new ECS_TaskProcessFragment
                {
                    Duration = 3 //@todo Set task duration
                });
                worker.IsWorking = true;
            }
        }
    }

    [BurstCompile(FloatPrecision = FloatPrecision.Low, OptimizeFor = OptimizeFor.Performance)]
    private partial struct TaskProgressJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter CommandBuffer;
        public float DeltaTime;

        public void Execute(Entity entity, [EntityIndexInQuery] int index, ref ECS_TaskProcessFragment process, ref ECS_WorkerFragment worker)
        {
            process.Progression += DeltaTime;
            if (process.Progression >= process.Duration)
            {
                if (process.Task != ECS_TaskProcessFragment.TaskType.Learning)
                {
                    worker.IsWorking = false;
                    worker.HoldResourcesAmount = worker.HoldingCapacity;
                }

                if (process.Task == ECS_TaskProcessFragment.TaskType.Learning)
                {
                    worker.IsWorking = false;
                }

                CommandBuffer.RemoveComponent<ECS_TaskProcessFragment>(index, entity);
            }
        }
    }

    [BurstCompile(OptimizeFor = OptimizeFor.Performance)]
    public void OnUpdate(ref SystemState state)
    {
        float deltaTime = SystemAPI.Time.DeltaTime;

        EntityCommandBuffer commandBufferProgress = new EntityCommandBuffer(Allocator.TempJob);
        var progressWriter = commandBufferProgress.AsParallelWriter();

        var taskProgressJob = new TaskProgressJob
        {
            DeltaTime = deltaTime,
            CommandBuffer = progressWriter,
        };

        state.Dependency = taskProgressJob.ScheduleParallel(state.Dependency);
        state.Dependency.Complete();

        commandBufferProgress.Playback(state.EntityManager);
        commandBufferProgress.Dispose();

        EntityCommandBuffer commandBufferCheck = new EntityCommandBuffer(Allocator.TempJob);
        var checkWriter = commandBufferCheck.AsParallelWriter();

        var taskCheckProgressJob = new TaskCheckProgressJob
        {
            DeltaTime = deltaTime,
            CommandBuffer = checkWriter,
        };

        state.Dependency = taskCheckProgressJob.ScheduleParallel(state.Dependency);
        state.Dependency.Complete();

        commandBufferCheck.Playback(state.EntityManager);
        commandBufferCheck.Dispose();
    }
}

