using System;
using _Modules.GE_Voxel.Utils;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
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
        private byte _initChunkSize = 15;
        private float _cubeSize = 1f;
        private byte _yMax;
        private MeshFilter _meshFilter;
        private MeshRenderer _meshRenderer;
        private GameObject _gameObject;
        private Material _Material;
        
        private Vector3 _chunkPosition;
        private NativeArray<byte> _nchunk;
        
        public GE_VoxelChunk(GameObject GO, Vector3 chunkPosition, byte chunkSize, float cubeSize, byte yMax, Material material)
        {
            _gameObject = GO;
            _cubeSize = cubeSize;
            _chunkSize = (byte)(chunkSize / _cubeSize);
            _initChunkSize = (byte)(chunkSize / _cubeSize);
            _yMax = yMax;
            _nchunk = new NativeArray<byte>(_chunkSize * _chunkSize, Allocator.TempJob);
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
        
        [BurstCompile(FloatMode = FloatMode.Fast, FloatPrecision = FloatPrecision.Low, OptimizeFor = OptimizeFor.Performance, CompileSynchronously = true)]
        private struct LoadNoise : IJob
        {
            [ReadOnly] public byte chunkSize; 
            [ReadOnly] public float2 chunkPosition; 
            [ReadOnly] public float cubeSize; 
            [ReadOnly] public byte yMax; 
            [WriteOnly] public NativeArray<byte> nchunk; 
        
            public void Execute()
            {
                for (byte i = 0; i < chunkSize; ++i)
                {
                    for (byte j = 0; j < chunkSize; ++j)
                    {
                        // Generate noise value for height 0.5 - 0.5 *cos( vec2(0.0,2.0) );
                        float2 a = new float2((float)math.cos(0), (float)math.cos(2f));
                        float2 pHeight = new float2(0.5f, 0.5f) - 0.5f * a;
                        
                        float2 p = (new float2(i* cubeSize, j* cubeSize) + new float2(chunkPosition.x, chunkPosition.y)*2)*.1f;
                        
                        float noiseValue;
                        VoronoiseBurst(out noiseValue, p, pHeight.x, pHeight.y);
                        
                        float maxY = Mathf.Clamp((noiseValue * yMax)+1, 0, yMax); // Scale and clamp the noise value
                
                        nchunk[i + chunkSize*j] = (byte)(maxY);
                    }
                }
            }

            public void VoronoiseBurst(out float result, in float2 p, float u, float v)
            {
                float k = 1.0f + 63.0f * (float)math.pow(1.0-v,6.0);
                float2 i = new float2((float)math.floor(p.x), (float)math.floor(p.y));
                float2 f = new float2(math.frac(p.x), math.frac(p.y)); // place add frac with vec2 :(
    
                float2 a = new float2(0f,0f);
                for( int y=-2; y<=2; y++ )
                for( int x=-2; x<=2; x++ )
                {
                    float2  g = new float2( x, y );
                    float3  o = hash3( i + g )*new float3(u,u,1.0f);
                    float2  d = g - f + new float2(o.x,o.y);
                    float w = math.pow( 1.0f - math.smoothstep(0.0f,1.414f, d.x*d.x + d.y*d.y), k); // (float)d.Length()),  => d.x*d.x + d.y*d.y
                    a += new float2(o.z*w,w);
                }
        
                result = a.x/a.y;
            }
            
            public static float3 hash3( float2 p ) // check this for more details -> https://www.shadertoy.com/view/4fGcWd
            {
                float3 q = new float3( math.dot(p,new float2(127.1f,311.7f)), 
                    math.dot(p,new float2(269.5f, 183.3f)), 
                    math.dot(p,new float2(419.2f,371.9f)));

                return new float3(
                    math.frac(math.sin(q.x) * 43758.5453f),
                    math.frac(math.sin(q.y) * 43758.5453f),
                    math.frac(math.sin(q.z) * 43758.5453f)
                );
            }
        }
        
        // TODO : add LoadMesh in IJob && BurstCompile
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
            //LoadNoise();

            var nativeNChunk = new NativeArray<byte>(_nchunk.Length, Allocator.TempJob);
            byte b;
            var job = new LoadNoise()
            {
                chunkSize = _chunkSize,
                chunkPosition = new float2(_chunkPosition.x, _chunkPosition.z),
                cubeSize = _cubeSize,
                yMax = _yMax,
                nchunk = _nchunk,
            };
            job.Schedule().Complete();
            
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
        public void UpdateLOD(float cubeSize)
        {
            // step 1: clear all cube in this chunk
            /*for (byte i = 0; i < _chunkSize; ++i)
            {
                for (byte j = 0; j < _chunkSize; ++j)
                {
                    _nchunk[i + _chunkSize*j] = 0;
                }
            }*/
            
            // step 2: replace allocated memory
            _cubeSize = cubeSize;
            _chunkSize = (byte)(_initChunkSize / _cubeSize);
            
            _nchunk = new NativeArray<byte>(_chunkSize * _chunkSize, Allocator.TempJob);
            
            // step 3: recalculate noise
            //LoadNoise();
            
            // step 4: redraw all cube
            //LoadMesh();
        }

    }
}