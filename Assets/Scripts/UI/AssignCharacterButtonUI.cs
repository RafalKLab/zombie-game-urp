using UnityEngine;
using UnityEngine.UI;

public class AssignCharacterButtonUI : MonoBehaviour
{
    [SerializeField] private Image characterImage;
    [SerializeField] private Button button;

    private PlayableCharacter playableCharacter;

    public void Init(PlayableCharacter playableCharacter, DefenseAssignContext defenseAssignContext)
    {
        button.onClick.RemoveAllListeners();

        if (playableCharacter == null) return;

        this.playableCharacter = playableCharacter;

        characterImage.sprite = playableCharacter.GetCharacterCore().GetCharacterSO().sprite;

        button.onClick.AddListener(() => ButtonPressed(defenseAssignContext));
    }

    private void ButtonPressed(DefenseAssignContext defenseAssignContext)
    {
        defenseAssignContext.baseDefenseController.AssignCharacterToSpot(playableCharacter, defenseAssignContext.defenseSpot);
    }

    public void DestorySelf()
    {
        Destroy(this.gameObject);
    }
}
