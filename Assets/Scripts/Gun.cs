using System.Collections;
using TMPro;
using UnityEngine;
using Photon.Pun;

public class Gun : MonoBehaviour
{
	[Header("References")]
	public GunData gunData;
	public PhotonView photonView;
	public PlayerSetup playerSetup;
	public Transform tip;
	[SerializeField] private Transform cam;
	[SerializeField] private Recoil recoil;
	[SerializeField] private ParticleSystem muzzleFlash;
	[SerializeField] private AudioSource shootAudio;
	[SerializeField] private AudioSource reloadAudio;
	[SerializeField] private TMP_Text gunText;

	[Header("Animation")]
	[SerializeField] private Animator anim;

	private float timeSinceLastShot;

	private void Start()
	{
		GunInput.shootInput += Shoot;
		GunInput.reloadInput += StartReload;
	}

	private bool CanShoot() => !gunData.isReloading && timeSinceLastShot > 1f / (gunData.fireRate / 60);

	private void OnDisable() => gunData.isReloading = false;

	private void StartReload()
	{
		if (!gunData.isReloading && this.gameObject.activeSelf)
		{
			StartCoroutine(Reload());
		}
	}

	private IEnumerator Reload()
	{
		// Calculate how much ammo is needed to fill the magazine
		int missingAmmo = gunData.magSize - gunData.currentAmmo;

		// If there's no missing ammo, we can't reload
		if (missingAmmo <= 0 || gunData.totalAmmo == 0)
		{
			Debug.Log("Can't reload right now.");
			yield break;
		}

		gunData.isReloading = true;
		// anim.CrossFadeInFixedTime(gunData.name + "_Reload", 0f);
		playerSetup.CrossfadeAnimation(gunData.name + "_Reload", photonView.ViewID, gunData.name);
		reloadAudio.Play();

		Debug.Log("Reloading");

		// Wait for the reload time to finish
		yield return new WaitForSeconds(gunData.reloadTime);

		// Assuming you have a total ammo pool from which to draw, we refill the magazine
		if (gunData.totalAmmo >= missingAmmo)
		{
			// Full reload possible
			gunData.currentAmmo += missingAmmo;
			gunData.totalAmmo -= missingAmmo;
		}
		else
		{
			// Partial reload if not enough total ammo
			gunData.currentAmmo += gunData.totalAmmo;
			gunData.totalAmmo = 0;
		}

		gunData.isReloading = false;
	}

	private void Shoot()
	{
		if (!photonView.IsMine) return; // Ensure only the owner can shoot

		if (gunData.currentAmmo > 0)
		{
			if (CanShoot())
			{
				if (Physics.Raycast(tip.position, cam.forward, out RaycastHit hitInfo, gunData.maxDistance))
				{
					gunData.currentAmmo--;
					timeSinceLastShot = 0;
					
					Vector3 hitPos = hitInfo.collider.gameObject.transform.position;
					Vector3 hitPoint = hitInfo.point;
					
					bool isPlayer = false;
					
					int targetViewID =-1;
					if (hitInfo.collider.transform.tag == "Object" && !(hitInfo.collider.transform.tag == "OtherPlayer")) // It is an Object
					{
						targetViewID = hitInfo.collider.gameObject.GetComponent<PhotonView>().ViewID;	
					}
					else if (hitInfo.collider.transform.GetComponent<PhotonView>() != null) // It is a Player
					{
						targetViewID = hitInfo.collider.gameObject.GetComponent<PhotonView>().ViewID;
						isPlayer = true;
					}

					int viewID = photonView.ViewID;
					string weaponName = gunData.name;
					float shootForce = gunData.shootForce;

					// Call the RPC to sync shooting
					playerSetup.OnGunShot(hitPoint, hitPos, targetViewID, viewID, weaponName, shootForce, gunData.damage, isPlayer);
				}
				else
				{
					Debug.Log("No hit");
				}
			}
		}
	}

	private void Update()
	{
		timeSinceLastShot += Time.deltaTime;

		recoil.recoilX = gunData.recoilX;
		recoil.recoilY = gunData.recoilY;
		recoil.recoilZ = gunData.recoilZ;
		recoil.snappiness = gunData.snappiness;
		recoil.returnSpeed = gunData.returnSpeed;

		gunText.text = gunData.currentAmmo + "/" + gunData.magSize + " (" + gunData.totalAmmo + ")";
	}
}
