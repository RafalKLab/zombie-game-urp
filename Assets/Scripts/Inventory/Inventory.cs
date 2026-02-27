using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public event Action OnChanged;
    private void NotifyChanged() => OnChanged?.Invoke();

    [Header("Normal Slots")]
    [SerializeField] private int capacity = 8;

    [Header("Huge Slots")]
    [SerializeField] private int hugeCapacity = 2;

    [Header("Starting Items")]
    [SerializeField] private List<ItemStack> startingItems = new();
    [SerializeField] private bool loadStartingItemsOnAwake = true;


    [Header("Debug")]
    [SerializeField] private bool debugLog = false;
    [SerializeField] private float debugInterval = 1f;

    private float debugTimer;

    private bool initialized;
    private List<ItemStack> slots;      // normal
    private List<ItemStack> hugeSlots;  // huge

    private void Awake()
    {
        EnsureInitialized();

        if (loadStartingItemsOnAwake)
            LoadStartingStacks();
    }

    private void LoadStartingStacks()
    {
        if (startingItems == null || startingItems.Count == 0)
            return;

        for (int i = 0; i < startingItems.Count; i++)
        {
            ItemStack s = startingItems[i];
            if (s == null || s.definition == null) continue;
            if (s.amount <= 0) continue;

            ItemStack copy = new ItemStack(s.definition, s.amount)
            {
                weaponRuntimeState = s.weaponRuntimeState
            };

            InsertStack(copy);
        }
    }

    private void Update()
    {
        if (!debugLog) return;

        debugTimer -= Time.deltaTime;
        if (debugTimer > 0f) return;

        debugTimer = debugInterval;
        DebugPrintInventory();
    }

    public IReadOnlyList<ItemStack> GetNormalSlots() => slots;
    public IReadOnlyList<ItemStack> GetHugeSlots() => hugeSlots;

    public bool HasSpaceFor(ItemDefinitionSO item, int amount)
    {
        if (item == null || amount <= 0) return false;
        return GetAddableAmount(item) >= amount;
    }

    public int GetAddableAmount(ItemDefinitionSO item)
    {
        if (item == null) return 0;

        var target = (item.requiredSlot == InventorySlotType.Huge) ? hugeSlots : slots;

        // Huge: 1 sztuka = 1 pusty huge slot
        if (item.requiredSlot == InventorySlotType.Huge)
        {
            int empty = 0;
            for (int i = 0; i < target.Count; i++)
                if (target[i] == null) empty++;

            return empty;
        }

        // Normal: stack space + empty slots
        int addable = 0;

        if (item.stackable)
        {
            for (int i = 0; i < target.Count; i++)
            {
                var s = target[i];
                if (s == null) continue;
                if (s.definition != item) continue;
                if (s.IsFull()) continue;

                addable += s.SpaceLeft();
            }
        }

        for (int i = 0; i < target.Count; i++)
        {
            if (target[i] != null) continue;

            addable += item.stackable ? item.maxStack : 1;
        }

        return addable;
    }

    public bool TryAdd(ItemDefinitionSO item, int amount)
    {
        return TryAddReturnRemaining(item, amount) == 0;
    }

    // Zwraca ile NIE weszlo (0 = wszystko weszlo)
    public int TryAddReturnRemaining(ItemDefinitionSO item, int amount)
    {
        if (item == null || amount <= 0) return amount;

        var target = (item.requiredSlot == InventorySlotType.Huge) ? hugeSlots : slots;
        int remaining = amount;

        // 1) Stackowanie
        if (item.stackable && item.requiredSlot == InventorySlotType.Normal)
        {
            for (int i = 0; i < target.Count; i++)
            {
                var stack = target[i];
                if (stack == null) continue;
                if (stack.definition != item) continue;
                if (stack.IsFull()) continue;

                int toAdd = Mathf.Min(stack.SpaceLeft(), remaining);
                stack.amount += toAdd;
                remaining -= toAdd;

                if (remaining <= 0)
                {
                    NotifyChanged();
                    return 0;
                }
            }
        }

        // 2) Puste sloty
        for (int i = 0; i < target.Count; i++)
        {
            if (target[i] != null) continue;

            int stackAmount = (item.requiredSlot == InventorySlotType.Huge)
                ? 1
                : (item.stackable ? Mathf.Min(item.maxStack, remaining) : 1);

            target[i] = new ItemStack(item, stackAmount);
            remaining -= stackAmount;

            if (remaining <= 0)
            {
                NotifyChanged();
                return 0;
            }
        }

        if (remaining != amount)
            NotifyChanged();

        return remaining;
    }

    public int TryRemove(ItemDefinitionSO item, int amount)
    {
        if (item == null || amount <= 0) return 0;

        var target = (item.requiredSlot == InventorySlotType.Huge) ? hugeSlots : slots;

        int remaining = amount;
        int removed = 0;

        for (int i = 0; i < target.Count; i++)
        {
            var stack = target[i];
            if (stack == null) continue;
            if (stack.definition != item) continue;

            int take = Mathf.Min(stack.amount, remaining);

            stack.amount -= take;
            remaining -= take;
            removed += take;

            if (stack.amount <= 0)
                target[i] = null;

            if (remaining <= 0)
                break;
        }

        if (removed > 0) NotifyChanged();

        return removed;
    }

    public bool RemoveStack(ItemStack stack)
    {
        if (stack == null || stack.definition == null) return false;

        var target = stack.definition.requiredSlot == InventorySlotType.Huge
            ? hugeSlots
            : slots;

        for (int i = 0; i < target.Count; i++)
        {
            if (target[i] == stack)
            {
                target[i] = null;

                NotifyChanged();

                return true;
            }
        }

        return false;
    }

    public bool InsertStack(ItemStack stack)
    {
        if (stack == null || stack.definition == null)
            return false;

        var target = stack.definition.requiredSlot == InventorySlotType.Huge
            ? hugeSlots
            : slots;

        for (int i = 0; i < target.Count; i++)
        {
            if (target[i] == null)
            {
                target[i] = stack;

                NotifyChanged();

                return true;
            }
        }

        return false; // brak miejsca
    }

    public bool HasEmptySlotFor(ItemStack stack)
    {
        if (stack == null || stack.definition == null)
            return false;

        var target = stack.definition.requiredSlot == InventorySlotType.Huge
            ? hugeSlots
            : slots;

        for (int i = 0; i < target.Count; i++)
        {
            if (target[i] == null)
                return true;
        }

        return false;
    }

    public int GetTotalAmount(ItemDefinitionSO item)
    {
        if (item == null) return 0;

        int total = 0;

        foreach (var s in slots)
            if (s != null && s.definition == item)
                total += s.amount;

        foreach (var s in hugeSlots)
            if (s != null && s.definition == item)
                total += s.amount;

        return total;
    }

    private void DebugPrintInventory()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("=== INVENTORY ===");

        for (int i = 0; i < slots.Count; i++)
        {
            var s = slots[i];
            if (s == null) sb.AppendLine($"Normal Slot {i}: EMPTY");
            else sb.AppendLine($"Normal Slot {i}: {s.definition.displayName} ({s.amount})");
        }

        for (int i = 0; i < hugeSlots.Count; i++)
        {
            var s = hugeSlots[i];
            if (s == null) sb.AppendLine($"Huge Slot {i}: EMPTY");
            else sb.AppendLine($"Huge Slot {i}: {s.definition.displayName} ({s.amount})");
        }

        Debug.Log(sb.ToString());
    }

    /// <summary>
    /// Zjada z inventory do 'requested' sztuk danego itemu.
    /// Zwraca ile faktycznie zjadlo (0..requested).
    /// remainingNeeded = requested - taken.
    /// remainingInInventory = ile zostalo tego itemu w inventory po operacji.
    /// </summary>
    public int TryConsumeUpToAndGetRemaining(
        ItemDefinitionSO item,
        int requested,
        out int remainingNeeded,
        out int remainingInInventory)
    {
        EnsureInitialized();

        remainingNeeded = Mathf.Max(0, requested);
        remainingInInventory = 0;

        if (item == null || requested <= 0)
            return 0;

        var target = (item.requiredSlot == InventorySlotType.Huge) ? hugeSlots : slots;

        int remaining = requested;
        int takenTotal = 0;

        for (int i = 0; i < target.Count; i++)
        {
            var stack = target[i];
            if (stack == null) continue;
            if (stack.definition != item) continue;

            int take = Mathf.Min(stack.amount, remaining);

            stack.amount -= take;
            remaining -= take;
            takenTotal += take;

            if (stack.amount <= 0)
                target[i] = null;

            if (remaining <= 0)
                break;
        }

        remainingNeeded = remaining;

        remainingInInventory = GetTotalAmount(item);

        return takenTotal;
    }

    private void EnsureInitialized()
    {
        if (initialized) return;

        slots = new List<ItemStack>(capacity);
        for (int i = 0; i < capacity; i++)
            slots.Add(null);

        hugeSlots = new List<ItemStack>(hugeCapacity);
        for (int i = 0; i < hugeCapacity; i++)
            hugeSlots.Add(null);

        initialized = true;
    }

    public bool TryGetFirstWeaponStack(out ItemStack weaponStack)
    {
        weaponStack = null;

        EnsureInitialized();

        if (hugeSlots == null) return false;

        for (int i = 0; i < hugeSlots.Count; i++)
        {
            ItemStack stack = hugeSlots[i];
            if (stack == null) continue;
            if (stack.definition == null) continue;

            if (stack.definition is WeaponItemSO)
            {
                weaponStack = stack;
                return true;
            }
        }

        return false;
    }
}

[Serializable]
public class ItemStack
{
    public ItemDefinitionSO definition;
    public int amount;
    public WeaponRuntimeState weaponRuntimeState;

    public ItemStack(ItemDefinitionSO definition, int amount)
    {
        this.definition = definition;
        this.amount = amount;
    }

    public bool IsFull()
    {
        if (!definition.stackable) return true;
        return amount >= definition.maxStack;
    }

    public int SpaceLeft()
    {
        if (!definition.stackable) return 0;
        return definition.maxStack - amount;
    }
}

public enum InventorySlotType
{
    Normal,
    Huge
}