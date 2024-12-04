using Unity.Burst;
using Unity.Entities;

namespace GabE.Module.ECS
{
    public struct ECS_BuildListenerFragment : IComponentData
    {
        public Entity BuildingEntity;

        public ECS_BuildListenerFragment(Entity buildingEntity)
        {
            BuildingEntity = buildingEntity;
        }
    }
}
