using Unity.Entities;

namespace GabE.Module.ECS
{
    public struct ECS_Frag_GameGlobal : IComponentData
    {
        public int DayCount;

        public float Prosperity;

        public int Population;
    }
}
