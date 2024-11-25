using Unity.Entities;

namespace GabE.Module.ECS
{
    public struct ECS_Frag_Building : IComponentData
    {


        public BuildingType Type;

        public int Capacicty; 
    }
}
