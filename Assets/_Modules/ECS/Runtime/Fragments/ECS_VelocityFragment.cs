using Unity.Entities;

namespace GabE.Module.ECS
{
    /// <summary>
    /// Represents the velocity of an entity.
    /// </summary>
    public struct ECS_Frag_Velocity : IComponentData
    {
        /// <summary>
        /// The velocity value.
        /// </summary>
        public float Value;
    }
}
