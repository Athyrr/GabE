using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

public partial struct ECS_FlowFieldRequest : IComponentData
{
    public float2 FlowFieldTargetPos;

    public NativeList<Entity> queryEntities;

    public bool HasFinishedInit;
}