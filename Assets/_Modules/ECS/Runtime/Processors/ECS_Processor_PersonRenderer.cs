using GabE.Module.ECS;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

/// <summary>
/// Renders person entities using instanced drawing for performance.
/// </summary>
[UpdateInGroup(typeof(PresentationSystemGroup))]
public partial class ECS_Processor_PersonRenderer : SystemBase
{
    private Material personMaterial;
    private Mesh personMesh;

    /// <summary>
    /// Initializes the system by loading the person material and mesh.
    /// </summary>
    protected override void OnCreate()
    {
        personMaterial = Resources.Load<Material>("Materials/MT_Person"); //@todo use Asset Bundle
        var personPrefab = Resources.Load<GameObject>("Meshes/MSH_Person");
        personMesh = personPrefab?.GetComponent<MeshFilter>()?.sharedMesh;

        if (personMaterial == null || personMesh == null)
        {
            Debug.LogError("Person material or mesh not found!");
            return;
        }
    }

    /// <summary>
    /// Updates the system by gathering person positions and drawing them.
    /// </summary>
    protected override void OnUpdate()
    {
        // Create a NativeList to store the transformation matrices
        // Use a default capacity to avoid unnecessary reallocations
        NativeList<Matrix4x4> positionsMatrices = new NativeList<Matrix4x4>(1000, Allocator.TempJob);

        // Job to prepare the matrices in parallel
        var setupMaticesJob = new SetupMatricesJob
        {
            PositionsMatrices = positionsMatrices.AsParallelWriter()
        };

        // Schedule, execute and complete the job
        Dependency = setupMaticesJob.ScheduleParallel(Dependency);
        Dependency.Complete();


        if (positionsMatrices.Length > 0)
        {
            // Convert NativeList to NativeArray
            NativeArray<Matrix4x4> positionsMatricesNativeArray = new NativeArray<Matrix4x4>(positionsMatrices.Length, Allocator.Temp);

            // Copy data from NativeList to NativeArray
            for (int i = 0; i < positionsMatrices.Length; i++)
                positionsMatricesNativeArray[i] = positionsMatrices[i];

            // Convert NativeArray to a standard array
            Matrix4x4[] matricesArray = positionsMatricesNativeArray.ToArray();

            // Draw on GPU all instances of the person mesh
            Graphics.DrawMeshInstanced(personMesh, 0, personMaterial, matricesArray);

            // Dispose the NativeArray.
            positionsMatricesNativeArray.Dispose();
        }

        // Dispose the NativeList
        positionsMatrices.Dispose();
    }



    /// <summary>
    /// Job to set up the transformation matrices for each person entity.
    /// </summary>
    [BurstCompile]
    private partial struct SetupMatricesJob : IJobEntity
    {
        /// <summary>
        /// Parallel writer for the positions matrices.
        /// </summary>
        public NativeList<Matrix4x4>.ParallelWriter PositionsMatrices;

        /// <summary>
        /// Executes the job for each person entity.
        /// </summary>
        /// <param name="position">The position component of the entity.</param>
        private void Execute([ReadOnly] ECS_Frag_Position position)
        {
            PositionsMatrices.AddNoResize(Matrix4x4.Translate(position.Position));
        }
    }
}
