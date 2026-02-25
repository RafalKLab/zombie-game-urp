using UnityEngine;
using System.Collections.Generic;

public class DefenseBaseUI : MonoBehaviour
{
    [SerializeField] private BaseDefenseController baseDefenseController;
    [SerializeField] private Transform defenseSlotContainer;
    [SerializeField] private DefenseSlotUI defenseSlotUIPrefab;
    [SerializeField] private AssignCharacterMenu assignCharacterMenu;

    private List<DefenseSlotUI> cachedSlots = new ();

    private void OnEnable()
    {
        baseDefenseController.onDefenseSpotsChanged += BaseDefenseController_onDefenseSpotsChanged;
        baseDefenseController.onCharacterDefenseStateChanged += BaseDefenseController_onCharacterDefenseStateChanged;

        RefeshSlots();
    }
    private void OnDisable()
    {
        baseDefenseController.onDefenseSpotsChanged -= BaseDefenseController_onDefenseSpotsChanged;
        baseDefenseController.onCharacterDefenseStateChanged -= BaseDefenseController_onCharacterDefenseStateChanged;
    }

    private void BaseDefenseController_onCharacterDefenseStateChanged()
    {
        RefeshSlots();
        assignCharacterMenu.Hide();
    }


    private void BaseDefenseController_onDefenseSpotsChanged()
    {
        RefeshSlots();
    }

    private void RefeshSlots()
    {
        foreach (var slot in cachedSlots)
        {
            slot.DestorySelf();
        }

        cachedSlots = new();

        List<DefenseSpot> defenseSpotsForCharacters = baseDefenseController.GetDefenseSpotsForCharacters();
        foreach (DefenseSpot defenseSpot in defenseSpotsForCharacters)
        {
            DefenseAssignContext defenseAssignContext = new DefenseAssignContext(baseDefenseController, defenseSpot);

            DefenseSlotUI defenseSlotUI = Instantiate(defenseSlotUIPrefab, defenseSlotContainer);
            defenseSlotUI.Init(assignCharacterMenu, defenseAssignContext);
            cachedSlots.Add(defenseSlotUI);
        }
    }

}
