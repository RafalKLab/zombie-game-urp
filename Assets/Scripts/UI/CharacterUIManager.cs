using UnityEngine;

public class CharacterUIManager : MonoBehaviour
{
    [SerializeField] private SelectedCharacterUI selectedCharacterUI;

    private void OnEnable()
    {
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
            selectedCharacterUI.Deactivate();
        } else
        {
            selectedCharacterUI.Activate(e.playableCharacter);
        }
    }
}
