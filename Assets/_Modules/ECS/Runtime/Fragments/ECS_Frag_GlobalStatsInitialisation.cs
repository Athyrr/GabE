using Unity.Entities;

namespace GabE.Module.ECS
{
    public struct ECS_Frag_GlobalStatsInitialisation : IComponentData
    {
        public float BaseVelocity;
        public int LifeExpectancy;
        public int MinAge;
    }
}
