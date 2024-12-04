using Unity.Entities;

namespace GabE.Module.ECS
{
    /// <summary>
    /// Component data for building fragments in the ECS.
    /// </summary>
    public struct ECS_BuildingFragment : IComponentData
    {
        /// <summary>
        /// The type of building.
        /// </summary>
        public BuildingType Type;

        /// <summary>
        /// The maximum capacity of the building.
        /// </summary>
        public int Capacicty;

        /// <summary>
        /// The current number of occupants in the building.
        /// </summary>
        public int Occupants;
    }
}
