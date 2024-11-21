using System.Numerics;
using UnityEngine;
using Vector3 = System.Numerics.Vector3;

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

            for (int i = 0; _chunckSize > i; i++)
            {
                UnityEngine.Vector3 tPos = new UnityEngine.Vector3(i, 0, 0);
                GE_VoxelCube GE_VC = new GE_VoxelCube(_gameObject);
                
                GE_VC.Draw();
                _cubeP[i] = new Vector3(tPos.x, tPos.y, tPos.z);
            }
        }
        
    }
}