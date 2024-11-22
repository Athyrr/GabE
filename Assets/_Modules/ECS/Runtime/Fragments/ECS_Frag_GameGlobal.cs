using Unity.Entities;

namespace GabE.Module.ECS
{
    public struct ECS_Frag_GameGlobal : IComponentData
    {
        public float Prosperity;

        public int DayCount;
        
        public int Population;
    }
}
