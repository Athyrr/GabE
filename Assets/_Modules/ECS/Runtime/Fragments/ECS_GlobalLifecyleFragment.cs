using Unity.Entities;

namespace GabE.Module.ECS
{
    public struct ECS_GlobalLifecyleFragment : IComponentData
    {
        public int DayCount;

        public float Prosperity;

        public int Population;
    }
}
