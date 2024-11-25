using GabE.Module.ECS;
using System;
using Unity.Collections;
using Unity.Entities;


/// <summary>
/// Component data for storing fragment resources.
/// </summary>
public struct ECS_Frag_Resources : IComponentData
{
    /// <summary>
    /// Wrapper struct for ResourceType to be used as a key in NativeHashMap.
    /// </summary>
    public struct ResourceTypeWrapper : IEquatable<ResourceTypeWrapper>
    {
        /// <summary>
        /// The resource type.
        /// </summary>
        public ResourceType Type;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceTypeWrapper"/> struct.
        /// </summary>
        /// <param name="type">The resource type.</param>
        public ResourceTypeWrapper(ResourceType type)
        {
            Type = type;
        }

        /// <summary>
        /// Checks if two ResourceTypeWrapper instances are equal.
        /// </summary>
        /// <param name="other">The other instance to compare with.</param>
        /// <returns>True if the resource types are equal, false otherwise.</returns>
        public bool Equals(ResourceTypeWrapper other)
        {
            return Type == other.Type;
        }
    }

    /// <summary>
    /// A NativeHashMap storing the resources and their amounts.
    /// </summary>
    public NativeHashMap<ResourceTypeWrapper, int> Resources;

    /// <summary>
    /// Initializes the resources map with all resource types and sets their initial amounts to 0.
    /// </summary>
    public void Initialize()
    {
        Resources = new NativeHashMap<ResourceTypeWrapper, int>(3, Allocator.Persistent);
        foreach (ResourceType type in Enum.GetValues(typeof(ResourceType)))
        {
            Resources[new ResourceTypeWrapper(type)] = 0;
        }
    }

    /// <summary>
    /// Adds a specified amount of a resource.
    /// </summary>
    /// <param name="type">The type of resource to add.</param>
    /// <param name="amount">The amount of resource to add.</param>
    public void AddResource(ResourceType type, int amount)
    {
        var key = new ResourceTypeWrapper(type);
        Resources[key] += amount;
    }

    /// <summary>
    /// Removes a specified amount of a resource.
    /// </summary>
    /// <param name="type">The type of resource to remove.</param>
    /// <param name="amount">The amount of resource to remove.</param>
    public void RemoveResource(ResourceType type, int amount)
    {
        var key = new ResourceTypeWrapper(type);
        Resources[key] -= amount;
    }

    /// <summary>
    /// Gets the amount of a specific resource.
    /// </summary>
    /// <param name="type">The type of resource to get the amount of.</param>
    /// <returns>The amount of the specified resource.</returns>
    public int GetResourceAmount(ResourceType type)
    {
        return Resources[new ResourceTypeWrapper(type)];
    }

    /// <summary>
    /// Disposes the NativeHashMap.
    /// </summary>
    public void Dispose()
    {
        Resources.Dispose();
    }
}
