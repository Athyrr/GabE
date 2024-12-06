using GabE.Module.ECS;
using UnityEngine;

using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;


[UpdateInGroup(typeof(ECS_LifecycleSystemGroup))]
[BurstCompile]
public partial struct ECS_StorageManagerSystem : ISystem
{
    int lastProcessedDay;

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var lifecycle = SystemAPI.GetSingleton<ECS_GlobalLifecyleFragment>();
        var storage = SystemAPI.GetSingletonBuffer<ECS_ResourceStorageFragment>();
        int currentDay = lifecycle.DayCount;

        if (currentDay == lastProcessedDay || currentDay == 0)
            return;

        lastProcessedDay = currentDay;

        var consumeResourceJob = new ConsumeResourceJob
        {
            ResourceType = ResourceType.Food,
            Quantity = lifecycle.Population * 1 // 1 > number food per person
        };

        Debug.Log("Consume resource");

        state.Dependency = consumeResourceJob.ScheduleParallel(state.Dependency);
        state.Dependency.Complete();

        // Display inventory
        foreach (var (inventory, entity) in SystemAPI.Query<DynamicBuffer<ECS_ResourceStorageFragment>>().WithEntityAccess())
        {
            foreach (var resource in inventory)
            {
                Debug.Log($"Resource: {resource.Type}, Quantity: {resource.Quantity}");
            }
        }
    }

    [BurstCompile]
    private partial struct AddResourceJob : IJobEntity
    {
        public ResourceType ResourceType;
        public int Quantity;

        public void Execute(ref DynamicBuffer<ECS_ResourceStorageFragment> inventory)
        {
            bool resourceFound = false;

            for (int i = 0; i < inventory.Length; i++)
            {
                if (inventory[i].Type != ResourceType)
                    continue;

                inventory[i] = new ECS_ResourceStorageFragment
                {
                    Type = inventory[i].Type,
                    Quantity = inventory[i].Quantity + Quantity
                };
                resourceFound = true;
                break;
            }

            if (!resourceFound)
            {
                inventory.Add(new ECS_ResourceStorageFragment
                {
                    Type = ResourceType,
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

        public void Execute(ref DynamicBuffer<ECS_ResourceStorageFragment> inventory)
        {
            for (int i = 0; i < inventory.Length; ++i)
            {
                if (inventory[i].Type != ResourceType)
                    continue;

                int newQuantity = math.max(0, inventory[i].Quantity - Quantity);
                inventory[i] = new ECS_ResourceStorageFragment
                {
                    Type = inventory[i].Type,
                    Quantity = newQuantity
                };

                return;
            }
        }
    }
}
