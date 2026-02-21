using System;
using System.Collections.Generic;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    public event Action OnResourceAmountsChanged;

    [SerializeField] private AvailableResourceTypeListSO availableResourceTypeListSO;

    private Dictionary<ResourceTypeSO, int> resourceAmounts = new();

    private void Awake()
    {
        resourceAmounts = new Dictionary<ResourceTypeSO, int>();

        foreach (ResourceTypeSO resourceType in availableResourceTypeListSO.list)
        {
            if (resourceType == null) continue;
            if (!resourceAmounts.ContainsKey(resourceType))
                resourceAmounts.Add(resourceType, 0);
        }
    }

    public int GetResourceAmount(ResourceTypeSO resourceTypeSO)
    {
        if (resourceTypeSO == null) return 0;
        if (resourceAmounts.TryGetValue(resourceTypeSO, out int amount)) return amount;

        return 0;
    }

    public void AddResourceAmount(ResourceTypeSO resourceTypeSO, int amount)
    {
        if (resourceTypeSO == null) return;

        if (!resourceAmounts.ContainsKey(resourceTypeSO))
            resourceAmounts[resourceTypeSO] = 0;

        resourceAmounts[resourceTypeSO] += amount;

        resourceAmounts[resourceTypeSO] = Mathf.Max(0, resourceAmounts[resourceTypeSO]);

        OnResourceAmountsChanged?.Invoke();
    }

    public Dictionary<ResourceTypeSO, int> GetResourceAmounts()
    {
        return resourceAmounts;
    }
}
