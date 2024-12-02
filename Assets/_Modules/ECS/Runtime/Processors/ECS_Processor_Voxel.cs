// using System;
// using Unity.Burst;
// using Unity.Collections;
// using Unity.Entities;
// using Unity.Mathematics;
// using UnityEngine;
// using Vector2 = System.Numerics.Vector2;
// using Vector3 = System.Numerics.Vector3;
//
// namespace GabE.Module.ECS
// {
//     public partial struct ECS_Processor_Voxel : ISystem
//     {
//         public static Vector3 hash3( Vector2 p ) // check this for more details -> https://www.shadertoy.com/view/4fGcWd
//         {
//             Vector3 q = new Vector3( Vector2.Dot(p,new Vector2(127.1f,311.7f)), 
//                 Vector2.Dot(p,new Vector2(269.5f, 183.3f)), 
//                 Vector2.Dot(p,new Vector2(419.2f,371.9f)));
//
//             return new Vector3(
//                 math.frac(Mathf.Sin(q.X) * 43758.5453f),
//                 math.frac(Mathf.Sin(q.Y) * 43758.5453f),
//                 math.frac(Mathf.Sin(q.Z) * 43758.5453f)
//             );
//         }
//
//         public static float Voronoise( in Vector2 p, float u, float v )
//         {
//             float k = 1.0f + 63.0f * (float)Math.Pow(1.0-v,6.0);
//
//             Vector2 i = new Vector2((float)Math.Floor(p.X), (float)Math.Floor(p.Y));
//             Vector2 f = new Vector2(math.frac(p.X), math.frac(p.Y)); // place add frac with vec2 :(
//         
//             Vector2 a = Vector2.Zero;
//             for( int y=-2; y<=2; y++ )
//             for( int x=-2; x<=2; x++ )
//             {
//                 Vector2  g = new Vector2( x, y );
//                 Vector3  o = hash3( i + g )*new Vector3(u,u,1.0f);
//                 Vector2  d = g - f + new Vector2(o.X,o.Y);
//                 float w = math.pow( 1.0f - math.smoothstep(0.0f,1.414f, d.Length()), k); // (float)d.Length()), 
//                 a += new Vector2(o.Z*w,w);
//             }
//             
//             return a.X/a.Y;
//         }
//         
//         public Mesh RenderCube(GameObject GO, bool[] neighbors) // Vector3[6] neighbors
//         {
//             Mesh mesh = new Mesh();
//             
//             UnityEngine.Vector3[] vertices = new UnityEngine.Vector3[]
//             {
//                 // Front face
//                 new UnityEngine.Vector3(0, 0, 0), new UnityEngine.Vector3(1, 0, 0), new UnityEngine.Vector3(1, 1, 0), new UnityEngine.Vector3(0, 1, 0),
//                 // Back face
//                 new UnityEngine.Vector3(0, 0, 1), new UnityEngine.Vector3(1, 0, 1), new UnityEngine.Vector3(1, 1, 1), new UnityEngine.Vector3(0, 1, 1)
//             };
//
//             mesh.vertices = vertices;
//             byte triangleIndex = 0;
//             
//             byte[][] faceTriangles = new byte[][]
//             {
//                 new byte[] {1, 2, 6, 6, 5, 1},  // Right face
//                 new byte[] {4, 7, 3, 3, 0, 4},  // Left face
//                 new byte[] {3, 7, 6, 6, 2, 3},  // Top face
//                 new byte[] {4, 0, 1, 1, 5, 4},  // Bottom face
//                 new byte[] {5, 6, 7, 7, 4, 5},   // Front face
//                 new byte[] {0, 3, 2, 2, 1, 0},  // Back face
//             };
//
//             byte neighborsCount = 0;
//             
//             foreach (var e in neighbors)
//             {
//                 if (!e)
//                     ++neighborsCount;
//             }
//             int[] triangles = new int[neighborsCount*6];
//
//             byte _nIndex = 0;
//             foreach (var e in neighbors)
//             {
//                 if (!e)
//                     for (byte i = 0; i < faceTriangles[_nIndex].Length; ++i)
//                     {
//                         triangles[triangleIndex++] = faceTriangles[_nIndex][i];
//                     }
//
//                 ++_nIndex;
//             }
//             
//             mesh.triangles = triangles;
//
//             mesh.RecalculateNormals();
//             mesh.RecalculateBounds();
//
//             return mesh;
//         }
//         
//         
//         struct test {
//             public byte ChunkSize;
//             public byte YMax;
//             public byte CubeOffset;
//             public byte ChunkOffset;
//             public byte ChunkLoop;
//         };
//         
//         [BurstCompile]
//         public void OnCreate(ref SystemState state)
//         {
//             /*
//              * INIT Values
//              */
//             test d = new test
//                 {
//                     ChunkSize = 16,
//                     YMax = 10,
//                     CubeOffset = 0,
//                     ChunkOffset = 0,
//                     ChunkLoop = 10,
//                 };
//             var entity = state.EntityManager.CreateEntity();
//             var buffer = state.EntityManager.AddBuffer<ECS_Frag_ChunkData>(entity);
// /*
//             for (ushort i = 0; i < (ushort)(d.ChunkLoop * d.ChunkLoop); ++i)
//             {
//                 ECS_Frag_ChunkData chunkData = new ECS_Frag_ChunkData
//                 {
//                     cubes = new FixedList4096Bytes<byte> { }
//                 };
//                 for (ushort j = 0; j < (ushort)(d.ChunkSize * d.ChunkSize); ++j)
//                 {   
//                     chunkData.cubes.Add(0);
//                 }
//             }*/
//
//             /*
//              * Load Chunk
//              */
//             for (byte i = 0; i < d.ChunkSize; ++i)
//             {
//                 for (byte j = 0; j < d.ChunkSize; ++j)
//                 {
//                     ECS_Frag_ChunkData chunkData = new ECS_Frag_ChunkData
//                     {
//                         cubes = new FixedList4096Bytes<byte> { }
//                     };
//                     
//                     
//                     GameObject _gameObject = new GameObject($"Chunk {i}-{j}");
//                     MeshFilter _meshFilter = _gameObject.GetComponent<MeshFilter>();
//                     if (_meshFilter == null)
//                         _meshFilter = _gameObject.AddComponent<MeshFilter>();
//                     
//                     Vector2 _chunkPosition = new Vector2(i, j);
//                     //Vector3[,,] _chunk;
//                     //_chunk = new Vector3[(int)d.ChunkSize, (int)d.YMax, (int)d.ChunkSize];
//                     // Generate noise value for height 0.5 - 0.5 *cos( vec2(0.0,2.0) );
//                     Vector2 a = new Vector2((float)math.cos(0), (float)math.cos(2f));
//                     Vector2 pHeight = new Vector2(0.5f, 0.5f) - 0.5f * a;
//                     float noiseValue = Voronoise((new Vector2(i, j) + new Vector2(_chunkPosition.X, _chunkPosition.Y)*2)*.1f , pHeight.X, pHeight.Y); 
//                     float maxY = Mathf.Clamp(noiseValue * d.YMax, 0, d.YMax); // Scale and clamp the noise value
//                     
//                     Debug.Log("chunkLoaded");
//             
//                     for (byte k = 0; k < d.YMax; k++)
//                     {
//                         if (k <= maxY)
//                         {
//                             float offset = 1 + d.CubeOffset; // Cube size + gap
//                             Vector3 p = new Vector3(
//                                 i * offset,
//                                 k * offset,
//                                 j * offset
//                             );
//                             chunkData.cubes.Add(k);
//                             //_chunk[i, k, j] = p; // Assign voxel position
//                         }
//                         else
//                         {
//                             // Empty space
//                             chunkData.cubes.Add(0); // 0 reserved for empty space
//                             //_chunk[i, k, j] = Vector3.Zero;
//                         }
//                     }
//                     
//                     Mesh combined = new Mesh();
//                     CombineInstance[] instances = new CombineInstance[d.ChunkSize * d.ChunkSize * d.YMax];
//                     int index = 0;
//
//                     for (byte x = 0; x < d.ChunkSize; ++x)
//                     {
//                         for (byte y = 0; y < d.ChunkSize; ++y)
//                         {
//                             for (byte z = 0; z < d.YMax; ++z)
//                             {
//                                 // Neighbor existence checks
//                                 bool[] neighbors = new bool[6]
//                                 {
//                                     (x + 1 < d.ChunkSize && chunkData.cubes[x + 1 + d.ChunkSize * y] != 0),   // Right
//                                     (x - 1 >= 0 && chunkData.cubes[x - 1 + d.ChunkSize * y] != 0),           // Left
//                                     (z + 1 < d.YMax && chunkData.cubes[x + d.ChunkSize * y] != 0),       // Top
//                                     (z - 1 >= 0 && chunkData.cubes[x + d.ChunkSize * y] != 0),           // Bottom
//                                     (y + 1 < d.ChunkSize && chunkData.cubes[x + d.ChunkSize * (y + 1)] != 0),  // Front
//                                     (y - 1 >= 0 && chunkData.cubes[x + d.ChunkSize * (y - 1)] != 0)           // Back
//                                 };
//
//                                 // Render Cube and Update Instances
//                                 Mesh cubeMesh = RenderCube(_gameObject, neighbors); // replace code here directly
//                                 instances[index].mesh = cubeMesh;
//
//                                 instances[index].transform = Matrix4x4.Translate(
//                                     new UnityEngine.Vector3(x, chunkData.cubes[x + d.ChunkSize * y], y) 
//                                     + new UnityEngine.Vector3(_chunkPosition.X, 0, _chunkPosition.Y)
//                                 );
//
//                                 index++;
//                             }
//                         }
//                     }
//
//                     Array.Resize(ref instances, index); // Resize the array to exclude unused instances
//                     combined.CombineMeshes(instances, true, true);
//                     _meshFilter.sharedMesh = combined;
//                     buffer.Add(chunkData);
//                 }
//
//             }
//
//             /*for (byte i = 0; d.ChunkLoop > i; ++i)
//             {
//                 for (byte x = 0; d.ChunkSize > x; ++x)
//                 {
//                     for (byte y = 0; d.ChunkSize > y; ++y)
//                     {
//
//
//                     }
//                 }
//             }*/
//         }
//
//         [BurstCompile]
//         public void OnUpdate(ref SystemState state)
//         {
//
//         }
//
//         [BurstCompile]
//         public void OnDestroy(ref SystemState state)
//         {
//
//         }
//     }
// }