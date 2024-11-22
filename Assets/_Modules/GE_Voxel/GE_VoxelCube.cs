using System.Linq;
using UnityEngine;

namespace _Modules.GE_Voxel
{
    public class GE_VoxelCube
    {
        private int[] _t = {};
        private Vector3[] _v = new Vector3[]
        {
            // Front face
            new Vector3(0, 0, 0), // Bottom-left (0)
            new Vector3(1, 0, 0), // Bottom-right (1)
            new Vector3(1, 1, 0), // Top-right (2)
            new Vector3(0, 1, 0), // Top-left (3)

            // Back face
            new Vector3(0, 0, 1), // Bottom-left (4)
            new Vector3(1, 0, 1), // Bottom-right (5)
            new Vector3(1, 1, 1), // Top-right (6)
            new Vector3(0, 1, 1), // Top-left (7)
        };

        private MeshFilter _meshFilter;
        private GameObject _gameObject;
        
        public GE_VoxelCube(GameObject GO)
        {
            _gameObject = GO;
        }

        public void Start()
        {
            
            _meshFilter = _gameObject.GetComponent<MeshFilter>();
            if (_meshFilter == null)
                _meshFilter = _gameObject.AddComponent<MeshFilter>();

        }
        
        public void Draw(Vector3 Location)
        {
            RenderCube(Location);

            // RenderCube(Location)
            //     .transform.position = Location;
        }
        public void Draw()
        {
            //RenderCube();
        }

        private MeshFilter RenderCube(Vector3 Location)
        {
            
            MeshFilter meshFilter = _gameObject.GetComponent<MeshFilter>();
            if (meshFilter == null)
                meshFilter = _gameObject.AddComponent<MeshFilter>();


            Mesh mesh = new Mesh();


            Vector3[] vertices = new Vector3[]
            {
                // Front face
                new Vector3(0, 0, 0) +Location, // Bottom-left (0)
                new Vector3(1, 0, 0) +Location, // Bottom-right (1)
                new Vector3(1, 1, 0) +Location, // Top-right (2)
                new Vector3(0, 1, 0) +Location, // Top-left (3)

                // Back face
                new Vector3(0, 0, 1) +Location, // Bottom-left (4)
                new Vector3(1, 0, 1) +Location, // Bottom-right (5)
                new Vector3(1, 1, 1) +Location, // Top-right (6)
                new Vector3(0, 1, 1) +Location, // Top-left (7)
            };


            int[] triangles = new int[]
            {
                // Front face
                0, 1, 2, // First triangle
                2, 3, 0, // Second triangle

                // Back face
                5, 4, 7, // First triangle
                7, 6, 5, // Second triangle

                // Left face
                4, 0, 3, // First triangle
                3, 7, 4, // Second triangle

                // Right face
                1, 5, 6, // First triangle
                6, 2, 1, // Second triangle

                // Top face
                3, 2, 6, // First triangle
                6, 7, 3, // Second triangle

                // Bottom face
                4, 5, 1, // First triangle
                1, 0, 4, // Second triangle
            };

            mesh.vertices = vertices;
            mesh.triangles = triangles;

            // Optionally calculate normals and UVs
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            
            // Assign the mesh to the MeshFilter
            meshFilter.mesh = mesh;
            
            return meshFilter;
        }
    }
}