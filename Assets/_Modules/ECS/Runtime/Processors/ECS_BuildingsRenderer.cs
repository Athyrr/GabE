using GabE.Module.ECS;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

partial class ECS_BuildingsRenderer : SystemBase
{
    private Material farmMaterial;
    private Mesh farmMesh;

    private Material schoolMaterial;
    private Mesh schoolMesh;

    private Material museumMaterial;
    private Mesh museumMesh;

    private Material houseMaterial;
    private Mesh houseMesh;

    private Material bookshopMaterial;
    private Mesh bookshopMesh;

    protected override void OnCreate()
    {
        farmMaterial = Resources.Load<Material>("Buildings/Materials/MT_Building_Farm");
        var farmPrefab = Resources.Load<GameObject>("Buildings/Meshes/MSH_Building_Farm");
        farmMesh = farmPrefab.GetComponent<MeshFilter>()?.sharedMesh;

        if (farmMaterial == null || farmMesh == null)
            Debug.LogError("Farm material or mesh not found!");

        schoolMaterial = Resources.Load<Material>("Buildings/Materials/MT_Building_School");
        var schoolPrefab = Resources.Load<GameObject>("Buildings/Meshes/MSH_Building_School");
        schoolMesh = schoolPrefab.GetComponent<MeshFilter>()?.sharedMesh;

        if (schoolMaterial == null || schoolMesh == null)
            Debug.LogError("School material or mesh not found!");

        museumMaterial = Resources.Load<Material>("Buildings/Materials/MT_Building_Museum");
        var museumPrefab = Resources.Load<GameObject>("Buildings/Meshes/MSH_Building_Museum");
        museumMesh = museumPrefab.GetComponent<MeshFilter>()?.sharedMesh;

        if (museumMaterial == null || museumMesh == null)
            Debug.LogError("Museum material or mesh not found!");

        houseMaterial = Resources.Load<Material>("Buildings/Materials/MT_Building_House");
        var housePrefab = Resources.Load<GameObject>("Buildings/Meshes/MSH_Building_House");
        houseMesh = housePrefab.GetComponent<MeshFilter>()?.sharedMesh;

        if (houseMaterial == null || houseMesh == null)
            Debug.LogError("House material or mesh not found!");

        bookshopMaterial = Resources.Load<Material>("Buildings/Materials/MT_Building_Bookshop");
        var bookshopPrefab = Resources.Load<GameObject>("Buildings/Meshes/MSH_Building_Bookshop");
        bookshopMesh = bookshopPrefab.GetComponent<MeshFilter>()?.sharedMesh;

        if (bookshopMaterial == null || bookshopMesh == null)
            Debug.LogError("Bookshop material or mesh not found!");
    }

    protected override void OnUpdate()
    {
        if (farmMaterial == null || farmMesh == null ||
            schoolMaterial == null || schoolMesh == null ||
            museumMaterial == null || museumMesh == null ||
            houseMaterial == null || houseMesh == null ||
            bookshopMaterial == null || bookshopMesh == null)
            return;

        var farmMatrices = new List<Matrix4x4>();
        var schoolMatrices = new List<Matrix4x4>();
        var museumMatrices = new List<Matrix4x4>();
        var houseMatrices = new List<Matrix4x4>();
        var bookshopMatrices = new List<Matrix4x4>();

        foreach (var (position, building) in SystemAPI.Query<RefRO<ECS_PositionFragment>, RefRO<ECS_BuildingFragment>>())
        {
            var matrix = Matrix4x4.Translate(position.ValueRO.Position);
            switch (building.ValueRO.Type)
            {
                case BuildingType.Farm:
                    farmMatrices.Add(matrix);
                    break;
                case BuildingType.School:
                    schoolMatrices.Add(matrix);
                    break;
                case BuildingType.Museum:
                    museumMatrices.Add(matrix);
                    break;
                case BuildingType.House:
                    houseMatrices.Add(matrix);
                    break;
                case BuildingType.Bookshop:
                    bookshopMatrices.Add(matrix);
                    break;
            }
        }

        if (farmMatrices.Count > 0)
            Graphics.DrawMeshInstanced(farmMesh, 0, farmMaterial, farmMatrices.ToArray());

        if (schoolMatrices.Count > 0)
            Graphics.DrawMeshInstanced(schoolMesh, 0, schoolMaterial, schoolMatrices.ToArray());

        if (museumMatrices.Count > 0)
            Graphics.DrawMeshInstanced(museumMesh, 0, museumMaterial, museumMatrices.ToArray());

        if (houseMatrices.Count > 0)
            Graphics.DrawMeshInstanced(houseMesh, 0, houseMaterial, houseMatrices.ToArray());

        if (bookshopMatrices.Count > 0)
            Graphics.DrawMeshInstanced(bookshopMesh, 0, bookshopMaterial, bookshopMatrices.ToArray());
    }
}
