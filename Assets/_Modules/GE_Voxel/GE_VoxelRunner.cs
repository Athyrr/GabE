using System;
using UnityEngine;

namespace _Modules.GE_Voxel
{
    [CreateAssetMenu(fileName = "GE Voxel Runner", menuName = "GE", order = 0)]
    public class GE_VoxelRunner : MonoBehaviour
    {
        public int chunkSize;
        public int yMax;
        
        private void Start()
        {
            //
            // MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
            // if (meshRenderer == null)
            //     meshRenderer = gameObject.AddComponent<MeshRenderer>();
            
            new GE_VoxelChunk(gameObject, chunkSize, yMax).Load();
            
        }

    }
}