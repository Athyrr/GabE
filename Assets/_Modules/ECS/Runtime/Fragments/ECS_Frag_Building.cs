using Unity.Entities;

namespace GabE.Module.ECS
{
    public struct ECS_Frag_Building : IComponentData
    {
        public enum BuildingType
        {
            House,
            School,
            Farm,
            Bookshop,
            Museum
        }

        public BuildingType Type;

        public int Capacicty; 
    }
}
