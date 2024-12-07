using System;
using System.Collections;
using System.Collections.Generic;
using _Modules.GE_Voxel.Utils;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;


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
        public GE_VoxelChunk[] _chunks;
        public Camera _playerCamera;
        public GameObject foliageMesh;
        public bool debug = false;

        private float _lastCameraPositionValue; // x + z
        
        private void Start()
        {
            if (!_playerCamera)
                throw new Exception("No player camera available");
            
            _chunks = new GE_VoxelChunk[chunkLoop*chunkLoop];
            
            enabled = false;
            
            for (byte i = 0; i < chunkLoop; i++)
            {
                for (byte j = 0; j < chunkLoop; j++)
                {
                    Vector3 p = new Vector3(chunkSize*i - chunkLoop * chunkSize / 2 + chunkSize / 2,0, chunkSize * j - chunkLoop * chunkSize / 2 + chunkSize / 2);

                    GE_VoxelChunk e = new GE_VoxelChunk(new GameObject(), p, chunkSize, 1, yMax, material, foliageMesh);
                    
                    e.Load();
                    _chunks[i + j * chunkLoop] = e;
                } 
            }
            foreach (var e in _chunks)
            {
                e.UpdateLOD(2);
            }
            foreach (var e in _chunks)
            {
                e.UpdateLOD(4);
            }
            foreach (var e in _chunks)
            {
                e.UpdateLOD(1);
            }
            
            
            enabled = true;
            //InvokeRepeating("GE_Update",0.5f,0.5f);
        }
        
        private void Update()
        {
            #if UNITY_EDITOR
                if (debug)
                    for (byte i = 0; i < _chunks.Length; ++i)
                    {
                        int x = i % chunkLoop;
                        int y = (int)math.floor(i / chunkLoop); // because chunkSize cannot be 0
                        Vector3 chunkPos = new Vector3(
                            x + x * chunkSize + chunkSize * 0.5f - chunkLoop * chunkSize * 0.5f,
                            0f,
                            y + y * chunkSize + chunkSize * 0.5f - chunkLoop * chunkSize * 0.5f);
                        GE_Debug.DrawBox(
                            chunkPos,
                            Quaternion.identity,
                            new Vector3(chunkSize, yMax, chunkSize),
                            Color.blue
                        );
                    }
            #endif
        }

        private void GE_Update()
        {
            
            Vector2 cPos = new Vector2(_playerCamera.transform.position.x, _playerCamera.transform.position.z);
            float tmpValue = cPos.x + cPos.y;

            if (_lastCameraPositionValue < tmpValue + 0.1f && _lastCameraPositionValue > tmpValue - 0.1f)
                return;
            else _lastCameraPositionValue = tmpValue;
            
            for (byte i = 0; i < _chunks.Length; ++i)
            {
                GE_VoxelChunk e = _chunks[i];
                int x = i%chunkLoop;
                int y = (int)math.floor(i / chunkLoop); // because chunkSize cannot be 0
                //_playerCamera.transform.position
                Vector3 chunkPos = new Vector3(
                    x + x * chunkSize + chunkSize * 0.5f - chunkLoop * chunkSize *0.25f,
                    0f,
                    y + y * chunkSize + chunkSize * 0.5f - chunkLoop * chunkSize *0.25f);
                
                float cPosF = chunkPos.x + chunkPos.y + chunkSize*0.5f;
                float d = math.distance(tmpValue, cPosF);

            }
        }
        
    }
}