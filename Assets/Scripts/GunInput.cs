using System;
using UnityEngine;
using Photon.Pun;

public class GunInput : MonoBehaviourPunCallbacks
{
    public static Action shootInput;
    public static Action reloadInput;

    [SerializeField] private Transform weaponObject;
    [SerializeField] private KeyCode reloadKey = KeyCode.R;

    private void Update()
    {
        Gun activeGun = weaponObject.gameObject?.GetComponentInChildren<Gun>(false);

        if (activeGun != null)
        {
            if (activeGun.gunData.autoShoot)
            {
                if (Input.GetMouseButton(0) && !activeGun.gunData.isReloading)
                    shootInput?.Invoke();
            }
            else
            {
                if (Input.GetMouseButtonDown(0) && !activeGun.gunData.isReloading)
                    shootInput?.Invoke();
            }

            if (Input.GetKeyDown(reloadKey) || activeGun.gunData.currentAmmo <= 0)
                reloadInput?.Invoke();
        }
    }
}
