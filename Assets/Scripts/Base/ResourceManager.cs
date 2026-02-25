using System;
using System.Collections.Generic;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    public event Action OnResourceAmountsChanged;

    [SerializeField] private AvailableResourceTypeListSO availableResourceTypeListSO;

    [Header("Init resources amounts")]
    [SerializeField] private List<InitResourceData> initResourceData;

    [Header("Exchange tables")]
    [SerializeField] private ResourceItemExchangeTableSO ammoExchangeTable;

    // Cache: item -> units per 1 currency
    private Dictionary<ItemDefinitionSO, int> ammoExchangeRatesByItem;

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

        ApplyInitResources();
        BuildAmmoExchangeDictionary();
    }

    private void BuildAmmoExchangeDictionary()
    {
        ammoExchangeRatesByItem = new Dictionary<ItemDefinitionSO, int>();

        if (ammoExchangeTable == null)
        {
            Debug.LogWarning("[ResourceManager] ammoExchangeTable is null.");
            return;
        }

        if (ammoExchangeTable.itemExchangeRates == null)
        {
            Debug.LogWarning("[ResourceManager] ammoExchangeTable.itemExchangeRates is null.");
            return;
        }

        foreach (var rate in ammoExchangeTable.itemExchangeRates)
        {
            if (rate == null) continue;
            if (rate.item == null) continue;
            if (rate.unitsPerCurrency <= 0) continue;

            ammoExchangeRatesByItem[rate.item] = rate.unitsPerCurrency;
        }
    }

    /// <summary>
    /// Preview only: returns how many units you COULD receive, without spending any currency.
    /// Useful for CanInteract checks / UI previews.
    /// </summary>
    public bool TryQuoteItemFromResource(ItemDefinitionSO item, int requestedUnits, out int grantedUnits, out int currencyCost)
    {
        grantedUnits = 0;
        currencyCost = 0;

        if (!TryCalculateWithdrawal(item, requestedUnits, out grantedUnits, out currencyCost))
            return false;

        // Quote does NOT modify any resources.
        return true;
    }

    /// <summary>
    /// Commit: spends currency and returns how many units were granted.
    /// </summary>
    public bool TryWithdrawItemFromResource(ItemDefinitionSO item, int requestedUnits, out int grantedUnits)
    {
        grantedUnits = 0;

        if (!TryCalculateWithdrawal(item, requestedUnits, out grantedUnits, out int currencyCost))
            return false;

        // Commit: deduct currency now
        ResourceItemExchangeTableSO table = GetExchangeTableForItem(item);
        AddResourceAmount(table.currencyResourceType, -currencyCost);

        return true;
    }

    /// <summary>
    /// Shared calculation used by both Quote and Commit.
    /// "Bundle model": you pay only for full bundles of unitsPerCurrency (no currency waste).
    /// </summary>
    private bool TryCalculateWithdrawal(ItemDefinitionSO item, int requestedUnits, out int grantedUnits, out int currencyCost)
    {
        grantedUnits = 0;
        currencyCost = 0;

        if (item == null) return false;
        if (requestedUnits <= 0) return false;

        // 1) Select exchange table based on item type
        ResourceItemExchangeTableSO table = GetExchangeTableForItem(item);
        if (table == null) return false;

        // 2) Get rate
        if (!TryGetUnitsPerCurrency(table, item, out int unitsPerCurrency)) return false;
        if (unitsPerCurrency <= 0) return false;

        // 3) Available currency
        int availableCurrency = GetResourceAmount(table.currencyResourceType);
        if (availableCurrency <= 0) return false;

        // 4) Bundle model (no currency waste)
        int maxCurrencyWeCanSpend = Mathf.Min(availableCurrency, requestedUnits / unitsPerCurrency); // floor division
        if (maxCurrencyWeCanSpend <= 0) return false;

        currencyCost = maxCurrencyWeCanSpend;
        grantedUnits = maxCurrencyWeCanSpend * unitsPerCurrency;

        return true;
    }

    private ResourceItemExchangeTableSO GetExchangeTableForItem(ItemDefinitionSO item)
    {
        if (item is WeaponAmmoItemSO) return ammoExchangeTable;
        return null;
    }

    /// <summary>
    /// Preview: how many units you get for spending exactly 1 currency.
    /// </summary>
    public bool TryQuoteOneCurrency(ItemDefinitionSO item, out int grantedUnits)
    {
        grantedUnits = 0;

        if (item == null) return false;

        var table = GetExchangeTableForItem(item);
        if (table == null) return false;

        if (!TryGetUnitsPerCurrency(table, item, out int unitsPerCurrency)) return false;
        if (unitsPerCurrency <= 0) return false;

        int availableCurrency = GetResourceAmount(table.currencyResourceType);
        if (availableCurrency < 1) return false;

        grantedUnits = unitsPerCurrency;
        return true;
    }

    /// <summary>
    /// Commit: spend exactly 1 currency and grant unitsPerCurrency.
    /// </summary>
    public bool TryWithdrawOneCurrency(ItemDefinitionSO item, out int grantedUnits)
    {
        grantedUnits = 0;

        if (!TryQuoteOneCurrency(item, out grantedUnits))
            return false;

        var table = GetExchangeTableForItem(item);
        if (table == null) return false;

        AddResourceAmount(table.currencyResourceType, -1);
        return true;
    }

    private bool TryGetUnitsPerCurrency(ResourceItemExchangeTableSO table, ItemDefinitionSO item, out int unitsPerCurrency)
    {
        unitsPerCurrency = 0;
        if (table == null || item == null) return false;

        if (table == ammoExchangeTable)
        {
            if (ammoExchangeRatesByItem == null) return false;
            return ammoExchangeRatesByItem.TryGetValue(item, out unitsPerCurrency);
        }

        if (table.itemExchangeRates == null) return false;

        foreach (ItemExchangeRate rate in table.itemExchangeRates)
        {
            if (rate == null) continue;
            if (rate.item != item) continue;
            if (rate.unitsPerCurrency <= 0) return false;

            unitsPerCurrency = rate.unitsPerCurrency;
            return true;
        }

        return false;
    }

    private void ApplyInitResources()
    {
        if (initResourceData == null) return;

        foreach (var data in initResourceData)
        {
            if (data == null || data.ResourceTypeSO == null) continue;

            if (!resourceAmounts.ContainsKey(data.ResourceTypeSO))
                resourceAmounts[data.ResourceTypeSO] = 0;

            resourceAmounts[data.ResourceTypeSO] = Mathf.Max(0, data.amount);
        }

        OnResourceAmountsChanged?.Invoke();
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

        if (!resourceAmounts.TryGetValue(resourceTypeSO, out int before))
            before = 0;

        int after = Mathf.Max(0, before + amount);

        // Notify only if value actually changed
        if (after == before) return;

        resourceAmounts[resourceTypeSO] = after;
        OnResourceAmountsChanged?.Invoke();
    }

    public Dictionary<ResourceTypeSO, int> GetResourceAmounts()
    {
        return resourceAmounts;
    }
}

[Serializable]
public class InitResourceData
{
    public ResourceTypeSO ResourceTypeSO;
    public int amount;
}