using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

public struct ECS_GoToFlowFieldFragment : IComponentData
{
    public NativeList<float3> NodesPos;
    public int Index;
}

