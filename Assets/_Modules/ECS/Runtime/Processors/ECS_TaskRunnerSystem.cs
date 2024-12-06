using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;

using GabE.Module.ECS;


[UpdateInGroup(typeof(ECS_LifecycleSystemGroup))]
public partial struct ECS_TaskProgressSystem : ISystem
{
    private DynamicBuffer<ECS_ResourceStorageFragment> _storage;

    [BurstCompile(FloatPrecision = FloatPrecision.Low, OptimizeFor = OptimizeFor.Performance)]
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

                //CommandBuffer.SetComponent<ECS_Frag_TargetPosition>(entityInQueryIndex, entity, storagePosition); //Set new position > Storage
            }
        }
    }

    private void OnCreate(ref SystemState state)
    {
        SystemAPI.TryGetSingletonBuffer<ECS_ResourceStorageFragment>(out _storage, true);
    }

    [BurstCompile(OptimizeFor = OptimizeFor.Performance)]
    public void OnUpdate(ref SystemState state)
    {
        float deltaTime = SystemAPI.Time.DeltaTime;

        EntityCommandBuffer commandBuffer = new EntityCommandBuffer(Allocator.TempJob);
        var writter = commandBuffer.AsParallelWriter();

        var taskProgressJob = new TaskProgressJob
        {
            DeltaTime = deltaTime,
            CommandBuffer = writter
        };

        state.Dependency = taskProgressJob.ScheduleParallel(state.Dependency);
        state.Dependency.Complete();

        commandBuffer.Playback(state.EntityManager);
        commandBuffer.Dispose();
    }
}
