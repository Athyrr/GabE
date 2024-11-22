using Unity.Collections;
using Unity.Entities;

namespace GabE.Module.ECS
{
    /// <summary>
    /// Component data of a rewarded or collectible resource.
    /// </summary>
    public struct ECS_Frag_Resources : IComponentData
    {
        /// <summary>
        /// Represents the types of resources available.
        /// </summary>
        public enum ResourceType
        {
            /// <summary>
            /// Represents food resource.
            /// </summary>
            Food = 0,
            /// <summary>
            /// Represents wood resource.
            /// </summary>
            Wood = 1,
            /// <summary>
            /// Represents stone resource.
            /// </summary>
            Stone = 2,
        }

        public NativeKeyValueArrays<ResourceType, int> resourceTypes; //Instead of Dictionary
    }
}
