using System.Collections.Generic;
using UnityEngine;

public class ResourcesUI : MonoBehaviour
{
    [SerializeField] private ResourceManager resourceManager;
    [SerializeField] private ResourceSlotUI resourceSlotUIPrefab;

    private Dictionary<ResourceTypeSO, ResourceSlotUI> resourceSlotMap = new();

    private void OnEnable()
    {
        if (!ValidateReferences()) return;

        if (resourceManager != null)
            resourceManager.OnResourceAmountsChanged += HandleResourceAmountsChanged;

        EnsureResourceSlotsExist();
        RefreshResourceSlots();
    }

    private void OnDisable()
    {
        if (resourceManager != null)
            resourceManager.OnResourceAmountsChanged -= HandleResourceAmountsChanged;
    }

    private void HandleResourceAmountsChanged()
    {
        Dictionary<ResourceTypeSO, int> resourceAmounts = resourceManager.GetResourceAmounts();

        foreach (KeyValuePair<ResourceTypeSO, int> resourceEntry in resourceAmounts)
        {
            if (resourceEntry.Key == null) continue;

            if (!resourceSlotMap.TryGetValue(resourceEntry.Key, out var slotUI))
            {
                Debug.LogWarning($"ResourcesUI: Missing UI slot for {resourceEntry.Key.name}", this);
                continue;
            }

            slotUI.SetName(GetResourceName(resourceEntry.Key));
            slotUI.SetAmount(resourceEntry.Value);
        }
    }

    private bool ValidateReferences()
    {
        if (resourceManager == null)
        {
            Debug.LogError("ResourcesUI: ResourceManager reference is missing.", this);
            return false;
        }

        if (resourceSlotUIPrefab == null)
        {
            Debug.LogError("ResourcesUI: resourceSlotUIPrefab reference is missing.", this);
            return false;
        }

        return true;
    }

    private string GetResourceName(ResourceTypeSO type)
    {
        return type != null ? type.displayName : "NULL";
    }

    private void RefreshResourceSlots()
    {
        Dictionary<ResourceTypeSO, int> resourceAmounts = resourceManager.GetResourceAmounts();

        foreach (KeyValuePair<ResourceTypeSO, int> resourceEntry in resourceAmounts)
        {
            if (resourceEntry.Key == null) continue;

            if (!resourceSlotMap.TryGetValue(resourceEntry.Key, out var slotUI))
            {
                Debug.LogWarning($"ResourcesUI: Missing UI slot for {resourceEntry.Key.name}", this);
                continue;
            }

            slotUI.SetName(GetResourceName(resourceEntry.Key));
            slotUI.SetAmount(resourceEntry.Value);
        }
    }

    private void EnsureResourceSlotsExist()
    {
        var resourceAmounts = resourceManager.GetResourceAmounts();

        foreach (var entry in resourceAmounts)
        {
            var type = entry.Key;
            if (type == null) continue;

            if (resourceSlotMap.ContainsKey(type))
                continue;

            ResourceSlotUI slot = Instantiate(resourceSlotUIPrefab, transform);
            slot.SetName(GetResourceName(type));
            slot.SetAmount(entry.Value);

            resourceSlotMap.Add(type, slot);
        }
    }
}
