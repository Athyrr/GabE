using Unity.Entities;

namespace GabE.Module.ECS
{
    /// <summary>
    /// Component data of a rewarded or collectible resource.
    /// </summary>
    public struct ECS_Frag_Resource : IComponentData
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

        public struct ResourceAmounts
        {
            public ResourceType resourceType;
            public int Amount;

            public ResourceAmounts(ResourceType resourceType, int amount)
            {
                this.resourceType = resourceType;
                Amount = amount;
            }
        }

        public ResourceType Type;
    }
}
