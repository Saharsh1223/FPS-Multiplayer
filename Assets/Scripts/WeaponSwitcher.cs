
using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;

public class WeaponSwitcher : MonoBehaviour
{
	[Header("References")]
	public Transform[] weapons;
	[SerializeField] private PhotonView photonView;
	[SerializeField] private List<GameObject> weaponPrefabs;
	[SerializeField] private Transform[] weaponParents;
	[SerializeField] private KeyCode dropKey = KeyCode.Q;
	[SerializeField] private KeyCode pickupKey = KeyCode.E;
	[SerializeField] private TMP_Text pickupText;
	[SerializeField] private PlayerSetup playerSetup;
	[Space]
	[SerializeField] private Camera cam;
	[Space]
	[SerializeField] private LayerMask weaponLayer;
	[SerializeField] private Transform orientation;

	[Header("Settings")]
	[SerializeField] private float switchTime;
	[SerializeField] private float dropForce;
	[SerializeField] private float pickupRange;
	
	[HideInInspector] public GameObject selectedWeaponObject;

	public int selectedWeapon;
	private float timeSinceLastSwitch;

	private void Start()
	{
		if (photonView.IsMine)
		{
			//InitializeWeapons();
			// Check if there are any weapons in the list before selecting one
			if (weapons.Length > 0)
			{
				Select(selectedWeapon);
			}
			else
			{
				Debug.Log("No weapons found. Player starts with no weapons.");
			}
			timeSinceLastSwitch = 0f;
		}
	}

	GameObject FindDisabledChildObject(GameObject parent)
	{
		GameObject foundChild = null;
		int disabledCount = 0;

		foreach (Transform child in parent.transform)
		{
			if (!child.gameObject.activeSelf)
			{
				disabledCount++;
				foundChild = child.gameObject;

				// If more than one disabled child is found, return null
				if (disabledCount > 1)
				{
					return null;
				}
			}
		}

		// Return the only disabled child or null if none or multiple were found
		return foundChild;
	}

	private void Update()
	{
		if (!photonView.IsMine) return;
		
		int previousSelectedWeapon = selectedWeapon;
		
		// Setup PickupText
		RaycastHit hit;
		Vector3 raycastOrigin = cam.transform.position;
		Vector3 raycastDirection = cam.transform.forward;

		if (Physics.Raycast(raycastOrigin, raycastDirection, out hit, pickupRange, weaponLayer))
		{
			if (hit.collider.CompareTag("Untagged"))
			{
				pickupText.text = "PICKUP (E)";
			}
		}
		else
		{
			pickupText.text = "";
		}

		// Drop weapon
		if (Input.GetKeyDown(dropKey))
		{
			DropWeapon(selectedWeapon);
			return; // Do not proceed further in this frame
		}

		// Pickup weapon
		if (Input.GetKeyDown(pickupKey))
		{
			PickupWeapon();
			return; // Do not proceed further in this frame
		}

		for (int i = 0; i < weapons.Length; i++)
		{
			if (Input.GetKeyDown(KeyCode.Alpha1 + i) && timeSinceLastSwitch >= switchTime)
			{
				selectedWeapon = i;
				
				if (weapons[selectedWeapon].name == "Grappling Gun")
				{
					GrapplingGun grappleGun = weapons[selectedWeapon].GetComponent<GrapplingGun>();
					
					if (grappleGun.IsGrappling())
					{
						Debug.Log("Cannot change while grappling!");
						break;	
					}
				}
				
				Select(selectedWeapon);
			}
		}

		timeSinceLastSwitch += Time.deltaTime;
	}

	private void Select(int weaponIndex)
	{
		// Ensure valid weapon index
		if (weaponIndex < 0 || weaponIndex >= weapons.Length)
		{
			Debug.Log("Invalid weapon index.");
			return;
		}

		// Disable all weapons:
		for (int i = 0; i < weapons.Length; i++)
		{
			bool isSelected = (i == weaponIndex);
			int viewID = photonView.ViewID;

			if (isSelected)
			{
				// Enable the selected weapon
				playerSetup.EnableWeapon(weapons[i].gameObject, viewID);
			}
			else
			{
				// Disable other weapons
				playerSetup.DisableWeapon(weapons[i].gameObject, viewID);
			}
		}

		// Reset time since last switch
		timeSinceLastSwitch = 0f;
		
		// Notify that a weapon has been selected
		OnWeaponSelected();
	}


	private void DropWeapon(int weaponIndex)
	{	
		if (weapons[weaponIndex].name != "Grappling Gun")
		{
			// Do not drop the weapon if it is reloading
			if (weapons[weaponIndex].GetComponent<Gun>().gunData.isReloading)
			{
				Debug.Log("Cannot drop weapon while reloading");
				return;
			}	
		}
		
		Debug.Log("Dropping weapon...");
		
		string n = weapons[weaponIndex].parent.name;
		string nm = weapons[weaponIndex].name;
		int wIndex = 0;
		
		switch (n)
		{
			case "Weapon1":
				wIndex = 0;
				break;
			case "Weapon2":
				wIndex = 1;
				break;
			case "Weapon3":
				wIndex = 2;
				break;
			case "Weapon4":
				wIndex = 3;
				break;
			case "Weapon5":
				wIndex = 4;
				break;
		}

		Debug.Log("Weapon index: " + wIndex);

		// Instantiate the dropped weapon prefab
		GameObject droppedWeapon = PhotonNetwork.Instantiate(weaponPrefabs[wIndex].name, weapons[weaponIndex].parent.transform.position, Quaternion.identity);
		
		String name = droppedWeapon.name;
		
		//droppedWeapon.name = name.Replace("(Clone)", "");
		//droppedWeapon.tag = "DroppedWeapon";
		playerSetup.ChangeInstantiatedWeaponName(name, "DroppedWeapon");
		
		DroppedGunData droppedGunData = droppedWeapon.GetComponent<DroppedGunData>();

		if (weapons[weaponIndex].name != "Grappling Gun")
		{
			Gun weaponGun = weapons[weaponIndex].GetComponent<Gun>();
			
			// Sync across the network
			droppedGunData.SyncGunData(
				weaponGun.gunData.damage,
				weaponGun.gunData.maxDistance,
				weaponGun.gunData.autoShoot,
				weaponGun.gunData.recoilX,
				weaponGun.gunData.recoilY,
				weaponGun.gunData.recoilZ,
				weaponGun.gunData.snappiness,
				weaponGun.gunData.returnSpeed,
				weaponGun.gunData.shootForce,
				weaponGun.gunData.currentAmmo,
				weaponGun.gunData.totalAmmo,
				weaponGun.gunData.magSize,
				weaponGun.gunData.fireRate,
				weaponGun.gunData.reloadTime
			);
		}

		// Get the Rigidbody component of the dropped weapon
		Rigidbody droppedWeaponRigidbody = droppedWeapon.GetComponent<Rigidbody>();
		if (droppedWeaponRigidbody != null)
		{
			// Apply a force in the forward direction
			droppedWeaponRigidbody.AddForce(orientation.forward * dropForce, ForceMode.Impulse);
		}

		// Disable the selected weapon
		GameObject weapon = weapons[weaponIndex].gameObject;
		int viewID = photonView.ViewID;
		playerSetup.DisableWeapon(weapon, viewID);

		// Remove the weapon from the weapons array
		List<Transform> weaponList = new List<Transform>(weapons);
		weaponList.RemoveAt(weaponIndex);
		weapons = weaponList.ToArray();

		// Reorder the keys
		ReorderKeys();

		// Update the selected weapon index if necessary
		if (selectedWeapon >= weapons.Length)
			selectedWeapon = weapons.Length - 1;
		Select(selectedWeapon);
		
		Debug.Log("Successfully dropped " + nm + "!");
	}
	
	private void PickupWeapon()
	{
		RaycastHit hit;
		Vector3 raycastOrigin = cam.transform.position;
		Vector3 raycastDirection = cam.transform.forward;

		Debug.DrawRay(raycastOrigin, raycastDirection * 10f, Color.blue); // Draw a debug raycast

		if (Physics.Raycast(raycastOrigin, raycastDirection, out hit, pickupRange, weaponLayer))
		{
			if (hit.collider.CompareTag("Untagged"))
			{
				Debug.Log("Picking up weapon...");
				// Re-enable the dropped weapon
				hit.collider.gameObject.SetActive(true);

				// Add the dropped weapon to the weapons array
				string weaponName = hit.collider.transform.parent.parent.name;
				
				Debug.Log(weaponName);
				
				if (weapons.Length > 0)
				{
					if (weaponName == weapons[selectedWeapon].name)
					{
						Debug.Log("Weapon already picked up!");
						return;
					}	
				}
				
				Transform weapon = gameObject.transform;
				
				foreach (Transform weaponParent in weaponParents)
				{
					if (weaponParent.Find(weaponName) != null)
					{
						weapon = weaponParent.Find(weaponName);
						Debug.Log("Weapon Found!");
						break;
					}
				}

				Array.Resize(ref weapons, weapons.Length + 1);
				weapons[weapons.Length - 1] = weapon;
				
				// Select the newly picked up weapon
				selectedWeapon = weapons.Length - 1;
				Select(selectedWeapon);
				
				// Set the gun data
				if (weapon.name != "Grappling Gun")
				{
					Gun weaponGun = weapon.GetComponent<Gun>();
					DroppedGunData droppedGunData = hit.collider.transform.parent.parent.GetComponent<DroppedGunData>();
					
					Debug.Log($"Picking up weapon: {weapon.name}, Damage: {droppedGunData.damage}, Ammo: {droppedGunData.currentAmmo}/{droppedGunData.totalAmmo}");
					
					weaponGun.gunData.damage = droppedGunData.damage;
					weaponGun.gunData.maxDistance = droppedGunData.maxDistance;
					
					weaponGun.gunData.autoShoot = droppedGunData.autoShoot;
					
					weaponGun.gunData.recoilX = droppedGunData.recoilX;
					weaponGun.gunData.recoilY = droppedGunData.recoilY;
					weaponGun.gunData.recoilZ = droppedGunData.recoilZ;
					
					weaponGun.gunData.snappiness = droppedGunData.snappiness;
					weaponGun.gunData.returnSpeed = droppedGunData.returnSpeed;
					
					weaponGun.gunData.shootForce = droppedGunData.shootForce;
					
					weaponGun.gunData.currentAmmo = droppedGunData.currentAmmo;
					weaponGun.gunData.totalAmmo = droppedGunData.totalAmmo;
					weaponGun.gunData.magSize = droppedGunData.magSize;
					
					weaponGun.gunData.fireRate = droppedGunData.fireRate;
					weaponGun.gunData.reloadTime = droppedGunData.reloadTime;
					
					Debug.Log("Weapon properties successfully inherited from dropped weapon.");
					Debug.Log($"After Picking up weapon: {weaponGun.gunData.name}, Damage: {weaponGun.gunData.damage}, Ammo: {weaponGun.gunData.currentAmmo}/{weaponGun.gunData.totalAmmo}");
				}

				// Reorder the keys
				ReorderKeys();
				
				// Delete the prefab of the dropped weapon
				//PhotonNetwork.Destroy(hit.collider.transform.parent.parent.gameObject);
				
				int viewID = hit.collider.transform.parent.parent.gameObject.GetComponent<PhotonView>().ViewID;
				playerSetup.DestroyWeapon(viewID);
				
				Debug.Log("Successfully picked up " + weapon.name + "!");
			}
		}
		else
		{
			Debug.Log("Raycast did not hit anything.");
		}
	}

	private void ReorderKeys()
	{
		for (int i = 0; i < weapons.Length; i++)
		{
			KeyCode key = KeyCode.Alpha1 + i;
			if (i != selectedWeapon)
			{
				if (Input.GetKeyDown(key))
				{
					if (i < selectedWeapon)
						selectedWeapon--; // Adjust selected weapon index if necessary
				}
			}
		}
	}

	private void OnWeaponSelected() {}
}