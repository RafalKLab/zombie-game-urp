using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DefenseSlotUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI slotName;
    [SerializeField] private TextMeshProUGUI characterName;
    [SerializeField] private Button actionButton;
    [SerializeField] private TextMeshProUGUI actionButtonText;

    private AssignCharacterMenu assignCharacterMenu;

    public void Init(AssignCharacterMenu assignCharacterMenu, DefenseAssignContext defenseAssignContext)
    {
        if (assignCharacterMenu == null) return;

        this.assignCharacterMenu = assignCharacterMenu;

        slotName.text = defenseAssignContext.defenseSpot.anchor.name;

        PlayableCharacter assignedCharacter = defenseAssignContext.defenseSpot.assignedCharacter;
        if (assignedCharacter == null)
        {
            characterName.text = "Empty";
            actionButtonText.text = "Assign";

            actionButton.onClick.RemoveAllListeners();
            actionButton.onClick.AddListener(() => OpenCharacterAssignMenu(defenseAssignContext));
        }
        else
        {
            characterName.text = assignedCharacter.GetCharacterCore().GetCharacterSO().characterName;
            actionButtonText.text = "Free";

            actionButton.onClick.RemoveAllListeners();
            actionButton.onClick.AddListener(() => UnassignCharacterFromSpot(defenseAssignContext));
        }
    }

    public void DestorySelf()
    {
        Destroy(this.gameObject);
    }

    private void OpenCharacterAssignMenu(DefenseAssignContext defenseAssignContext)
    {
        assignCharacterMenu.Show(defenseAssignContext);
    }

    private void UnassignCharacterFromSpot(DefenseAssignContext defenseAssignContext)
    {
        defenseAssignContext.baseDefenseController.UnassignCharacterFromSpot(defenseAssignContext.defenseSpot);
    }
}
