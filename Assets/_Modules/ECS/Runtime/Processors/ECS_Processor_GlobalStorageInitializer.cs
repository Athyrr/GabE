using Unity.Entities;

namespace GabE.Module.ECS
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    partial struct ECS_Processor_GlobalStorageInitializer : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            if (SystemAPI.TryGetSingleton<ECS_Frag_Resources>(out var resources))
                state.EntityManager.CreateSingleton(resources);

            resources.Initialize();


            var d = state.EntityManager.CreateEntity();
            state.EntityManager.AddComponent<ECS_Frag_Resources>(d);
            state.EntityManager.AddComponent<ECS_Frag_Resources>(d);
            state.EntityManager.AddComponent<ECS_Frag_Resources>(d);

        }
    }
}