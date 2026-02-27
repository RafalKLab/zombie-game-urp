using System.Collections.Generic;
using UnityEngine;

public class FactionRelationsManager : MonoBehaviour
{
    public static FactionRelationsManager Instance { get; private set; }

    [SerializeField] private FactionRelationsSO factionRelationsData;

    private Dictionary<Faction, HashSet<Faction>> friendlyFactionsByFaction;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        BuildFriendlyRelationsCache();
    }

    private void BuildFriendlyRelationsCache()
    {
        friendlyFactionsByFaction = new Dictionary<Faction, HashSet<Faction>>();

        if (factionRelationsData == null || factionRelationsData.friendlyPairs == null)
            return;

        foreach (var pair in factionRelationsData.friendlyPairs)
        {
            if (pair == null)
                continue;

            if (pair.factionA == pair.factionB)
                continue;

            AddFriendlyRelationOneWay(pair.factionA, pair.factionB);
            AddFriendlyRelationOneWay(pair.factionB, pair.factionA);
        }
    }

    private void AddFriendlyRelationOneWay(Faction sourceFaction, Faction targetFaction)
    {
        if (!friendlyFactionsByFaction.TryGetValue(sourceFaction, out var friendlySet))
        {
            friendlySet = new HashSet<Faction>();
            friendlyFactionsByFaction[sourceFaction] = friendlySet;
        }

        friendlySet.Add(targetFaction);
    }

    public List<Faction> GetFriendlyFactions(Faction faction)
    {
        if (friendlyFactionsByFaction != null &&
            friendlyFactionsByFaction.TryGetValue(faction, out var friendlySet))
        {
            return new List<Faction>(friendlySet);
        }

        return new List<Faction>();
    }

    public bool AreFriendly(Faction firstFaction, Faction secondFaction)
    {
        if (firstFaction == secondFaction)
            return true;

        return friendlyFactionsByFaction != null
               && friendlyFactionsByFaction.TryGetValue(firstFaction, out var friendlySet)
               && friendlySet.Contains(secondFaction);
    }
}