using Unity.Entities;
using Unity.Mathematics;

namespace GabE.Module.ECS
{
    public struct ECS_Frag_TargetPosition : IComponentData
    {
        public float3 Position;
    }
}
