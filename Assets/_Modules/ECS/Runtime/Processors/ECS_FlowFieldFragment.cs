using Unity.Entities;
using Unity.Mathematics;

namespace GabE.Module.ECS
{
    public struct ECS_FlowFieldFragment : IComponentData
    {
        public int2 DestinationCellIndex;

        public float2 DestinationCellPosition;

        public float2 BottomLeft;

        public int CellNumberX;
        public int CellNumberY;
    }
}
