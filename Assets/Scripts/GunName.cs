using TMPro;
using UnityEngine;

public class GunName : MonoBehaviour
{
    [SerializeField] private WeaponSwitcher weaponSwitcher;
    [SerializeField] private TMP_Text gunText;

    private void Update()
    {
        if (weaponSwitcher.weapons.Length <= 0)
        {
            gunText.text = "";
            return;
        }

        // Update the gun name locally based on the selected weapon
        UpdateGunName(weaponSwitcher.weapons[weaponSwitcher.selectedWeapon].name);
    }

    private void UpdateGunName(string gunName)
    {
        gunText.text = gunName;
    }
}
