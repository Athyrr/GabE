using System;
using _Modules.GE_Voxel.Utils;
using Unity.Mathematics;
using UnityEngine;
using Matrix4x4 = UnityEngine.Matrix4x4;
using Random = System.Random;
using Vector2 = System.Numerics.Vector2;

namespace _Modules.GE_Voxel
{
    public class GE_VoxelChunk
    {
        private byte _chunkSize = 15;
        private byte _yMax = 2;
        private byte _cubeOffset = 0;
        private MeshFilter _meshFilter;
        private MeshRenderer _meshRenderer;
        private GameObject _gameObject;
        private Material _Material;
        private byte _chunkID;
        private byte _loop;
        
        private Vector3 _chunkPosition;
        private Vector3[,,] _chunk;
        
        public GE_VoxelChunk(GameObject GO, Vector3 chunkPosition, byte chunkSize, byte chunkID, byte loop, byte yMax, Material material, byte cubeOffset = 0)
        {
            _gameObject = GO;
            _chunkSize = chunkSize;
            _yMax = yMax;
            _chunk = new Vector3[chunkSize, _yMax, chunkSize];
            _cubeOffset = cubeOffset;
            _chunkPosition = chunkPosition;
            _chunkID = chunkID;
            _loop = loop;
            _Material = material;
            
            
            MeshRenderer _meshRenderer = _gameObject.GetComponent<MeshRenderer>();
            if (_meshRenderer == null)
                _meshRenderer = _gameObject.AddComponent<MeshRenderer>();
            
            if (_Material)
                _meshRenderer.material = _Material;

            
            _meshFilter = _gameObject.GetComponent<MeshFilter>();
            if (_meshFilter == null)
                _meshFilter = _gameObject.AddComponent<MeshFilter>();
            
            
        }

        public void LoadNoise()
        {
            for (byte i = 0; i < _chunkSize; ++i)
            {
                for (byte j = 0; j < _chunkSize; ++j)
                {
                    // Generate noise value for height 0.5 - 0.5 *cos( vec2(0.0,2.0) );
                    Vector2 a = new Vector2((float)math.cos(0), (float)math.cos(2f));
                    Vector2 pHeight = new Vector2(0.5f, 0.5f) - 0.5f * a;
                    float noiseValue = GE_Math.Voronoise((new Vector2(i, j) + new Vector2(_chunkPosition.x, _chunkPosition.z)*2)*.1f , pHeight.X, pHeight.Y); 
                    float maxY = Mathf.Clamp(noiseValue * _yMax, 0, _yMax); // Scale and clamp the noise value
                    
                    //Debug.Log(_chunkID);
            
                    for (byte k = 0; k < _yMax; k++)
                    {
                        if (k <= maxY)
                        {
                            float offset = 1 + _cubeOffset; // Cube size + gap
                            Vector3 p = new Vector3(
                                i * offset,
                                k * offset,
                                j * offset
                            );
                            _chunk[i, k, j] = p; // Assign voxel position
                        }
                        else
                        {
                            // Empty space
                            _chunk[i, k, j] = Vector3.zero;
                        }
                    }
                }
            }
        }


        public void LoadMesh()
        {
            Mesh combined = new Mesh();
            CombineInstance[] instances = new CombineInstance[_chunkSize * _chunkSize * _yMax];
            int index = 0;

            for (byte i = 0; i < _chunkSize; ++i)
            {
                for (byte j = 0; j < _chunkSize; ++j)
                {
                    for (byte k = 0; k < _yMax; ++k)
                    {
                        // Neighbor existence checks
                        bool[] neighbors = new bool[6]
                        {
                            (i + 1 < _chunkSize && _chunk[i + 1, k, j] != Vector3.zero),   // Right
                            (i - 1 >= 0 && _chunk[i - 1, k, j] != Vector3.zero),           // Left
                            (k + 1 < _yMax && _chunk[i, k + 1, j] != Vector3.zero),       // Top
                            (k - 1 >= 0 && _chunk[i, k - 1, j] != Vector3.zero),           // Bottom
                            (j + 1 < _chunkSize && _chunk[i, k, j + 1] != Vector3.zero),  // Front
                            (j - 1 >= 0 && _chunk[i, k, j - 1] != Vector3.zero)           // Back
                        };

                        // Render Cube and Update Instances
                        GE_VoxelCube cube = new GE_VoxelCube(_gameObject, neighbors);
                        Mesh cubeMesh = cube.RenderCube(Vector3.zero);
                        instances[index].mesh = cubeMesh;
                        instances[index].transform = Matrix4x4.Translate(_chunk[i, k, j] + _chunkPosition);
                        index++;
                    }
                }
            }

            Array.Resize(ref instances, index); // Resize the array to exclude unused instances
            combined.CombineMeshes(instances, true, true);
            _meshFilter.sharedMesh = combined;
        }

        public void Load()
        {
            LoadNoise();
            LoadMesh();
            //_gameObject.transform.SetParent(world.transform);
            _gameObject.transform.position = _chunkPosition;
            _gameObject.name = "Chunk " + _chunkPosition.x + ", " + _chunkPosition.z;
        }
    }
}
