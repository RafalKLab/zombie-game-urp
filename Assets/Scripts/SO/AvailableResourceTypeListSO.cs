using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/ResourceTypeList")]
public class AvailableResourceTypeListSO : ScriptableObject
{
    public List<ResourceTypeSO> list;
}
