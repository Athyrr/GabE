using Unity.Burst;
using Unity.Entities;

namespace GabE.Module.ECS
{
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    partial struct ECS_UISystem : ISystem
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
