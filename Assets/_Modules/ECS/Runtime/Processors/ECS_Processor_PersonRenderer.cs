using Unity.Entities;
using Unity.Rendering;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Burst;
using UnityEngine;

[BurstCompile]
public partial struct ECS_Processor_PersonRenderer : ISystem
{
    //public void OnCreate(ref SystemState state)
    //{
    //    // Charger le matériel et le maillage nécessaires pour les entités Person
    //    var personMaterial = UnityEngine.Resources.Load<Material>("Materials/PersonMaterial");
    //    var personMesh = UnityEngine.Resources.Load<Mesh>("Meshes/PersonMesh");

    //    if (personMaterial == null || personMesh == null)
    //    {
    //        UnityEngine.Debug.LogError("Person material or mesh not found!");
    //        return;
    //    }

    //    // Stocker les données pour un RenderMesh par archétype
    //    var renderMesh = new RenderMesh
    //    {
    //        material = personMaterial,
    //        mesh = personMesh,
    //        castShadows = UnityEngine.Rendering.ShadowCastingMode.On,
    //        receiveShadows = true
    //    };

    //    state.EntityManager.AddComponentData(state.SystemHandle, renderMesh);
    //}

    //public void OnUpdate(ref SystemState state)
    //{
    //    foreach (var (person, entity) in SystemAPI.Query<ECS_Frag_Person>().WithEntityAccess())
    //    {
    //        // Vérifie si l'entité n'a pas encore de RenderMesh
    //        if (!state.EntityManager.HasComponent<RenderMesh>(entity))
    //        {
    //            // Ajoute un composant pour rendre l'entité visible
    //            state.EntityManager.AddComponent<RenderMesh>(entity);
    //            state.EntityManager.AddComponent<LocalToWorld>(entity);
    //            state.EntityManager.AddComponent(entity, new Translation
    //            {
    //                Value = new float3(
    //                    UnityEngine.Random.Range(-10f, 10f),
    //                    0f,
    //                    UnityEngine.Random.Range(-10f, 10f)
    //                )
    //            });
    //        }
    //    }
    //}
}
