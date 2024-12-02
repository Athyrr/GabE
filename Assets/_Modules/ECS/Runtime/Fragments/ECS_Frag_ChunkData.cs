using Unity.Collections;
using Unity.Entities;


public struct ECS_Frag_ChunkData : IBufferElementData
{
    public FixedList4096Bytes<byte> cubes;
}
