using System;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace _Modules.GE_Voxel
{
    public class GE_VoxelCube
    {
        private int[] _t = {};
        private bool[] _neighbor = new bool[6];

        private GameObject _gameObject;
        
        public GE_VoxelCube(GameObject GO, bool[] neighbors) // Vector3[6] neighbors
        {
            _gameObject = GO;
            _neighbor = neighbors;
        }

        public void Draw(Vector3 Location)
        {
            RenderCube(Location);
        }
        

        public Mesh RenderCube(Vector3 location)
        {
            Mesh mesh = new Mesh();

            Vector3[] vertices = new Vector3[]
            {
                // Front face
                new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(1, 1, 0), new Vector3(0, 1, 0),
                // Back face
                new Vector3(0, 0, 1), new Vector3(1, 0, 1), new Vector3(1, 1, 1), new Vector3(0, 1, 1)
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

            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            return mesh;
        }

    }
}