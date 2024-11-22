using System;
using UnityEngine;
using Matrix4x4 = UnityEngine.Matrix4x4;
using Random = System.Random;

namespace _Modules.GE_Voxel
{
    public class GE_VoxelChunk
    {
        private int _chunkSize = 15;
        private int _yMax = 2;
        private MeshFilter _meshFilter;
        private GameObject _gameObject;

        private Vector3[,,] _chunk;
        
        public GE_VoxelChunk(GameObject GO, int chunkSize, int yMax)
        {
            _gameObject = GO;
            _chunkSize = chunkSize;
            _yMax = yMax;
            _chunk = new Vector3[chunkSize, _yMax, chunkSize];
            
            _meshFilter = _gameObject.GetComponent<MeshFilter>();
            if (_meshFilter == null)
                _meshFilter = _gameObject.AddComponent<MeshFilter>();
        }

        public void LoadNoise()
        {
            Random rnd = new Random();
            for (int i = 0; i < _chunkSize; ++i)
            {
                for (int j = 0; j < _chunkSize; ++j)
                {
                    int y = rnd.Next(1, _yMax);        
                    for (int k = 0; k < _yMax; k++)
                    {
                        if (k <= y)
                        {
                            Vector3 p = new Vector3(
                                i * 1.1f,
                                k,
                                j * 1.1f
                            );
                            _chunk[i, k, j] = p;
                        }
                        else
                        {
                            // Mark empty spaces, mb delete this
                            _chunk[i, k, j] = Vector3.zero;
                        }
                    }
                }
            }
        }

        public void LoadMesh()
        {
            MeshRenderer meshRenderer = _gameObject.GetComponent<MeshRenderer>();
            if (meshRenderer == null)
                meshRenderer = _gameObject.AddComponent<MeshRenderer>();
            
            Mesh combined = new Mesh();
            CombineInstance[] instances = new CombineInstance[_chunkSize * _chunkSize * _yMax];
            int index = 0;

            for (int i = 0; i < _chunkSize; ++i)
            {
                for (int j = 0; j < _chunkSize; ++j)
                {
                    for (int k = 0; k < _yMax; ++k)
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
                        instances[index].transform = Matrix4x4.Translate(_chunk[i, k, j]);
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
        }
    }
}
