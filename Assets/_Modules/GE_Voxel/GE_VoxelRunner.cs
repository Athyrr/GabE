using System;
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
        
        private void Start()
        {
            float offset = 1 + chunkOffset;
            
            for (byte i = 0; i < chunkLoop; i++)
            {
                for (byte j = 0; j < chunkLoop; j++)
                {
                    Vector3 p = new Vector3(chunkSize*i*0.5f+offset,0,chunkSize*j*0.5f+offset);
                    //Debug.Log(p);
                    new GE_VoxelChunk(new GameObject(), p, chunkSize, (byte)(i + j) , chunkLoop, yMax, material, cubeOffset).Load();
                } 
            }
            
        }

    }
}