using System.Collections.Generic;
using UnityEngine;

public class ResourcesUI : MonoBehaviour
{
    [SerializeField] private ResourceManager resourceManager;
    [SerializeField] private Transform resourcesContainer;
    [SerializeField] private ResourceTypeLineItemUI resourceTypeLineItemUIPrefab;

    private Dictionary<ResourceTypeSO, ResourceTypeLineItemUI> resourceLineItemMap = new();

    private void OnEnable()
    {
        if (!ValidateReferences()) return;

        if (resourceManager != null)
            resourceManager.OnResourceAmountsChanged += ResourceManager_OnResourceAmountsChanged;
    }

    private void OnDisable()
    {
        if (resourceManager != null)
            resourceManager.OnResourceAmountsChanged -= ResourceManager_OnResourceAmountsChanged;
    }

    private void Start()
    {
        if (!ValidateReferences()) return;

        resourceLineItemMap.Clear();

        Dictionary<ResourceTypeSO, int> resourceAmounts = resourceManager.GetResourceAmounts();

        foreach (KeyValuePair<ResourceTypeSO, int> pair in resourceAmounts)
        {
            if (pair.Key == null)
            {
                Debug.LogWarning("ResourcesUI: ResourceTypeSO key is null.", this);
                continue;
            }

            if (resourceLineItemMap.ContainsKey(pair.Key))
            {
                Debug.LogWarning($"ResourcesUI: Duplicate UI entry for {pair.Key.name}", this);
                continue;
            }

            ResourceTypeLineItemUI lineItem =
                Instantiate(resourceTypeLineItemUIPrefab, resourcesContainer);

            lineItem.SetText(BuildResourceText(pair.Key, pair.Value));

            resourceLineItemMap.Add(pair.Key, lineItem);
        }
    }

    private void ResourceManager_OnResourceAmountsChanged()
    {
        Dictionary<ResourceTypeSO, int> resourceAmounts = resourceManager.GetResourceAmounts();

        foreach (KeyValuePair<ResourceTypeSO, int> pair in resourceAmounts)
        {
            if (pair.Key == null) continue;

            if (!resourceLineItemMap.TryGetValue(pair.Key, out var lineItem))
            {
                Debug.LogWarning($"ResourcesUI: Missing line item for {pair.Key.name}", this);
                continue;
            }

            lineItem.SetText(BuildResourceText(pair.Key, pair.Value));
        }
    }

    private bool ValidateReferences()
    {
        if (resourceManager == null)
        {
            Debug.LogError("ResourcesUI: ResourceManager reference is missing.", this);
            return false;
        }

        if (resourcesContainer == null)
        {
            Debug.LogError("ResourcesUI: ResourcesContainer reference is missing.", this);
            return false;
        }

        if (resourceTypeLineItemUIPrefab == null)
        {
            Debug.LogError("ResourcesUI: ResourceTypeLineItemUIPrefab reference is missing.", this);
            return false;
        }

        return true;
    }

    private string BuildResourceText(ResourceTypeSO type, int amount)
    {
        string name = type != null ? type.displayName : "NULL";
        return $"{name}: {amount}";
    }
}