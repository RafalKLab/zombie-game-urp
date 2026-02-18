using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SelectCharacterButtonUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textBlock;

    private PlayableCharacter playableCharacter;
    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    private void OnEnable()
    {
        if (button != null)
            button.onClick.AddListener(OnButtonClicked);
    }

    private void OnDisable()
    {
        if (button != null)
            button.onClick.RemoveListener(OnButtonClicked);
    }

    public void SetData(PlayableCharacter playableCharacter)
    {
        this.playableCharacter = playableCharacter;
        textBlock.text = playableCharacter.GetCharacterCore().GetCharacterSO().characterName;
    }

    private void OnButtonClicked()
    {
        if (playableCharacter == null) return;

        ActiveCharacterManager activeCharacterManager = ActiveCharacterManager.Instance;
        if (activeCharacterManager == null) return;

        activeCharacterManager.SetActivePlayableCharacter(playableCharacter);
    }
}
