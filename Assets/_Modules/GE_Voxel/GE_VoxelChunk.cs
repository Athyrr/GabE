using System.Numerics;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace _Modules.GE_Voxel
{
    public class GE_VoxelChunk
    {
        private int _chunckSize;
        private Vector3[] _cubeP;
        private GameObject _gameObject;

        public GE_VoxelChunk(GameObject GO, int chunckSize)
        {
            _gameObject = GO;
            _chunckSize = chunckSize;
            _cubeP = new Vector3[_chunckSize];
        }

        public void Load()
        {
            
            MeshRenderer meshRenderer = _gameObject.GetComponent<MeshRenderer>();
            if (meshRenderer == null)
                meshRenderer = _gameObject.AddComponent<MeshRenderer>();
            
            new GE_VoxelCube(_gameObject)
                .Draw(new UnityEngine.Vector3(2, 0, 0));
            new GE_VoxelCube(_gameObject)
                .Draw(new UnityEngine.Vector3(-2, 0, 0));
            
            
            
            /*
            for (int i = 0; _chunckSize > i; i++)
            {
                UnityEngine.Vector3 tPos = new UnityEngine.Vector3(i*10, 0, 0);
                
                new GE_VoxelCube(_gameObject)
                    .Draw(tPos);
            }*/
        }
    }
}