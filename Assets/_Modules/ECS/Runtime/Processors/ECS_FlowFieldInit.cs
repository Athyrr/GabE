using _Modules.GE_Voxel;
using GabE.Module.ECS;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

//UpdateAfter[]
public partial struct ECS_FlowFieldInit : ISystem
{

    [BurstCompile(FloatMode = FloatMode.Fast, FloatPrecision = FloatPrecision.Low, OptimizeFor = OptimizeFor.Performance, CompileSynchronously = false)]
    public void OnUpdate(ref SystemState state)
    {
        //if (Input.GetKeyDown(KeyCode.Mouse0))
        //{
        //    GE_VoxelRunner.Instance.RecalculateCollisionsMap(true);
        //    Entity e2 = state.EntityManager.CreateEntity();
        //    state.EntityManager.AddComponent<ECS_FlowFieldRequest>(e2);
        //    state.EntityManager.SetComponentData<ECS_FlowFieldRequest>(e2, new ECS_FlowFieldRequest() { FlowFieldTargetPos = new float2(100, -60) });
        //    EntityCommandBuffer entitiesBuffer = new EntityCommandBuffer(Allocator.TempJob);

        //    foreach (var request in SystemAPI.Query<RefRW<ECS_FlowFieldRequest>>())
        //    {
        //        int i = 0;

        //        request.ValueRW.queryEntities = new NativeList<Entity>(Allocator.Temp);
        //        foreach (var (req2, entity) in SystemAPI.Query<RefRO<ECS_PersonFragment>>().WithEntityAccess())
        //        {
        //            request.ValueRW.queryEntities.Add(entity);
        //            i++;
        //        }
        //    }

        //    entitiesBuffer.Playback(state.EntityManager);
        //    entitiesBuffer.Dispose();
        //}
        EntityCommandBuffer buffer = new EntityCommandBuffer(Allocator.TempJob);
        foreach (var req1 in SystemAPI.Query<RefRW<ECS_FlowFieldRequest>>())
        {
            if (req1.ValueRO.HasFinishedInit)
                break;
            var reqPos = req1.ValueRO.FlowFieldTargetPos;

            UnityEngine.Debug.Log(req1.ValueRO.HasFinishedInit);


            NativeList<(Entity, float3)> entities = new NativeList<(Entity, float3)>(Allocator.Temp);

            foreach (var entity in req1.ValueRO.queryEntities)
            {
                entities.Add((entity, SystemAPI.GetComponent<ECS_PositionFragment>(entity).Position));
            }


            NativeList<float2> entityChuncks = new NativeList<float2>(Allocator.TempJob);

            GetEntityChuncks(entities, ref entityChuncks);


            float2 queryChunck = GetChunckPosFromPos(new float3(req1.ValueRO.FlowFieldTargetPos.x, 0, req1.ValueRO.FlowFieldTargetPos.y), 30, true);

            bool canAdd = true;
            for (int k = 0; k < entityChuncks.Length; k++)
            {
                if (Equals(entityChuncks[k], queryChunck))
                    canAdd = false;
            }
            if (canAdd)
            {
                entityChuncks.Add(queryChunck);
            }



            float extremLeft = float.PositiveInfinity;
            float extremRight = float.NegativeInfinity;
            float extremUp = float.NegativeInfinity;
            float extremBottom = float.PositiveInfinity;

            foreach (var chunck in entityChuncks)
            {
                if (chunck.x < extremLeft)
                    extremLeft = chunck.x;
                if (chunck.x > extremRight)
                    extremRight = chunck.x;
                if (chunck.y > extremUp)
                    extremUp = chunck.y;
                if (chunck.y < extremBottom)
                    extremBottom = chunck.y;
            }

            extremLeft -= 15;
            extremRight += 15;
            extremUp += 15;
            extremBottom -= 15;


            Entity grid = buffer.CreateEntity();
            DynamicBuffer<ECS_CellBufferFragment> cellStor = buffer.AddBuffer<ECS_CellBufferFragment>(grid);



            int indexX = 0;
            int indexY = 0;
            for (float x = extremLeft; x < extremRight; x++)
            {
                indexY = 0;
                for (float y = extremBottom; y < extremUp; y++)
                {
                    Entity e = buffer.CreateEntity();
                    buffer.AddComponent<ECS_CellFragment>(e, new ECS_CellFragment
                    {
                        CellPos = new float2(x, y),
                        CellIndex = new int2(indexX, indexY),
                        Cost = 1,
                        BestCost = int.MaxValue
                    });
                    cellStor.Add(e);
                    //UnityEngine.Debug.DrawLine(new float3(x + 0.5f, 0, y + 0.5f), new float3(x + 0.5f, 10, y + 0.5f), UnityEngine.Color.yellow, 100f);
                    indexY++;
                }
                indexX++;
            }

            //foreach (var item in entityChuncks)
            //{
            //    UnityEngine.Debug.DrawLine(new float3(item.x, 0, item.y), new float3(item.x, 100, item.y), UnityEngine.Color.red, 100f);
            //}
            //new float2(extremLeft, extremUp)
            //UnityEngine.Debug.Log(reqPos.x);
            int2 reqNodeIndex = (int2)GetNodeIndexFromPosAndOrigin(new float3(reqPos.x, 0, reqPos.y), 1, new float2(extremLeft, extremBottom));
            //UnityEngine.Debug.Log(reqNodeIndex);
            //UnityEngine.Debug.DrawLine(new float3(reqNodeIndex.x, 0, reqNodeIndex.y), new float3(reqNodeIndex.x, 100, reqNodeIndex.y), UnityEngine.Color.green, 100f);
            buffer.AddComponent<ECS_FlowFieldFragment>(grid);

            buffer.SetComponent<ECS_FlowFieldFragment>(grid, new ECS_FlowFieldFragment()
            {
                DestinationCellPosition = reqPos,
                DestinationCellIndex = reqNodeIndex,
                BottomLeft = new float2(extremLeft, extremBottom),
                CellNumberX = indexX,
                CellNumberY = indexY
            });

            buffer.AddComponent<ECS_FlowFieldProcessTag>(grid);

            //cellStor.Reinterpret<ECS_CellFragment>();

            //int flatIndex = reqNodeIndex.x * indexY + reqNodeIndex.y;

            //float2 pos = cellStor[flatIndex].CellPos;

            //UnityEngine.Debug.DrawLine(new float3(pos.x, 0, pos.y), new float3(pos.x, 100, pos.y), UnityEngine.Color.black, 100f);


            req1.ValueRW.HasFinishedInit = true;

            entities.Dispose();
            entityChuncks.Dispose();

        }

        buffer.Playback(state.EntityManager);
        buffer.Dispose();

        float2 GetChunckPosFromPos(float3 pos, float chunckSize, bool GoToCenterOfChunck)
        {
            return new float2(math.floor(pos.x / chunckSize) * chunckSize + (GoToCenterOfChunck ? chunckSize * 0.5f : 0),
                math.floor(pos.z / chunckSize) * chunckSize + (GoToCenterOfChunck ? chunckSize * 0.5f : 0));
        }

        float2 GetNodeIndexFromPosAndOrigin(float3 pos, float chunckSize, float2 bottomLeftOrigin)
        {
            return new float2(math.floor(math.abs(pos.x - bottomLeftOrigin.x) / chunckSize),
                math.floor(math.abs(pos.z - bottomLeftOrigin.y) / chunckSize));
        }


        void GetEntityChuncks(NativeList<(Entity, float3)> entities, ref NativeList<float2> chuncks)
        {
            foreach (var ent in entities)
            {
                float2 chunckIds = GetChunckPosFromPos(ent.Item2, 30, true);
                bool canAdd = true;
                for (int k = 0; k < chuncks.Length; k++)
                {
                    if (Equals(chuncks[k], chunckIds))
                        canAdd = false;
                }
                if (canAdd)
                {
                    chuncks.Add(chunckIds);
                }
            }

            foreach (var chunck in chuncks)
            {
                UnityEngine.Debug.DrawLine(new float3(chunck.x, 0, chunck.y), new float3(chunck.x, 100, chunck.y), UnityEngine.Color.blue, 100f);
            }
        }
    }


}
