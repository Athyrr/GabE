using System;
using UnityEngine;

namespace _Modules.GE_Voxel
{
    [CreateAssetMenu(fileName = "GE Voxel Runner", menuName = "GE", order = 0)]
    public class GE_VoxelRunner : MonoBehaviour
    {

        
        private void Start()
        {
            //
            // MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
            // if (meshRenderer == null)
            //     meshRenderer = gameObject.AddComponent<MeshRenderer>();
            
            new GE_VoxelChunk(gameObject, 5).Load();
            
        }

    }
}