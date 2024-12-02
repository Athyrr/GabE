using Unity.Entities;

using GabE.Module.ECS;


/// <summary>
/// Component data for storing fragment resource.
/// </summary>
public struct ECS_Frag_Resource : IBufferElementData
{
    public ResourceType ResourceType;
    public int Quantity;
}