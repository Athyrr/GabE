using Unity.Entities;

using GabE.Module.ECS;


/// <summary>
/// Represents a resource in the global inventory.
/// </summary>
public struct ECS_ResourceStorageFragment : IBufferElementData
{
    public ResourceType Type;

    public int Quantity;
}