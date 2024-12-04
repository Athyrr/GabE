using GabE.Module.ECS;
using Unity.Entities;


/// <summary>
/// Represents a resource in the global inventory.
/// </summary>
public struct ECS_ResourceStorageFragment : IBufferElementData
{
    public ResourceType Type;
    public int Quantity;
}