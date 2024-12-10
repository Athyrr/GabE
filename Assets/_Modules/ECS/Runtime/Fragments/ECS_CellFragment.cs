using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;


public partial struct ECS_CellFragment : IComponentData
{
    public float2 CellPos;

    public int2 CellIndex;

    public NativeList<bool> neighboursCollision;

    public float Cost;

    public float BestCost;

    public int2 BestDirection;
}

