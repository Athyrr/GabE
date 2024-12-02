using Unity.Burst;
using Unity.Entities;

namespace GabE.Module.ECS
{
    public struct ECS_Frag_BuildListener : IComponentData
    {
        public Entity BuildingEntity;

        public ECS_Frag_BuildListener(Entity buildingEntity)
        {
            BuildingEntity = buildingEntity;
        }
    }
}
