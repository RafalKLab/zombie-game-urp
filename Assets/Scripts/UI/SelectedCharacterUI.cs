using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class SelectedCharacterUI : MonoBehaviour
{
    [Header("Character Visual")]
    [SerializeField] private Image characterImage;
    [SerializeField] private Image weaponImage;

    [Header("Health")]
    [SerializeField] private Image healbarImage;

    [Header("Text Info")]
    [SerializeField] private TextMeshProUGUI characterName;

    [Header("Ammo Info")]
    [SerializeField] private TextMeshProUGUI ammo;
    [SerializeField] private TextMeshProUGUI totalAmmo;

    [Header("Controls")]
    [SerializeField] private Button closeButton;

    private PlayableCharacter playableCharacter;
    private CharacterCore characterCore;

    private void OnEnable()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(CloseButtonAction);
    }

    private void OnDisable()
    {
        if (closeButton != null)
            closeButton.onClick.RemoveListener(CloseButtonAction);

        UnsubscribeFromCharacterCoreEvents();
    }

    public void Activate(PlayableCharacter playableCharacter)
    {
        this.playableCharacter = playableCharacter;
        characterCore = (this.playableCharacter != null) ? this.playableCharacter.GetCharacterCore() : null;

        SubscribeToCharacterCoreEvents();
        RefreshUIFromCharacter();

        gameObject.SetActive(true);
    }

    public void Deactivate()
    {
        UnsubscribeFromCharacterCoreEvents();

        characterCore = null;
        playableCharacter = null;

        gameObject.SetActive(false);
    }

    public void CloseButtonAction()
    {
        if (playableCharacter == null) return;

        ActiveCharacterManager activeCharacterManager = ActiveCharacterManager.Instance;
        if (activeCharacterManager == null) return;

        activeCharacterManager.UnsetActivePlayableCharacter(playableCharacter);
    }

    private void SubscribeToCharacterCoreEvents()
    {
        if (characterCore == null) return;

        characterCore.OnDamaged += CharacterCore_OnDamaged; ;
    }

    private void UnsubscribeFromCharacterCoreEvents()
    {
        if (characterCore == null) return;

        characterCore.OnDamaged -= CharacterCore_OnDamaged;
    }

    private void RefreshUIFromCharacter()
    {
        if (healbarImage == null) return;
        if (characterCore == null) return;

        healbarImage.fillAmount = characterCore.GetNormalizedHealth();

        CharacterSO characterSO = characterCore.GetCharacterSO();
        if (characterSO != null)
        {
            characterImage.sprite = characterSO.sprite;
            characterName.text = characterSO.characterName;
        }
        else
        {
            characterImage.gameObject.SetActive(false);
            characterName.text = "";
        }

        WeaponTypeSO weaponTypeSO = characterCore.GetWeaponTypeSO();
        if (weaponTypeSO != null)
        {
            weaponImage.sprite = weaponTypeSO.sprite;
            ammo.text = $"{weaponTypeSO.magazineCapacity} / {weaponTypeSO.magazineCapacity}";
            totalAmmo.text = weaponTypeSO.totalAmmo.ToString();
        } else
        {
            weaponImage.gameObject.SetActive(false);
            ammo.text = "";
            totalAmmo.text = "";
        }
    }

    private void CharacterCore_OnDamaged(object sender, CharacterCore.OnDamagedEventArgs e)
    {
        if (healbarImage == null) return;

        healbarImage.fillAmount = e.currentHealthNormalized;
    }
}
