using GabE.Module.ECS;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile]
public partial struct ECS_Processor_InventoryManager : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        Debug.Log("Update inventory");



        //SystemAPI.Query(DynamicBuffer<ECS_Frag_Resource>) d



        //Entities.ForEach((DynamicBuffer<ECS_Frag_Resource> inventory) =>
        //{
        //    AddResource(inventory, ResourceType.Wood, 20);

        //    ConsumeResource(inventory, ResourceType.Food, 10);

        //    //@todo remove debug  Display resources
        //    foreach (var resource in inventory)
        //    {
        //        UnityEngine.Debug.Log($"Resource: {resource.ResourceType}, Quantity: {resource.Quantity}");
        //    }
        //}).Run(); // Schedule() if async.
    }

    private void AddResource(DynamicBuffer<ECS_Frag_Resource> inventory, ResourceType resourceType, int quantity)
    {
        for (int i = 0; i < inventory.Length; i++)
        {
            if (inventory[i].ResourceType == resourceType)
            {
                inventory[i] = new ECS_Frag_Resource
                {
                    ResourceType = inventory[i].ResourceType,
                    Quantity = inventory[i].Quantity + quantity
                };
                return;
            }
        }

        // @todo if delete this, remove consumeresource release step.
        inventory.Add(new ECS_Frag_Resource { ResourceType = resourceType, Quantity = quantity });
    }

    private void ConsumeResource(DynamicBuffer<ECS_Frag_Resource> inventory, ResourceType resourceType, int quantity)
    {
        for (int i = 0; i < inventory.Length; i++)
        {
            if (inventory[i].ResourceType != resourceType)
                continue;

            int newQuantity = math.max(0, inventory[i].Quantity - quantity);
            inventory[i] = new ECS_Frag_Resource
            {
                ResourceType = inventory[i].ResourceType,
                Quantity = newQuantity
            };

            // @todo Do we need to release an inventory space ?
            if (newQuantity == 0)
                inventory.RemoveAt(i);

            return;
        }

        UnityEngine.Debug.LogWarning($"Tried to consume {quantity} of {resourceType}, but it doesn't exist in the inventory.");
    }
}
