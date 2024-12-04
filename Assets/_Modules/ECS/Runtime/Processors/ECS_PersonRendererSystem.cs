using GabE.Module.ECS;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

[UpdateInGroup(typeof(PresentationSystemGroup))]
public partial class ECS_PersonRendererSystem : SystemBase
{
    private Material personMaterial;
    private Mesh personMesh;

    protected override void OnCreate()
    {
        personMaterial = Resources.Load<Material>("Materials/MT_Person");
        var personPrefab = Resources.Load<GameObject>("Meshes/MSH_Person");
        personMesh = personPrefab?.GetComponent<MeshFilter>()?.sharedMesh;

        if (personMaterial == null || personMesh == null)
            Debug.LogError("Person material or mesh not found!");
    }

    protected override void OnUpdate()
    {
        if (personMaterial == null || personMesh == null)
            return;

        var matrices = new NativeList<Matrix4x4>(Allocator.TempJob);

        Entities.ForEach((in ECS_Frag_Position position, in ECS_WorkerFragment worker) =>
        {
            matrices.Add(Matrix4x4.Translate(position.Position));
        }).Run();

        if (matrices.Length > 0)
            Graphics.DrawMeshInstanced(personMesh, 0, personMaterial, matrices.AsArray().ToArray());

        matrices.Dispose();
    }
}
