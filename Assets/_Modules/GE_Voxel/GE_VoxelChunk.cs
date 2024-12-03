using System;
using _Modules.GE_Voxel.Utils;
using Unity.Mathematics;
using UnityEngine;
using Matrix4x4 = UnityEngine.Matrix4x4;
using Random = UnityEngine.Random;
using Vector2 = System.Numerics.Vector2;

namespace _Modules.GE_Voxel
{
    public class GE_VoxelChunk
    {
        private byte _chunkSize = 15;
        private float _cubeSize = 1f;
        private byte _yMax;
        private MeshFilter _meshFilter;
        private MeshRenderer _meshRenderer;
        private GameObject _gameObject;
        private Material _Material;
        
        private Vector3 _chunkPosition;
        private byte[] _nchunk;
        
        public GE_VoxelChunk(GameObject GO, Vector3 chunkPosition, byte chunkSize, float cubeSize, byte yMax, Material material)
        {
            _gameObject = GO;
            _cubeSize = cubeSize;
            _chunkSize = (byte)(chunkSize / _cubeSize);
            _yMax = yMax;
            _nchunk = new byte[_chunkSize * _chunkSize];
            _chunkPosition = chunkPosition;
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
                    float noiseValue = GE_Math.Voronoise((new Vector2(i* _cubeSize, j* _cubeSize) + new Vector2(_chunkPosition.x, _chunkPosition.z)*2)*.1f , pHeight.X, pHeight.Y); 
                    float maxY = Mathf.Clamp((noiseValue * _yMax)+1, 0, _yMax); // Scale and clamp the noise value
                    
                    _nchunk[i + _chunkSize*j] = (byte)(maxY);
                }
            }
        }

        public void LoadMesh()
        {
            Matrix4x4[] matrices;
            Mesh combined = new Mesh();
            CombineInstance[] instances = new CombineInstance[_chunkSize * _chunkSize * _yMax];
            int index = 0;
            matrices = new Matrix4x4[_chunkSize * _chunkSize * _yMax * 6];
            for (byte i = 0; i < _chunkSize; ++i)
            {
                for (byte j = 0; j < _chunkSize; ++j)
                {
                    byte nValue = _nchunk[i + _chunkSize * j];
                    for (byte k = 0; k < nValue; ++k)
                    {
                        Mesh cubeMesh = RenderCube(Vector3.zero, GetNeighborsWithChunks(i, j, k, nValue));
                        instances[index].mesh = cubeMesh;
                        instances[index].transform = Matrix4x4.Translate(new Vector3(i * _cubeSize, nValue-k,j * _cubeSize) + _chunkPosition);
                        
                        // Vector3 scale = Vector3.one;
                        // Quaternion rotation = Quaternion.Euler(Random.Range(-180, 180), Random.Range(-180, 180), Random.Range(-180, 180));
                        // var mat = Matrix4x4.TRS(new Vector3(i, nValue-k,j) + _chunkPosition, rotation, scale);
                        // matrices[i] = mat;
                        cubeMesh.RecalculateNormals();
                        cubeMesh.RecalculateBounds();
                        ++index;
                    }
                }
            }

            int range = 100;
            
            Array.Resize(ref instances, index); // Resize the array to exclude unused instances
            combined.CombineMeshes(instances, true, true);
            _meshFilter.sharedMesh = combined;
        }

        public void Load()
        {
            LoadNoise();
            LoadMesh();
            _gameObject.transform.position = _chunkPosition;
            _gameObject.name = "Chunk " + _chunkPosition.x + ", " + _chunkPosition.z;
        }
        
        private bool[] GetNeighborsWithChunks(int i, int j, int k, byte nValue)
        {
            return new bool[]
            {
                // Check Right Neighbor
                !(i + 1 < _chunkSize && _nchunk[(i + 1) + _chunkSize * j] < nValue),
                // Check Left Neighbor
                !(i - 1 >= 0 && _nchunk[(i - 1) + _chunkSize * j] < nValue),
                // Check Top Neighbor
                (k - 1 >= 0 && nValue > 0),
                // Check Bottom Neighbor
                true, // never see this face
                // Check Front Neighbor
                !(j + 1 < _chunkSize && _nchunk[i + _chunkSize * (j + 1)] < nValue),
                // Check Back Neighbor
                !(j - 1 >= 0 && _nchunk[i + _chunkSize * (j - 1)] < nValue)
            };
        }

        public Mesh RenderCube(Vector3 location, bool[] _neighbor)
        {
            Mesh mesh = new Mesh();

            Vector3[] vertices = new Vector3[]
            {
                // Front face
                new Vector3(0, 0, 0), new Vector3(_cubeSize, 0, 0), new Vector3(_cubeSize, _cubeSize, 0), new Vector3(0, _cubeSize, 0),
                // Back face
                new Vector3(0, 0, _cubeSize), new Vector3(_cubeSize, 0, _cubeSize), new Vector3(_cubeSize, _cubeSize, _cubeSize), new Vector3(0, _cubeSize, _cubeSize)
            };

            mesh.vertices = vertices;
            byte triangleIndex = 0;
            
            byte[][] faceTriangles = new byte[][]
            {
                new byte[] {1, 2, 6, 6, 5, 1},  // Right face
                new byte[] {4, 7, 3, 3, 0, 4},  // Left face
                new byte[] {3, 7, 6, 6, 2, 3},  // Top face
                new byte[] {4, 0, 1, 1, 5, 4},  // Bottom face
                new byte[] {5, 6, 7, 7, 4, 5},   // Front face
                new byte[] {0, 3, 2, 2, 1, 0},  // Back face
            };

            byte neighborsCount = 0;
            
            foreach (var e in _neighbor)
            {
                if (!e)
                    ++neighborsCount;
            }
            int[] triangles = new int[neighborsCount*6];

            byte _nIndex = 0;
            foreach (var e in _neighbor)
            {
                if (!e)
                    for (byte i = 0; i < faceTriangles[_nIndex].Length; ++i)
                    {
                        triangles[triangleIndex++] = faceTriangles[_nIndex][i];
                    }

                ++_nIndex;
            }
            
            mesh.triangles = triangles;

            //mesh.RecalculateNormals();
            //mesh.RecalculateBounds();

            return mesh;
        }
    }
}
