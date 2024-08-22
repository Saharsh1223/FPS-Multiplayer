using UnityEngine;

public class InitialiseAllDroppedWeapons : MonoBehaviour
{
	public void Start()
	{
		foreach (var droppedGunData in FindObjectsByType<DroppedGunData>(FindObjectsInactive.Include, FindObjectsSortMode.None))
		{
			droppedGunData.currentAmmo = droppedGunData.magSize;
			droppedGunData.totalAmmo = droppedGunData.magSize;
			Debug.Log("Initialised: " + droppedGunData.currentAmmo + " / " + droppedGunData.totalAmmo);
		}
	}
}