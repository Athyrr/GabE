using GabE.Module.ECS;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

[UpdateInGroup(typeof(PresentationSystemGroup))]
public partial class ECS_PersonRendererSystem : SystemBase
{
    private Material personMaterial;
    private Mesh personMesh;

    protected override void OnCreate()
    {
        personMaterial = Resources.Load<Material>("Workers/Materials/MT_Person");
        GameObject personPrefab = Resources.Load<GameObject>("Workers/Meshes/MSH_Person");
        personMesh = personPrefab.GetComponent<MeshFilter>()?.sharedMesh;

        if (personMaterial == null || personMesh == null)
            Debug.LogError("Person material or mesh not found!");
    }

    protected override void OnUpdate()
    {
        if (personMaterial == null || personMesh == null)
            return;

        var matrices = new List<Matrix4x4>();

        foreach (var (position, worker) in SystemAPI.Query<RefRO<ECS_Frag_Position>, RefRO<ECS_WorkerFragment>>())
            matrices.Add(Matrix4x4.Translate(position.ValueRO.Position));

        Graphics.DrawMeshInstanced(personMesh, 0, personMaterial, matrices.ToArray() /*512*/);
    }
}
