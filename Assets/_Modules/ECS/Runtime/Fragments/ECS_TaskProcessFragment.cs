using Unity.Entities;

namespace GabE.Module.ECS
{
    public struct ECS_Frag_TaskProcess : IComponentData
    {
        public float Duration;

        public float Progression;

        public bool HasFinished;
    }
}
