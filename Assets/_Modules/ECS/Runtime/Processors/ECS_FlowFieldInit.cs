using GabE.Module.ECS;
using Unity.Collections;
using Unity.Entities;


public partial struct ECS_FlowFieldInit : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        Entity e = state.EntityManager.CreateEntity();
        state.EntityManager.AddComponent<ECS_GridFragment>(e);
        state.EntityManager.SetComponentData<ECS_GridFragment>(e, new ECS_GridFragment { CellSize = 1 });
    }

    public void OnUpdate(ref SystemState state)
    {
        NativeArray<Entity> entities = new NativeArray<Entity>();
        int i = 0;
        foreach (var (request, entity) in SystemAPI.Query<RefRO<ECS_BuildListenerFragment>>().WithEntityAccess())
        {
            entities[i] = entity;
            i++;
        }

        foreach (var request in SystemAPI.Query<RefRO<ECS_FlowFieldRequest>>())
        {
            Entity e = state.EntityManager.CreateEntity();
            var reqPos = request.ValueRO.FlowFieldTargetPos;
            DynamicBuffer<ECS_CellData> cellData = state.EntityManager.AddBuffer<ECS_CellData>(e);

        }
    }


}
