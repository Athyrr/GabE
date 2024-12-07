using Unity.Entities;
using Unity.Mathematics;

namespace GabE.Module.ECS
{
    public struct ECS_PositionFragment : IComponentData
    {
        public float3 Position;
        //public byte SpatialHashingID;
    }
}
