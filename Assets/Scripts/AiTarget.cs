using UnityEngine;

public class AiTarget : MonoBehaviour
{
    [SerializeField] private Faction faction;
    [SerializeField] private Transform aimPoint;

    public Faction GetFaction()
    {
        return faction;
    }

    public Transform GetAimPoint()
    {
        return aimPoint ?? transform;
    }
}

public enum Faction
{
    Player,
    Zombie,
    Hostile,
}

