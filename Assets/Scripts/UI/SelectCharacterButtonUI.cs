using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SelectCharacterButtonUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textBlock;
    [SerializeField] private Image backgroundImage;

    private bool selected = false;
    private readonly Color defaultColor = Color.white;
    private readonly Color selectedColor = new Color(0.91f, 0.94f, 1f);

    private PlayableCharacter playableCharacter;

    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(ButtonOnClickAction);
        ActiveCharacterManager.Instance.OnActiveCharacterChanged += ActiveCharacterManager_OnActiveCharacterChanged;

        CharacterDeathManager.Instance.OnCharacterKilled += CharacterDeathManager_OnCharacterKilled;
    }

    private void CharacterDeathManager_OnCharacterKilled(object sender, CharacterDeathManager.CharacterKilledEventArgs e)
    {
        if (playableCharacter == e.playableCharacter)
        {
            Destroy(gameObject);
        }
    }

    private void ActiveCharacterManager_OnActiveCharacterChanged(object sender, ActiveCharacterManager.OnActiveCharacterChangedEventArgs e)
    {
        if (e.playableCharacter != playableCharacter)
        {
            selected = false;
            backgroundImage.color = defaultColor;
        }
    }

    public void SetData(PlayableCharacter playableCharacter)
    {
        this.playableCharacter = playableCharacter;
        textBlock.text = "Placeholder text";
    }

    private void ButtonOnClickAction()
    {
        if (selected) {
            
            ActiveCharacterManager.Instance.UnsetActivePlayableCharacter(playableCharacter);
            selected = false;
            backgroundImage.color = defaultColor;
        }
        else
        {
            ActiveCharacterManager.Instance.SetActivePlayableCharacter(playableCharacter);
            selected = true;
            backgroundImage.color = selectedColor;
        }
    }

    private void OnDestroy()
    {
        if (ActiveCharacterManager.Instance != null)
            ActiveCharacterManager.Instance.OnActiveCharacterChanged -= ActiveCharacterManager_OnActiveCharacterChanged;

        if (CharacterDeathManager.Instance != null)
            CharacterDeathManager.Instance.OnCharacterKilled -= CharacterDeathManager_OnCharacterKilled;
    }
}
