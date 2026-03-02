using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SelectedCharacterUI : MonoBehaviour
{
    // Serialized
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

    // Runtime
    private PlayableCharacter playableCharacter;
    private CharacterCore characterCore;

    // Lifecycle
    private void Awake()
    {
        Debug.Assert(characterImage != null, "CharacterImage not assigned");
        Debug.Assert(weaponImage != null, "WeaponImage not assigned");
        Debug.Assert(healbarImage != null, "HealbarImage not assigned");
        Debug.Assert(characterName != null, "CharacterName not assigned");
        Debug.Assert(ammo != null, "Ammo text not assigned");
        Debug.Assert(totalAmmo != null, "TotalAmmo text not assigned");
        Debug.Assert(closeButton != null, "CloseButton not assigned");
    }

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

    // Public API
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

    // Events
    private void SubscribeToCharacterCoreEvents()
    {
        if (characterCore == null) return;

        characterCore.OnDamaged += CharacterCore_OnDamaged;
        characterCore.OnWeaponChanged += CharacterCore_OnWeaponChanged;
        characterCore.OnAmmoChanged += CharacterCore_OnAmmoChanged;
    }

    private void UnsubscribeFromCharacterCoreEvents()
    {
        if (characterCore == null) return;

        characterCore.OnDamaged -= CharacterCore_OnDamaged;
        characterCore.OnWeaponChanged -= CharacterCore_OnWeaponChanged;
        characterCore.OnAmmoChanged -= CharacterCore_OnAmmoChanged;
    }

    private void CharacterCore_OnDamaged(object sender, CharacterCore.OnDamagedEventArgs e)
    {
        healbarImage.fillAmount = e.currentHealthNormalized;
    }

    private void CharacterCore_OnWeaponChanged()
    {
        UpdateWeaponVisual();
    }

    private void CharacterCore_OnAmmoChanged()
    {
        UpdateAmmoVisual();
    }


    // Visuals
    private void RefreshUIFromCharacter()
    {
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

        UpdateWeaponVisual();
    }

    private void UpdateWeaponVisual()
    {
        if (characterCore == null) return;

        WeaponItemSO weaponItemSO = characterCore.GetWeaponItemSO();
        if (weaponItemSO == null)
        {
            weaponImage.gameObject.SetActive(false);
            ammo.gameObject.SetActive(false);
            totalAmmo.gameObject.SetActive(false);
        }
        else
        {
            weaponImage.sprite = weaponItemSO.icon;
            weaponImage.gameObject.SetActive(true);
            ammo.gameObject.SetActive(true);
            totalAmmo.gameObject.SetActive(true);
            UpdateAmmoVisual();
        }
    }

    private void UpdateAmmoVisual()
    {
        if (characterCore == null) return;

        CharacterWeaponHandler.AmmoInfo ammoInfo = characterCore.GetAmmoInfo();

        ammo.text = $"{ammoInfo.CurrentAmmo} / {ammoInfo.MagazineSize}";
        totalAmmo.text = ammoInfo.TotalAmmo.ToString();
    }
}
