using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

using GabE.Module.ECS;
using UnityEngine.UIElements;

[UpdateInGroup(typeof(PresentationSystemGroup))]
public partial class ECS_ResourceZonesRendererSystem : SystemBase
{
    private Material foodZoneMaterial;
    private Mesh foodZoneMesh;

    private Material woodZoneMaterial;
    private Mesh woodZoneMesh;

    private Material stoneZoneMaterial;
    private Mesh stoneZoneMesh;

    protected override void OnCreate()
    {
        foodZoneMaterial = Resources.Load<Material>("ResourceZones/Materials/MT_ResourcesZone_Food");
        var foodZonePrefab = Resources.Load<GameObject>("ResourceZones/Meshes/MSH_ResourceZone_Food");
        foodZoneMesh = foodZonePrefab.GetComponent<MeshFilter>()?.sharedMesh;

        if (foodZoneMaterial == null || foodZoneMesh == null)
            Debug.LogError("food material or mesh not found!");

        woodZoneMaterial = Resources.Load<Material>("ResourceZones/Materials/MT_ResourcesZone_Wood");
        var woodZonePrefab = Resources.Load<GameObject>("ResourceZones/Meshes/MSH_ResourceZone_Wood");
        woodZoneMesh = woodZonePrefab.GetComponent<MeshFilter>()?.sharedMesh;

        if (woodZoneMaterial == null || foodZoneMesh == null)
            Debug.LogError("wood material or mesh not found!");

        stoneZoneMaterial = Resources.Load<Material>("ResourceZones/Materials/MT_ResourcesZone_Stone");
        var stoneZonePrefab = Resources.Load<GameObject>("ResourceZones/Meshes/MSH_ResourceZone_Stone");
        stoneZoneMesh = stoneZonePrefab.GetComponent<MeshFilter>()?.sharedMesh;

        if (foodZoneMaterial == null || foodZoneMesh == null)
            Debug.LogError("stone material or mesh not found!");
    }

    protected override void OnUpdate()
    {
        if (foodZoneMaterial == null || foodZoneMesh == null ||
            woodZoneMaterial == null || woodZoneMesh == null ||
            stoneZoneMaterial == null || stoneZoneMesh == null)
            return;

        var foodMatrices = new List<Matrix4x4>();
        var woodMatrices = new List<Matrix4x4>();
        var stoneMatrices = new List<Matrix4x4>();

        foreach (var (position, resource) in SystemAPI.Query<RefRO<ECS_Frag_Position>, RefRO<ECS_ResourceZoneFragment>>())
        {
            var matrix = Matrix4x4.Translate(position.ValueRO.Position);
            switch (resource.ValueRO.Type)
            {
                case ResourceType.Food:
                    foodMatrices.Add(matrix);
                    break;
                case ResourceType.Wood:
                    woodMatrices.Add(matrix);
                    break;
                case ResourceType.Stone:
                    stoneMatrices.Add(matrix);
                    break;
            }
        }

        //Mesh combinedMeshes = new Mesh();

        //CombineInstance[] combineInstances = new CombineInstance[3];

        //combineInstances[0].mesh = combinedMeshes;
        //combineInstances[0].transform = Matrix4x4.Translate(Vector3.zero);

        //combinedMeshes.CombineMeshes(combineInstances);


        if (foodMatrices.Count > 0)
            Graphics.DrawMeshInstanced(foodZoneMesh, 0, foodZoneMaterial, foodMatrices.ToArray());

        if (woodMatrices.Count > 0)
            Graphics.DrawMeshInstanced(woodZoneMesh, 0, woodZoneMaterial, woodMatrices.ToArray());

        if (stoneMatrices.Count > 0)
            Graphics.DrawMeshInstanced(stoneZoneMesh, 0, stoneZoneMaterial, stoneMatrices.ToArray());
    }
}

