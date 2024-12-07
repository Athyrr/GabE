using GabE.Module.ECS;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;


[UpdateInGroup(typeof(PresentationSystemGroup))]
public partial class ECS_PersonRendererSystem : SystemBase
{
    private Mesh[] _lodMeshes;
    private Material[] _workersMaterials;

    //private Camera _mainCamera;

    protected override void OnCreate()
    {
        // Load Meshes LOD
        _lodMeshes = new Mesh[1]; // set length to 3 (LOD lvls)
        _lodMeshes[0] = Resources.Load<GameObject>("Workers/Meshes/MSH_Person_LOD0")
            .GetComponent<MeshFilter>()?.sharedMesh;

        //lodMeshes[1] = Resources.Load<GameObject>("Workers/Meshes/MSH_Person_LOD1")
        // .GetComponent<MeshFilter>()?.sharedMesh;

        //lodMeshes[2] = Resources.Load<GameObject>("Workers/Meshes/MSH_Person_LOD2")
        // .GetComponent<MeshFilter>()?.sharedMesh;


        // Load Materials   /!\ DO NOT CHANGE ARRAY ORDER !
        _workersMaterials = new Material[5]; // Byte[]
        _workersMaterials[0] = Resources.Load<Material>("Workers/Materials/MT_Worker_FoodHarvester");
        _workersMaterials[1] = Resources.Load<Material>("Workers/Materials/MT_Worker_Timberman");
        _workersMaterials[2] = Resources.Load<Material>("Workers/Materials/MT_Worker_Miner");
        _workersMaterials[3] = Resources.Load<Material>("Workers/Materials/MT_Worker_Mason");
        _workersMaterials[4] = Resources.Load<Material>("Workers/Materials/MT_Worker_Vagabond");

        //_mainCamera = Camera.main;

#if DEBUG

        for (int i = 0; i < _workersMaterials.Length; i++)
        {
            if (_workersMaterials[i] == null)
            {
                Debug.LogError($"Material at index {i} is null. Check the resource path!");
            }
        }

        Debug.Log($"Loaded Mesh: {_lodMeshes[0]?.name}");
        for (int i = 0; i < _workersMaterials.Length; i++)
        {
            if (_workersMaterials[i] != null)
                Debug.Log($"Loaded Material {i}: {_workersMaterials[i].name}");
            else
                Debug.LogError($"Material {i} failed to load.");
        }

        //if (_mainCamera == null)
        //    Debug.LogError("Main Camera not found!");

#endif
    }

    protected override void OnUpdate()
    {
        if (_lodMeshes == null || _workersMaterials == null /*|| _mainCamera == null*/)
            return;

        // Gather LODs and Materials in Matrices 
        Dictionary<(int lodLevel, byte materialID), List<Matrix4x4>> renderGroups = new Dictionary<(int lodLevel, byte materialID), List<Matrix4x4>>();

        foreach (var (lod, position, material) in SystemAPI.Query<RefRO<ECS_MeshLODGroupFragment>, RefRO<ECS_PositionFragment>, RefRO<ECS_WorkerMaterialFragment>>())
        {
            int lodLevel = lod.ValueRO.CurrentLOD;
            byte materialID = material.ValueRO.MaterialID;


            if (lodLevel >= _lodMeshes.Length || materialID >= _workersMaterials.Length)
                continue;

            // Set rendersGroups group by (lod,material) 
            if (!renderGroups.ContainsKey((lodLevel, materialID)))
                renderGroups[(lodLevel, materialID)] = new List<Matrix4x4>();

            renderGroups[(lodLevel, materialID)].Add(Matrix4x4.Translate(position.ValueRO.Position));
        }

        // Render RenderGroups
        foreach (var group in renderGroups)
        {
            int lodLevel = group.Key.lodLevel;
            byte materialID = group.Key.materialID;
            var matrices = group.Value;

            if (matrices.Count > 0)
            {
                Mesh renderedMesh = _lodMeshes[lodLevel];
                Material renderedMaterial = _workersMaterials[materialID];



                if (renderedMesh == null || renderedMaterial == null)
                {
                    Debug.LogWarning($"Mesh or Material is null. LOD: {lodLevel}, Material ID: {materialID}");
                    continue;
                }



                Graphics.DrawMeshInstanced(renderedMesh, 0, renderedMaterial, matrices.ToArray());
            }
        }
    }
}
