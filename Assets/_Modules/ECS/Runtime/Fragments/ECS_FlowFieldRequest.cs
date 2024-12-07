using Unity.Entities;
using Unity.Mathematics;

public partial struct ECS_FlowFieldRequest : IComponentData
{
    public float3 FlowFieldTargetPos;
}