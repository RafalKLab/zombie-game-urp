using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static CharacterWeaponHandler;

public class EquippedInventoryUI : MonoBehaviour
{
    [SerializeField] private Image weaponImage;
    [SerializeField] private TextMeshProUGUI weaponName;
    [SerializeField] private TextMeshProUGUI weaponAmmo;

    public void Init(WeaponTypeSO weaponTypeSO, AmmoInfo ammoInfo)
    {
        if (weaponImage == null) return;
        if (weaponName == null) return;
        if (weaponAmmo == null) return;

        if (weaponTypeSO == null)
        {
            weaponImage.gameObject.SetActive(false);
            weaponName.gameObject.SetActive(false);
            weaponAmmo.gameObject.SetActive(false);

            return;
        }

        weaponImage.gameObject.SetActive(true);
        weaponName.gameObject.SetActive(true);
        weaponAmmo.gameObject.SetActive(true);

        weaponImage.sprite = weaponTypeSO.sprite;
        weaponName.text = weaponTypeSO.weaponName;
        weaponAmmo.text = $"{ammoInfo.CurrentAmmo} / {ammoInfo.MagazineSize}";
    }
}
