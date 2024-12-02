using GabE.Module.ECS;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;



[DisableAutoCreation]
[UpdateInGroup(typeof(ECS_Group_Lifecycle))]
[BurstCompile]
public partial struct ECS_Processor_InventoryManager : ISystem
{

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        //    var addResourceJob = new AddResourceJob
        //    {
        //        ResourceType = ResourceType.Wood,
        //        Quantity = 10
        //    };

        //    Debug.Log("Add resource");

        //    state.Dependency = addResourceJob.ScheduleParallel(state.Dependency);
        //    state.Dependency.Complete();

        //    var consumeResourceJob = new ConsumeResourceJob
        //    {
        //        ResourceType = ResourceType.Wood,
        //        Quantity = 5
        //    };

        //    Debug.Log("Consume resource");

        //    state.Dependency = consumeResourceJob.ScheduleParallel(state.Dependency);
        //    state.Dependency.Complete();

        // Display inventory
        foreach (var (inventory, entity) in SystemAPI.Query<DynamicBuffer<ECS_Frag_Resource>>().WithEntityAccess())
        {
            Debug.Log($"Entity {entity.Index}:");
            foreach (var resource in inventory)
            {
                Debug.Log($"Resource: {resource.ResourceType}, Quantity: {resource.Quantity}");
            }
        }
    }

    [BurstCompile]
    private partial struct AddResourceJob : IJobEntity
    {
        public ResourceType ResourceType;
        public int Quantity;

        public void Execute(ref DynamicBuffer<ECS_Frag_Resource> inventory)
        {


            bool resourceFound = false;

            for (int i = 0; i < inventory.Length; i++)
            {
                if (inventory[i].ResourceType == ResourceType)
                {
                    inventory[i] = new ECS_Frag_Resource
                    {
                        ResourceType = inventory[i].ResourceType,
                        Quantity = inventory[i].Quantity + Quantity
                    };
                    resourceFound = true;
                    break;
                }
            }

            if (!resourceFound)
            {
                inventory.Add(new ECS_Frag_Resource
                {
                    ResourceType = ResourceType,
                    Quantity = Quantity
                });
            }
        }
    }

    [BurstCompile]
    private partial struct ConsumeResourceJob : IJobEntity
    {
        public ResourceType ResourceType;
        public int Quantity;

        public void Execute(ref DynamicBuffer<ECS_Frag_Resource> inventory)
        {
            for (int i = 0; i < inventory.Length; i++)
            {
                if (inventory[i].ResourceType == ResourceType)
                {
                    int newQuantity = math.max(0, inventory[i].Quantity - Quantity);
                    inventory[i] = new ECS_Frag_Resource
                    {
                        ResourceType = inventory[i].ResourceType,
                        Quantity = newQuantity
                    };

                    if (newQuantity == 0)
                    {
                        inventory.RemoveAt(i);
                    }
                    return;
                }
            }

            Debug.LogWarning($"Tried to consume {Quantity} of {ResourceType}, but it doesn't exist in the inventory.");
        }
    }
}
