using System;
using System.Collections.Generic;
using UnityEngine;

public class BaseDefenseController : MonoBehaviour
{
    public event Action onDefenseSpotsChanged;
    public event Action onCharacterDefenseStateChanged;
    private void NotifyDefenseSpotsChanged() => onDefenseSpotsChanged?.Invoke();
    private void NotifyCharacterDefenseStateChanged() => onCharacterDefenseStateChanged?.Invoke();

    [SerializeField] private List<DefenseSpot> defenseSpotsForCharacters;

    private void Start()
    {
        InitDefenseSpots();
    }

    private void InitDefenseSpots()
    {
        foreach (var spot in defenseSpotsForCharacters)
        {
            spot.assignedCharacter = null;
        }

        NotifyDefenseSpotsChanged();
    }

    public List<DefenseSpot> GetDefenseSpotsForCharacters()
    {
        return defenseSpotsForCharacters;
    }

    public void AssignCharacterToSpot(PlayableCharacter playableCharacter, DefenseSpot defenseSpot)
    {
        playableCharacter.GetCharacterAi().CommandDefendPoint(defenseSpot.anchor);
        defenseSpot.assignedCharacter = playableCharacter;
        NotifyCharacterDefenseStateChanged();
    }

    public void UnassignCharacterFromSpot(DefenseSpot defenseSpot)
    {
        PlayableCharacter playableCharacter = defenseSpot.assignedCharacter;
        playableCharacter.GetCharacterAi().CommandIdle();
        defenseSpot.assignedCharacter = null;
        NotifyCharacterDefenseStateChanged();
    }
}

[System.Serializable]
public class DefenseSpot
{
    public Transform anchor;

    [System.NonSerialized]
    public PlayableCharacter assignedCharacter;
}

public class DefenseAssignContext
{
    public BaseDefenseController baseDefenseController;
    public DefenseSpot defenseSpot;

    public DefenseAssignContext(BaseDefenseController baseDefenseController, DefenseSpot defenseSpot)
    {
        this.baseDefenseController = baseDefenseController;
        this.defenseSpot = defenseSpot;
    }
}