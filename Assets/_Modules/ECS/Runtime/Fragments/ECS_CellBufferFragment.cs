using Unity.Entities;

namespace GabE.Module.ECS
{
    public struct ECS_CellBufferFragment : IBufferElementData
    {
        public Entity entity;

        public static implicit operator Entity(ECS_CellBufferFragment buffer)
        {
            return buffer.entity;
        }

        public static implicit operator ECS_CellBufferFragment(Entity ent)
        {
            return new ECS_CellBufferFragment { entity = ent };
        }
    }
}
