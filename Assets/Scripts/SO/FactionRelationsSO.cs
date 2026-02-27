using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Faction Relations")]
public class FactionRelationsSO : ScriptableObject
{
    [Serializable]
    public class FriendlyFactionPair
    {
        public Faction factionA;
        public Faction factionB;
    }

    public List<FriendlyFactionPair> friendlyPairs = new();
}
