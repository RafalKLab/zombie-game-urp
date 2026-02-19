using System.Collections.Generic;
using UnityEngine;

public class FactionBaseRegistry : MonoBehaviour
{
    public static FactionBaseRegistry Instance { get; private set; }

    [SerializeField] private List<FactionBaseMap> factionBases;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Multiple FactionBaseRegistry instances detected. Destroying duplicate.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public BaseManager GetBaseManagerByFaction(Faction faction)
    {
        foreach (FactionBaseMap map in factionBases)
        {
            if (map.faction == faction)
                return map.baseManager;
        }

        Debug.LogWarning($"No BaseManager assigned for faction: {faction}");
        return null;
    }
}

[System.Serializable]
public class FactionBaseMap
{
    public Faction faction;
    public BaseManager baseManager;
}
