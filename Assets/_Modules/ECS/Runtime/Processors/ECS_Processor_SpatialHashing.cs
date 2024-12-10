// using Unity.Burst;
// using Unity.Entities;
//
// namespace GabE.Module.ECS
// {
//     [BurstCompile]
//     [UpdateInGroup(typeof(InitializationSystemGroup))]
//     public partial struct ECS_Processor_InventoryManagerInitializer : ISystem
//     {
//         private byte spatialHashingIDBufferLength;
//         
//         public void OnCreate(ref SystemState state)
//         {
//             var entity = state.EntityManager.CreateEntity();
//             var buffer = state.EntityManager.AddBuffer<ECS_Frag_SpatialHashingData>(entity);
//             
//             // limit of bytes => 255
//             spatialHashingIDBufferLength = 20;
//             for (byte i = 0; i>=spatialHashingIDBufferLength; ++i)
//                 buffer.Add(new ECS_Frag_SpatialHashingData { ID = i });
//         }
//     }
//     
//     public partial struct ECS_Processor_SpatialHashing : ISystem
//     {
//         
//         [BurstCompile]
//         public void OnUpdate(ref SystemState state)
//         {
//             
//             EntityManager entityManager = state.EntityManager;
//
//             foreach (var (positionRequest,entity) in SystemAPI.Query<RefRW<ECS_PositionFragment>>().WithEntityAccess())
//             {
//                 int a = positionRequest.ValueRO.SpatialHashingID;
//                 DynamicBuffer<ECS_Frag_SpatialHashingData> SHBuffer = entityManager.GetBuffer<ECS_Frag_SpatialHashingData>(entity, false);
//
//                 for (int i = 0; i < SHBuffer.Length; ++i)
//                 {
//                     if (positionRequest.ValueRO.SpatialHashingID == SHBuffer[0].ID)
//                     {
//                         
//                     }
//                 }
//             }
//         }
//         
//         [BurstCompile]
//         public void OnDestroy(ref SystemState state)
//         {
//
//         }
//     }
// }