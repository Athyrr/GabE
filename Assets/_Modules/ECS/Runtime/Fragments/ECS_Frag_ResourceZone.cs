using GabE.Module.ECS;
using Unity.Entities;

public struct ECS_Frag_ResourceZone : IComponentData
{
    public ResourceType Type;
    public float Quantity; 
}
