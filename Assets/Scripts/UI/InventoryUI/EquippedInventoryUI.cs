using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EquippedInventoryUI : MonoBehaviour
{
    [SerializeField] private Image weaponImage;
    [SerializeField] private TextMeshProUGUI weaponName;

    public void Init(WeaponTypeSO weaponTypeSO)
    {
        if (weaponImage == null) return;
        if (weaponName == null) return;

        if (weaponTypeSO == null)
        {
            weaponImage.gameObject.SetActive(false);
            weaponName.gameObject.SetActive(false);

            return;
        }

        weaponImage.gameObject.SetActive(true);
        weaponImage.sprite = weaponTypeSO.sprite;
        weaponName.text = weaponTypeSO.weaponName;
    }
}
