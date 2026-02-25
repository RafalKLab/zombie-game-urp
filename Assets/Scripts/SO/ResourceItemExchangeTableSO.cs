using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/ResourceItemExchangeTable")]
public class ResourceItemExchangeTableSO : ScriptableObject
{
    public ResourceTypeSO currencyResourceType;

    public List<ItemExchangeRate> itemExchangeRates;
}

[Serializable]
public class ItemExchangeRate
{
    public ItemDefinitionSO item;
    public int unitsPerCurrency;
}
