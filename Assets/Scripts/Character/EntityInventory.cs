using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkedVar;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// CharacterInventoryAndInteraction handles:
/// - Item interaction and inventory
/// - World interaction
/// </summary>
public class EntityInventory : NetworkedBehaviour
{
    #region Constructor and Variables

    [Header("References")]
    private readonly NetworkedVar<Inventory> MainInventory = new NetworkedVar<Inventory>(
        new NetworkedVarSettings { WritePermission = NetworkedVarPermission.Everyone }, 
        new Inventory(4, 10));
    private readonly NetworkedVar<Inventory> HotbarInventory = new NetworkedVar<Inventory>(
        new NetworkedVarSettings { WritePermission = NetworkedVarPermission.Everyone },
        new Inventory(1, 10));
    private readonly NetworkedVar<ItemStack> MouseSlot = new NetworkedVar<ItemStack>(
        new NetworkedVarSettings { WritePermission = NetworkedVarPermission.Everyone }, 
        new ItemStack(InventoryManager.NULL_ITEM_ID, 0));

    #endregion

    #region Input Events

    /// <summary>
    /// Player presses Inventory key. 
    /// Opens inventory.
    /// </summary>
    /// <param name="input"></param>
    public void OnInventory(InputValue _)
    {
        InventoryManager.Singleton.InventoryUI.ToggleInventory();
    }
    
    /// <summary>
    /// Player presses Primary Action key.
    /// Performs primary action on selected hotbar item.
    /// </summary>
    /// <param name="input"></param>
    public void OnPrimaryAction(InputValue _)
    {
        if (!InventoryManager.Singleton.InventoryUI.mainInventoryOpen)
        {
            var itemStack = InventoryManager.Singleton.HotbarUI.GetSelectedItemStack();
            var newStack = itemStack.GetItemDefinition().OnUsePrimary(itemStack, this);
            if (newStack != null)
            {
                InventoryManager.Singleton.HotbarUI.SetSelectedItemStack(newStack);
            }
        }
    }

    /// <summary>
    /// Player presses Secondary Action key.
    /// Performs secondary action on selected hotbar item.
    /// </summary>
    /// <param name="input"></param>
    public void OnSecondaryAction(InputValue _)
    {
        if (!InventoryManager.Singleton.InventoryUI.mainInventoryOpen)
        {
            var itemStack = InventoryManager.Singleton.HotbarUI.GetSelectedItemStack();
            var newStack = itemStack.GetItemDefinition().OnUseSecondary(itemStack, this);
            if (newStack != null)
            {
                InventoryManager.Singleton.HotbarUI.SetSelectedItemStack(newStack);
            }
        }
    }

    /// <summary>
    /// Player scrolls with HotbarScroll input.
    /// Toggle selected item in hotbar.
    /// </summary>
    /// <param name="scroll"></param>
    public void OnHotbarScroll(InputValue scroll)
    {
        var scrollValue = scroll.Get<Vector2>().y;
        if (scrollValue > 20)
        {
            InventoryManager.Singleton.HotbarUI.NavigateToLeft();
        }
        else if (scrollValue < -20)
        {
            InventoryManager.Singleton.HotbarUI.NavigateToRight();
        }
    }

    #endregion

    #region Inventory Manipulation

    public Inventory GetMainInventory()
    {
        return MainInventory.Value;
    }

    public Inventory GetHotbarInventory()
    {
        return HotbarInventory.Value;
    }

    public ItemStack GetMouseSlot()
    {
        return MouseSlot.Value;
    }

    /// <summary>
    /// Insert an item stack into this character's overall inventory.
    /// Hotbar takes priority, then Main Inventory.
    /// </summary>
    /// <param name="stack"></param>
    /// <returns></returns>
    [ServerRPC(RequireOwnership = false)]
    public ItemStack PickupItemStack(ItemStack stack)
    {
        Debug.Log($"Attempting to insert {stack} into Player {OwnerClientId}'s inventory.");
        var overflow = HotbarInventory.Value.InsertItemStackIntoInventory(stack);
        overflow = MainInventory.Value.InsertItemStackIntoInventory(overflow);
        
        HotbarInventory.isDirty = true;
        MainInventory.isDirty = true;
        MouseSlot.isDirty = true;
        
        return overflow;
    }

    #endregion

    #region Dev Commands

    public void GiveBandages()
    {
        PickupItemStack(new ItemStack("bandage", 20));
    }

    #endregion
}