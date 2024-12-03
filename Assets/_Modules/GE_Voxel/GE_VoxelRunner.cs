using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
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
        private GE_VoxelChunk[] _chunks;
        public Camera _playerCamera;
        public bool debug = false;
        
        private IEnumerator Start()
        {
            if (!_playerCamera)
                throw new Exception("No player camera available");
            
            float offset = 1 + chunkOffset;
            _chunks = new GE_VoxelChunk[chunkLoop*chunkLoop];
            
            enabled = false;
            yield return new WaitForSeconds( 1f ); // to avoid a stranglehold
            
            for (byte i = 0; i < chunkLoop; i++)
            {
                for (byte j = 0; j < chunkLoop; j++)
                {
                    Vector3 p = new Vector3(chunkSize*i*0.5f+offset - chunkLoop * chunkSize *0.25f,0,chunkSize*j*0.5f+offset - chunkLoop * chunkSize *0.25f);
                    //Debug.Log(p);
                    GE_VoxelChunk e = new GE_VoxelChunk(new GameObject(), p, chunkSize, 1f, yMax, material);
                    e.Load();
                    _chunks[i + j * chunkLoop] = e;
                    
                    yield return new WaitForSeconds( 0.005f );
                } 
            }
            
            /*
            for (byte i = 0; i < _chunks.Length; ++i)
            {
                int x = i%chunkLoop;
                int y = (int)math.floor(i / chunkLoop); // because chunkSize cannot be 0
                
                DrawBox(
                    new Vector3(x + x * chunkSize + chunkSize *0.5f, 0f, y + y * chunkSize + chunkSize *0.5f),
                    Quaternion.identity,
                    new Vector3(chunkSize, yMax, chunkSize),
                    Color.blue
                );
            }*/
            
            enabled = true;
        }

        private void Update()
        {


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

                // draw debug bounds of chuncks
                float cPosF = chunkPos.x + chunkPos.y + chunkSize*0.5f;
                float pPosF = _playerCamera.transform.position.x + _playerCamera.transform.position.z;
                float d = math.distance(pPosF, cPosF);
                
                /*
                if (d < 5)
                    
                else if (d > 5)
                    
                else if (d > 5)
                */
                
                
                
                
                
                #if UNITY_EDITOR
                    if (debug)
                        DrawBox(
                            chunkPos,
                            Quaternion.identity,
                            new Vector3(chunkSize, yMax, chunkSize),
                            Color.blue
                        );
                #endif
            }
            //enabled = false;
        }
        
        public void DrawBox(Vector3 pos, Quaternion rot, Vector3 scale, Color c)
        {
            // create matrix
            Matrix4x4 m = new Matrix4x4();
            m.SetTRS(pos, rot, scale);

            var point1 = m.MultiplyPoint(new Vector3(-0.5f, -0.5f, 0.5f));
            var point2 = m.MultiplyPoint(new Vector3(0.5f, -0.5f, 0.5f));
            var point3 = m.MultiplyPoint(new Vector3(0.5f, -0.5f, -0.5f));
            var point4 = m.MultiplyPoint(new Vector3(-0.5f, -0.5f, -0.5f));

            var point5 = m.MultiplyPoint(new Vector3(-0.5f, 0.5f, 0.5f));
            var point6 = m.MultiplyPoint(new Vector3(0.5f, 0.5f, 0.5f));
            var point7 = m.MultiplyPoint(new Vector3(0.5f, 0.5f, -0.5f));
            var point8 = m.MultiplyPoint(new Vector3(-0.5f, 0.5f, -0.5f));

            Debug.DrawLine(point1, point2, c);
            Debug.DrawLine(point2, point3, c);
            Debug.DrawLine(point3, point4, c);
            Debug.DrawLine(point4, point1, c);

            Debug.DrawLine(point5, point6, c);
            Debug.DrawLine(point6, point7, c);
            Debug.DrawLine(point7, point8, c);
            Debug.DrawLine(point8, point5, c);

            Debug.DrawLine(point1, point5, c);
            Debug.DrawLine(point2, point6, c);
            Debug.DrawLine(point3, point7, c);
            Debug.DrawLine(point4, point8, c);

            Vector3 forward;
            forward.x = m.m02;
            forward.y = m.m12;
            forward.z = m.m22;

            Vector3 upwards;
            upwards.x = m.m01;
            upwards.y = m.m11;
            upwards.z = m.m21;
            
            // optional axis display
            Debug.DrawRay(m.GetPosition(), forward, Color.magenta);
            Debug.DrawRay(m.GetPosition(), upwards, Color.yellow);
            //Debug.DrawRay(m.GetPosition(), m.GetRight(), Color.red);
        }
    }
}