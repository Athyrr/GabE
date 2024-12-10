using _Modules.GE_Voxel.Utils;
using System;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;


namespace _Modules.GE_Voxel
{
    [CreateAssetMenu(fileName = "GE Voxel Runner", menuName = "GE", order = 0)]
    public class GE_VoxelRunner : MonoBehaviour
    {
        public byte chunkSize;
        public byte yMax;
        public byte cubeOffset;
        public byte chunkOffset;
        public byte chunkLoop;
        public Material material;
        public GE_VoxelChunk[] _chunks;
        public Camera _playerCamera;
        public GameObject foliageMesh;
        public bool debug = false;

        public NativeArray<byte> collisionsHeightMap;
        public NativeArray<bool> collisionsMap;

        private float _lastCameraPositionValue; // x + z

        public static GE_VoxelRunner Instance;

        private int _maxFlatIndex;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
        }

        private void Start()
        {
            if (!_playerCamera)
                throw new Exception("No player camera available");

            _chunks = new GE_VoxelChunk[chunkLoop * chunkLoop];

            collisionsHeightMap = new NativeArray<byte>(chunkLoop * chunkLoop * chunkSize * chunkSize, Allocator.TempJob);
            collisionsMap = new NativeArray<bool>(chunkLoop * chunkLoop * chunkSize * chunkSize * 8, Allocator.TempJob);

            foreach (var chunk in _chunks)
            {

            }

            enabled = false;

            for (byte i = 0; i < chunkLoop; i++)
            {
                for (byte j = 0; j < chunkLoop; j++)
                {
                    Vector3 p = new Vector3(chunkSize * i - chunkLoop * chunkSize / 2 + chunkSize / 2, 0, chunkSize * j - chunkLoop * chunkSize / 2 + chunkSize / 2);

                    GE_VoxelChunk e = new GE_VoxelChunk(new GameObject(), p, chunkSize, 1, yMax, material, foliageMesh);

                    e.Load();
                    _chunks[i + j * chunkLoop] = e;
                }
            }
            foreach (var e in _chunks)
            {
                e.UpdateLOD(2);
            }
            foreach (var e in _chunks)
            {
                e.UpdateLOD(4);
            }
            foreach (var e in _chunks)
            {
                e.UpdateLOD(1);
            }

            _maxFlatIndex = chunkLoop * chunkLoop * chunkSize * chunkSize;

            enabled = true;
            //InvokeRepeating("GE_Update",0.5f,0.5f);
        }

        private void Update()
        {
#if UNITY_EDITOR
            if (debug)
                for (byte i = 0; i < _chunks.Length; ++i)
                {
                    int x = i % chunkLoop;
                    int y = (int)math.floor(i / chunkLoop); // because chunkSize cannot be 0
                    Vector3 chunkPos = new Vector3(
                        x + x * chunkSize + chunkSize * 0.5f - chunkLoop * chunkSize * 0.5f,
                        0f,
                        y + y * chunkSize + chunkSize * 0.5f - chunkLoop * chunkSize * 0.5f);
                    GE_Debug.DrawBox(
                        chunkPos,
                        Quaternion.identity,
                        new Vector3(chunkSize, yMax, chunkSize),
                        Color.blue
                    );
                }
#endif
        }

        private void GE_Update()
        {

            Vector2 cPos = new Vector2(_playerCamera.transform.position.x, _playerCamera.transform.position.z);
            float tmpValue = cPos.x + cPos.y;

            if (_lastCameraPositionValue < tmpValue + 0.1f && _lastCameraPositionValue > tmpValue - 0.1f)
                return;
            else _lastCameraPositionValue = tmpValue;

            for (byte i = 0; i < _chunks.Length; ++i)
            {
                GE_VoxelChunk e = _chunks[i];
                int x = i % chunkLoop;
                int y = (int)math.floor(i / chunkLoop); // because chunkSize cannot be 0
                //_playerCamera.transform.position
                Vector3 chunkPos = new Vector3(
                    x + x * chunkSize + chunkSize * 0.5f - chunkLoop * chunkSize * 0.25f,
                    0f,
                    y + y * chunkSize + chunkSize * 0.5f - chunkLoop * chunkSize * 0.25f);

                float cPosF = chunkPos.x + chunkPos.y + chunkSize * 0.5f;
                float d = math.distance(tmpValue, cPosF);

            }
        }

        public void RecalculateCollisionsMap(bool allchuncks, int chunkFlatIndex = 0)
        {
            for (int i = 0; i < _chunks.Length; i++)
            {
                for (int j = 0; j < _chunks[i]._nchunk.Length; j++)
                {
                    if (collisionsHeightMap[i * chunkSize * chunkSize + j] < 100)
                        collisionsHeightMap[i * chunkSize * chunkSize + j] = _chunks[i]._nchunkQuiFonctionne[j];
                }
            }

            for (int i = 0; i < collisionsHeightMap.Length; i++)
            {
                int[] neighboursIndex = GetNeighboursIndexFromNodeIndexes(i);
                for (int j = 0; j < 8; j++)
                {
                    collisionsMap[i * 8 + j] = !(neighboursIndex[j] >= 0 && (collisionsHeightMap[neighboursIndex[j]] - collisionsHeightMap[i]) < 2);
                }
            }

            //for (int i = 0; i < _chunks.Length; i++)
            //{
            //    for (int j = 0; j < _chunks[i]._nchunk.Length; j++)
            //    {
            //        int index = 0;
            //        for (int x = -1; x <= 1; x++)
            //        {
            //            for (int y = -1; y <= 1; y++)
            //            {
            //                if (x == 0 && y == 0) continue;
            //                int xNeighbour = i + x;
            //                int yNeighbour = i + y;
            //                Debug.Log(index);
            //                int nodeIndex = i * chunkSize * chunkSize + j + index;
            //                int neighbourQuery = i * chunkSize * chunkSize + j;
            //                if (xNeighbour <= 0 || xNeighbour >= chunkLoop || yNeighbour <= 0 || yNeighbour >= chunkLoop)
            //                {
            //                    collisionsMap[i, j][index] = true;
            //                }
            //                else if (collisionsHeightMap[xNeighbour, yNeighbour] >= 100)
            //                {
            //                    collisionsMap[i, j][index] = true;
            //                }
            //                else
            //                {
            //                    collisionsMap[i * chunkSize * chunkSize + j + index] = math.abs(collisionsHeightMap[xNeighbour, yNeighbour] - collisionsHeightMap[i, j]) > 1;
            //                }
            //                index++;
            //            }
            //        }
            //    }
            //}

            //collisionsHeightMap = new byte[chunkLoop * chunkSize, chunkLoop * chunkSize];
            //for (int i = 0; i < _chunks.Length; i++)
            //{
            //    for (int j = 0; j < _chunks[i]._nchunkQuiFonctionne.Length; j++)
            //    {
            //        if(collisionsHeightMap[i, j] < 100)
            //            collisionsHeightMap[i, j] = _chunks[i]._nchunkQuiFonctionne[j];
            //    }
            //}
            //for (int i = 0; i < chunkLoop * chunkSize; i++)
            //{
            //    for (int j = 0; j < chunkLoop * chunkSize; j++)
            //    {
            //        collisionsMap[i, j] = new bool[8];
            //        int index = 0;
            //        for (int x = -1; x <= 1; x++)
            //        {
            //            for (int y = -1; y <= 1; y++)
            //            {
            //                if (x == 0 && y == 0) continue;

            //                int xNeighbour = i + x;
            //                int yNeighbour = i + y;
            //                Debug.Log(index);
            //                if (xNeighbour <= 0 || xNeighbour >= chunkLoop || yNeighbour <= 0 || yNeighbour >= chunkLoop)
            //                {
            //                    collisionsMap[i, j][index] = true;
            //                }
            //                else if(collisionsHeightMap[xNeighbour, yNeighbour] >= 100)
            //                {
            //                    collisionsMap[i, j][index] = true;
            //                }
            //                else
            //                {
            //                    collisionsMap[i, j][index] = math.abs(collisionsHeightMap[xNeighbour, yNeighbour] - collisionsHeightMap[i, j]) > 1;
            //                }
            //                index++;
            //            }
            //        }
            //    }
            //}
        }

        public int[] GetNeighboursIndexFromNodeIndexes(int nodeFlat)
        {
            int[] neighboursIndex = new int[8];
            int i = 0;
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (x == 0 && y == 0) continue;

                    int2 index = GetNodeIndexFromFlatIndex(nodeFlat);
                    index = new int2(index.x + x, index.y + y);
                    if (index.x < 0 || index.y < 0 || index.x >= chunkSize * chunkLoop || index.y >= chunkSize * chunkLoop)
                    {
                        neighboursIndex[i] = -1;
                        continue;
                    }
                    neighboursIndex[i] = GetFlatIndexFromNodePos(index, true);
                    i++;
                    //int neighbourFlatIndex = nodeFlat;
                    //if (x == 0 && y == 0) continue;

                    //neighbourFlatIndex += x * chunkSize;
                    //if (neighbourFlatIndex < 0 || neighbourFlatIndex >= _maxFlatIndex)
                    //{
                    //    neighboursIndex[i] = -1;
                    //    continue;
                    //}

                    //neighbourFlatIndex += y * chunkLoop * chunkSize * chunkSize;
                    //if (neighbourFlatIndex < 0 || neighbourFlatIndex >= _maxFlatIndex)
                    //{
                    //    neighboursIndex[i] = -1;
                    //    continue;
                    //}
                    //neighboursIndex[i] = neighbourFlatIndex;
                    //if (x == -1 && indexX % chunkSize == 0)
                    //{
                    //    neighbourX = indexX - chunkSize;
                    //}
                    //else if(x == 1 && chunckIndex.Item1 < chunkLoop && (indexX + 1) % chunkSize == 0)
                    //{
                    //    neighbourX = indexX + chunkSize;
                    //}
                    //else
                    //{
                    //    neighbourX = indexX + x * chunkSize;
                    //}
                }
            }

            return neighboursIndex;
        }

        public int GetSpecificNeighbourIndexFromNodeIndexes(int nodeFlat, int x, int y)
        {
            //int neighboursIndex = nodeFlat;

            //neighboursIndex += x * chunkSize;


            //if (neighboursIndex < 0 || neighboursIndex >= _maxFlatIndex)
            //{
            //    return -1;
            //}

            //neighboursIndex += y * chunkLoop * chunkSize * chunkSize;
            //if (neighboursIndex < 0 || neighboursIndex >= _maxFlatIndex)
            //{
            //    return -1;
            //}

            int2 index = GetNodeIndexFromFlatIndex(nodeFlat);
            index = new int2(index.x + x, index.y + y);
            if (index.x < 0 || index.y < 0 || index.x >= chunkSize * chunkLoop || index.y >= chunkSize * chunkLoop)
                return -1;
            int neighbourFlatIndex = GetFlatIndexFromNodePos(index, true);

            return neighbourFlatIndex;
            //if (x == -1 && indexX % chunkSize == 0)
            //{
            //    neighbourX = indexX - chunkSize;
            //}
            //else if(x == 1 && chunckIndex.Item1 < chunkLoop && (indexX + 1) % chunkSize == 0)
            //{
            //    neighbourX = indexX + chunkSize;
            //}
            //else
            //{
            //    neighbourX = indexX + x * chunkSize;
            //}
        }

        public int GetFlatIndexFromNodePos(int2 pos, bool isOnRelative = false)
        {
            int2 nodeIndex = pos;
            if (!isOnRelative)
            {
                nodeIndex += chunkLoop * chunkSize / 2;
            }
            int2 chunckIndex = nodeIndex / chunkSize;
            int2 relativeNodeIndex = nodeIndex - (chunckIndex * chunkSize);
            int chunckFlatIndex = chunckIndex.x * chunkLoop * chunkSize * chunkSize + chunckIndex.y * chunkSize * chunkSize;
            int relativeFlatNodeIndex = relativeNodeIndex.x * chunkSize + relativeNodeIndex.y;
            return chunckFlatIndex + relativeFlatNodeIndex;
        }

        public int2 GetNodeIndexFromFlatIndex(int flat)
        {
            int chunckX = flat / (chunkSize * chunkSize * chunkLoop);
            int chunckY = (flat % (chunkSize * chunkSize * chunkLoop)) / (chunkSize * chunkSize);

            int firstChunkNodeID = chunckX * chunkSize * chunkSize * chunkLoop + chunckY * chunkSize * chunkSize;

            int relativeNodeFlat = flat - firstChunkNodeID;

            int nodePosX = relativeNodeFlat / chunkSize;
            int nodePosY = relativeNodeFlat % chunkSize;

            int chunkPosX = chunckX * chunkSize;
            int chunkPosY = chunckY * chunkSize;

            return new int2(chunkPosX + nodePosX, chunkPosY + nodePosY);
        }

        public int2 GetPosFromIndex(int2 index)
        {
            return new int2(index.x - chunkLoop * chunkSize / 2, index.y - chunkLoop * chunkSize / 2);
        }

        public bool[] GetNeighboursCollisionsFromFlatIndex(int index)
        {
            int[] neigbhoursIndex = GetNeighboursIndexFromNodeIndexes(index);
            bool[] cols = new bool[8];
            for (int i = 0; i < neigbhoursIndex.Length; i++)
            {
                cols[i] = collisionsMap[neigbhoursIndex[i]];
            }

            return cols;
        }

        public int GetYFromFlatIndex(int index)
        {
            //int chunckX = index / (chunkSize * chunkSize * chunkLoop);
            //int chunckY = (index % (chunkSize * chunkSize * chunkLoop)) / (chunkSize * chunkSize);

            //int chunckFlatIndex = chunckX * chunkLoop + chunckY;

            //int chunckNodeFlatIndex = chunckX * chunkSize * chunkSize * chunkLoop + chunckY * chunkSize * chunkSize;

            //int relativeFlatIndex = index - chunckNodeFlatIndex;

            int chunckX = index / (chunkSize * chunkSize * chunkLoop);
            int chunckY = (index % (chunkSize * chunkSize * chunkLoop)) / (chunkSize * chunkSize);

            int chunckID = chunckX * chunkLoop + chunckY;

            int firstChunkNodeID = chunckX * chunkSize * chunkSize * chunkLoop + chunckY * chunkSize * chunkSize;

            int relativeNodeFlat = index - firstChunkNodeID;

            //int nodePosX = relativeNodeFlat / chunkSize;
            //int nodePosY = relativeNodeFlat % chunkSize;

            //int chunkPosX = chunckX * chunkSize;
            //int chunkPosY = chunckY * chunkSize;

            int2 pos = GetNodeIndexFromFlatIndex(firstChunkNodeID + relativeNodeFlat);

            pos -= chunkLoop * chunkSize / 2;

            float3 pos2 = new float3(pos.x, 0, pos.y);
            UnityEngine.Debug.DrawLine(new float3(pos2.x, 0, pos2.z), new float3(pos2.x, _chunks[chunckID]._nchunk[relativeNodeFlat] * 3, pos2.z), Color.magenta, 100f);
            //collisionsHeightMap[firstChunkNodeID + relativeNodeFlat]

            return _chunks[chunckID]._nchunk[relativeNodeFlat];
        }

        public (int, int) GetChunckIndexFromNodeIndex(int indexX, int indexY)
        {
            return (((int)(indexX / (int)chunkSize), (int)(indexY / chunkSize)));
        }

    }
}