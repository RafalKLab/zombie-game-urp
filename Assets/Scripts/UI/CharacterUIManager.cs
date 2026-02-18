using UnityEngine;

public class CharacterUIManager : MonoBehaviour
{
    [SerializeField] private SelectCharacterUI selectCharacter;
    [SerializeField] private SelectedCharacterUI selectedCharacterUI;

    private void OnEnable()
    {
        selectCharacter.Activate();
        selectedCharacterUI.Deactivate();

        if (ActiveCharacterManager.Instance != null)
            ActiveCharacterManager.Instance.OnActiveCharacterChanged += ActiveCharacterManager_OnActiveCharacterChanged;
        else
            Debug.LogError("[CharacterUIManager] ActiveCharacterManager.Instance is NULL in OnEnable", this);
    }

    private void OnDisable()
    {
        if (ActiveCharacterManager.Instance != null)
            ActiveCharacterManager.Instance.OnActiveCharacterChanged -= ActiveCharacterManager_OnActiveCharacterChanged;
    }

    private void ActiveCharacterManager_OnActiveCharacterChanged(object sender, ActiveCharacterManager.OnActiveCharacterChangedEventArgs e)
    {
        if (e.playableCharacter == null)
        {
            selectCharacter.Activate();
            selectedCharacterUI.Deactivate();
        } else
        {
            selectCharacter.Deactivate();
            selectedCharacterUI.Activate(e.playableCharacter);
        }
    }
}
