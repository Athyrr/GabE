using Unity.Burst;
using Unity.Entities;

namespace GabE.Module.ECS
{
    [UpdateInGroup(typeof(ECS_Group_MovementManagement))]
    partial struct ECS_PathFindingSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
        
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
        
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        
        }
    }
}
