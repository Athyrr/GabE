using _Modules.GE_Voxel;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace GabE.Module.ECS
{
    partial struct ECS_FlowFieldProcessSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {

        }

        [BurstCompile(FloatMode = FloatMode.Fast, FloatPrecision = FloatPrecision.Low, OptimizeFor = OptimizeFor.Performance, CompileSynchronously = false)]
        public void OnUpdate(ref SystemState state)
        {
            EntityCommandBuffer commandBuffer = new EntityCommandBuffer(Allocator.TempJob);
            int yCellNumber;
            foreach (var (buffer, destinationCell, tag, reqEnt) in SystemAPI.Query<DynamicBuffer<ECS_CellBufferFragment>, RefRO<ECS_FlowFieldFragment>, RefRO<ECS_FlowFieldProcessTag>>().WithEntityAccess())
            {
                DynamicBuffer<Entity> cellsBuffer = buffer.Reinterpret<Entity>();
                NativeArray<ECS_CellFragment> cellsData = new NativeArray<ECS_CellFragment>(GE_VoxelRunner.Instance.chunkLoop * GE_VoxelRunner.Instance.chunkLoop * GE_VoxelRunner.Instance.chunkSize * GE_VoxelRunner.Instance.chunkSize, Allocator.TempJob);

                for (int i = 0; i < cellsBuffer.Length; i++)
                {
                    cellsData[i] = SystemAPI.GetComponent<ECS_CellFragment>(cellsBuffer[i]);
                }

                yCellNumber = destinationCell.ValueRO.CellNumberY;


                int flatIndex = FlatIndex(destinationCell.ValueRO.DestinationCellIndex.x, destinationCell.ValueRO.DestinationCellIndex.y);
                float2 cellPos = cellsData[flatIndex].CellPos;
                UnityEngine.Debug.DrawLine(new float3(cellPos.x, 0, cellPos.y), new float3(cellPos.x, 100, cellPos.y), Color.magenta, 100f);
                

                ECS_CellFragment destinationCellFrag = cellsData[flatIndex];
                destinationCellFrag.BestCost = 0;
                cellsData[flatIndex] = destinationCellFrag;

                NativeQueue<int2> indices = new NativeQueue<int2>(Allocator.TempJob);
                NativeList<int2> neighbours = new NativeList<int2>(Allocator.TempJob);

                indices.Enqueue(destinationCellFrag.CellIndex);


                int j = 0;
                while (indices.Count > 0)
                {
                    int2 index = indices.Dequeue();
                    int cellIndex = FlatIndex(index.x, index.y);
                    ECS_CellFragment cellFrag = cellsData[cellIndex];
                    neighbours.Clear();
                    GetNeighboursFlatIndexes(cellFrag, destinationCell.ValueRO.CellNumberX, destinationCell.ValueRO.CellNumberY, ref neighbours);

                    foreach (var neighbour in neighbours)
                    {
                        int neighbourFlatIndex = FlatIndex(neighbour.x, neighbour.y);
                        ECS_CellFragment neighbourCell = cellsData[neighbourFlatIndex];

                        float diagonalCostMultiplier = 1;
                        if (neighbour.x - index.x != 0 && neighbour.y - index.y != 0)
                        {
                            diagonalCostMultiplier = 1.4f;
                        }


                        if (neighbourCell.Cost * diagonalCostMultiplier + cellFrag.BestCost < neighbourCell.BestCost)
                        {

                            neighbourCell.BestCost = neighbourCell.Cost * diagonalCostMultiplier + cellFrag.BestCost;
                            cellsData[neighbourFlatIndex] = neighbourCell;
                            indices.Enqueue(new int2(neighbour.x, neighbour.y));
                        }
                        j++;
                    }
                }

                for (int i = 0; i < cellsData.Length; i++)
                {
                    ECS_CellFragment cellData = cellsData[i];
                    float bestCost = cellData.BestCost;
                    int2 bestDirection = int2.zero;
                    neighbours.Clear();
                    GetNeighboursFlatIndexes(cellData, destinationCell.ValueRO.CellNumberX, destinationCell.ValueRO.CellNumberY, ref neighbours);
                    foreach (var neighbour in neighbours)
                    {
                        int neighbourFlatIndex = FlatIndex(neighbour.x, neighbour.y);
                        ECS_CellFragment neighboursCell = cellsData[neighbourFlatIndex];
                        //if (i % 20 == 0)
                        //{
                        //    Debug.Log("1: " + new float3(cellData.CellPos.x, 0, cellData.CellPos.y));
                        //    Debug.Log("2: " + new float3(neighboursCell.CellPos.x, 0, neighboursCell.CellPos.y));
                        //    Debug.DrawLine(new float3(cellData.CellPos.x, 0, cellData.CellPos.y), new float3(neighboursCell.CellPos.x, 0, neighboursCell.CellPos.y), Color.red, 100f);
                        //}
                        if (neighboursCell.BestCost < bestCost)
                        {
                            //if (neighboursCell.BestCost == 0)
                            //{
                            //    UnityEngine.Debug.Log(cellsData[i].CellIndex);
                            //    UnityEngine.Debug.Log(neighboursCell.CellIndex);
                            //}

                            bestCost = neighboursCell.BestCost;
                            bestDirection = neighboursCell.CellIndex - cellData.CellIndex;
                        }
                    }

                    cellData.BestDirection = bestDirection;
                    cellsData[i] = cellData;
                }

                for (int i = 0; i < cellsBuffer.Length; i++)
                {
                    commandBuffer.SetComponent(cellsBuffer[i], cellsData[i]);
                    UnityEngine.Debug.DrawLine(new float3(cellsData[i].CellPos.x, 0, cellsData[i].CellPos.y),
                        new float3(cellsData[i].CellPos.x, 0, cellsData[i].CellPos.y)
                        + new float3(cellsData[i].BestDirection.x, 0, cellsData[i].BestDirection.y), new UnityEngine.Color(cellsData[i].BestCost, cellsData[i].BestCost / 10, 0), 10f);
                }

                indices.Dispose();
                neighbours.Dispose();

                NativeList<(Entity, float3)> entities = new NativeList<(Entity, float3)>(Allocator.TempJob);
                foreach (var req in SystemAPI.Query<RefRO<ECS_FlowFieldRequest>>())
                {
                    foreach (var ent in req.ValueRO.queryEntities)
                    {
                        entities.Add((ent, SystemAPI.GetComponent<ECS_PositionFragment>(ent).Position));
                    }
                }

                foreach (var entity in entities)
                {
                    NativeList<float3> positionsToGo = new NativeList<float3>(Allocator.TempJob);
                    int2 entityGridIndex = GetNodeIndexFromPosAndOrigin(entity.Item2, 1, destinationCell.ValueRO.BottomLeft);
                    int entityFlatIndex = FlatIndex(entityGridIndex.x, entityGridIndex.y);


                    ECS_CellFragment data = cellsData[entityFlatIndex];
                    float2 lastDirection = float2.zero;

                    while (true)
                    {
                        int flat = GE_VoxelRunner.Instance.GetFlatIndexFromNodePos((int2)data.CellPos);
                        int y = GE_VoxelRunner.Instance.GetYFromFlatIndex(flat);
                        positionsToGo.Add(new Vector3(data.CellPos.x, y + 2.5f, data.CellPos.y));
                        int nextFlat = FlatIndex((int)data.CellIndex.x + data.BestDirection.x, (int)data.CellIndex.y + data.BestDirection.y);
                        if (data.CellIndex.x == cellsData[nextFlat].CellIndex.x && data.CellIndex.y == cellsData[nextFlat].CellIndex.y)
                            break;
                        data = cellsData[nextFlat];
                    }

                    commandBuffer.AddComponent<ECS_GoToFlowFieldFragment>(entity.Item1, new ECS_GoToFlowFieldFragment { NodesPos = positionsToGo, Index = -1 });
                }

                foreach (var (req, entity) in SystemAPI.Query<RefRO<ECS_FlowFieldRequest>>().WithEntityAccess())
                {
                    commandBuffer.DestroyEntity(entity);
                }

                foreach (var cell in cellsBuffer)
                {
                    commandBuffer.DestroyEntity(cell);
                }

                commandBuffer.DestroyEntity(reqEnt);
                //commandBuffer.RemoveComponent<ECS_FlowFieldProcessTag>(reqEnt);

                cellsData.Dispose();

            }

            int2 GetNodeIndexFromPosAndOrigin(float3 pos, float chunckSize, float2 bottomLeftOrigin)
            {
                return new int2((int)math.floor(math.abs(pos.x - bottomLeftOrigin.x) / chunckSize),
                    (int)math.floor(math.abs(pos.z - bottomLeftOrigin.y) / chunckSize));
            }

            int FlatIndex(int x, int y)
            {
                return x * yCellNumber + y;
            }

            NativeList<int2> GetNeighboursFlatIndexes(ECS_CellFragment cellData, int xChuncks, int yChuncks, ref NativeList<int2> result)
            {
                //int indexX = 0;
                //int indexY = 0;
                //for (int x = -1; x <= -1; x++)
                //{
                //    for (int y = -1; y <= 1; y++)
                //    {
                //        int neighbourXIndex = cellIndex.x + x;
                //        int neighbourYIndex = cellIndex.y + y;
                //        if (!GE_VoxelRunner.Instance.collisionsMap[cellIndex.x, cellIndex.y][indexX + indexY])
                //            result.Add(new int2(neighbourXIndex, neighbourYIndex));
                //        indexX++;
                //    }
                //    indexY += 3;
                //    indexX = 0;
                //}


                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {

                        //UnityEngine.Debug.Log("x: " + cellData.CellIndex.x + x);
                        //UnityEngine.Debug.Log("y : " + cellData.CellIndex.y + y);

                        if (x == 0 && y == 0)
                            continue;
                        int neighbourXIndex = cellData.CellIndex.x + x;
                        if (neighbourXIndex < 0 || neighbourXIndex >= xChuncks)
                            continue;
                        int neighbourYIndex = cellData.CellIndex.y + y;
                        if (neighbourYIndex < 0 || neighbourYIndex >= yChuncks)
                            continue;


                        int voxelFlatIndex = GE_VoxelRunner.Instance.GetFlatIndexFromNodePos((int2)cellData.CellPos);

                        //UnityEngine.Debug.Log("eeee");

                        int neighbourFlatIndex = GE_VoxelRunner.Instance.GetSpecificNeighbourIndexFromNodeIndexes(voxelFlatIndex, x, y);
                        if (neighbourFlatIndex < 0)
                            continue;

                        //if(cellData.CellIndex.x == 29 && cellData.CellIndex.y == 29)
                        //{
                        //    //Debug.Log("cell pos: " + cellData.CellPos);
                        //    //int2 index = GE_VoxelRunner.Instance.GetNodeIndexFromFlatIndex(voxelFlatIndex);
                        //    //int2 pos = GE_VoxelRunner.Instance.GetPosFromIndex(index);
                        //    //Debug.Log("cell pos after process: " + pos);
                        //    //UnityEngine.Debug.DrawLine(new float3(chunckX, 0, chunckY), new float3(chunckX, 100, chunckY), Color.blue, 100f);
                        //    int2 cellIndex = GE_VoxelRunner.Instance.GetNodeIndexFromFlatIndex(neighbourFlatIndex);
                        //    int2 cellPos = GE_VoxelRunner.Instance.GetPosFromIndex(cellIndex);
                        //    UnityEngine.Debug.Log(cellPos);
                        //    UnityEngine.Debug.DrawLine(new float3(cellPos.x, 0, cellPos.y), new float3(cellPos.x, 100, cellPos.y), Color.blue, 100f);
                        //}



                        if (GE_VoxelRunner.Instance.collisionsMap[neighbourFlatIndex])
                            continue;


                        result.Add(new int2(neighbourXIndex, neighbourYIndex));

                    }
                }

                return result;
            }

            commandBuffer.Playback(state.EntityManager);
            commandBuffer.Dispose();
        }
    }
}
