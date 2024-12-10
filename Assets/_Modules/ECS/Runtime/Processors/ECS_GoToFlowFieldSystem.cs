using GabE.Module.ECS;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using _Modules.GE_Voxel;
using System.Diagnostics;

partial struct ECS_GoToFlowField : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        
    }

    [BurstCompile(FloatMode = FloatMode.Fast, FloatPrecision = FloatPrecision.Low, OptimizeFor = OptimizeFor.Performance, CompileSynchronously = true)]
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer commandBuffer = new EntityCommandBuffer(Allocator.TempJob);
        foreach (var (goTo, position, reqEnt) in SystemAPI.Query<RefRW<ECS_GoToFlowFieldFragment>, RefRO<ECS_PositionFragment>>().WithEntityAccess())
        {
            if (!SystemAPI.HasComponent<ECS_Frag_TargetPosition>(reqEnt))
            {
                commandBuffer.AddComponent<ECS_Frag_TargetPosition>(reqEnt);
                continue;
            }

            if(goTo.ValueRO.Index == -1)
            {
                ECS_Frag_TargetPosition posToGoFrag = SystemAPI.GetComponent<ECS_Frag_TargetPosition>(reqEnt);
                if (goTo.ValueRO.NodesPos.Length == 1)
                    goTo.ValueRW.Index = 0;
                else
                    goTo.ValueRW.Index = 1;
                float3 tempPos = goTo.ValueRO.NodesPos[goTo.ValueRO.Index];
                posToGoFrag.Position = tempPos;
                commandBuffer.SetComponent<ECS_Frag_TargetPosition>(reqEnt, posToGoFrag);

                //float3 pos2 = position.ValueRO.Position;
                //float3 cellPos2 = goTo.ValueRO.NodesPos[goTo.ValueRO.Index];
                //UnityEngine.Debug.Log((pos2, cellPos2));
            }

            float3 pos = position.ValueRO.Position;
            float3 cellPos = goTo.ValueRO.NodesPos[goTo.ValueRO.Index];

            if (math.distance(pos, cellPos) < 1f)
            {
                
                if(goTo.ValueRW.Index + 1 >= goTo.ValueRO.NodesPos.Length)
                {
                    commandBuffer.RemoveComponent<ECS_GoToFlowFieldFragment>(reqEnt);
                }
                else
                {
                    goTo.ValueRW.Index++;
                    ECS_Frag_TargetPosition posToGoFrag = SystemAPI.GetComponent<ECS_Frag_TargetPosition>(reqEnt);
                    float3 tempPos = goTo.ValueRO.NodesPos[goTo.ValueRO.Index];
                    posToGoFrag.Position = tempPos;
                    commandBuffer.SetComponent<ECS_Frag_TargetPosition>(reqEnt, posToGoFrag);
                }
            }
        }

        commandBuffer.Playback(state.EntityManager);
        commandBuffer.Dispose();
    }
}
