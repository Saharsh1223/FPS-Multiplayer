using UnityEngine;
using Photon.Pun;

public class DroppedGunData : MonoBehaviour//, IPunObservable
{
	[Header("Info")]
	public new string name;
	
	[Space]
	
	public float damage;
	public float maxDistance;
	
	[Header("Shooting")]
	public bool autoShoot;
	
	[Header("Recoil")]
	public float recoilX;
	public float recoilY;
	public float recoilZ;
	
	public float snappiness;
	public float returnSpeed;
	
	[Range(0f, 8f)]
	public float shootForce;
	
	[Header("Reloading")]
	public int currentAmmo;
	public int totalAmmo;
	public int magSize;
	
	public float fireRate;
	public float reloadTime;
	
	[HideInInspector]
	public bool isReloading;

	public PhotonView photonView;

	// Method to sync the gun data
	public void SyncGunData(float damage, float maxDistance, bool autoShoot, float recoilX, float recoilY, float recoilZ, float snappiness, float returnSpeed, float shootForce, int currentAmmo, int totalAmmo, int magSize, float fireRate, float reloadTime)
	{
		// Set local variables
		this.damage = damage;
		this.maxDistance = maxDistance;
		this.autoShoot = autoShoot;
		this.recoilX = recoilX;
		this.recoilY = recoilY;
		this.recoilZ = recoilZ;
		this.snappiness = snappiness;
		this.returnSpeed = returnSpeed;
		this.shootForce = shootForce;
		this.currentAmmo = currentAmmo;
		this.totalAmmo = totalAmmo;
		this.magSize = magSize;
		this.fireRate = fireRate;
		this.reloadTime = reloadTime;

		// Sync across network
		photonView.RPC("RPC_SyncGunData", RpcTarget.All, damage, maxDistance, autoShoot, recoilX, recoilY, recoilZ, snappiness, returnSpeed, shootForce, currentAmmo, totalAmmo, magSize, fireRate, reloadTime);
	}

	[PunRPC]
	private void RPC_SyncGunData(float damage, float maxDistance, bool autoShoot, float recoilX, float recoilY, float recoilZ, float snappiness, float returnSpeed, float shootForce, int currentAmmo, int totalAmmo, int magSize, float fireRate, float reloadTime)
	{
		this.damage = damage;
		this.maxDistance = maxDistance;
		this.autoShoot = autoShoot;
		this.recoilX = recoilX;
		this.recoilY = recoilY;
		this.recoilZ = recoilZ;
		this.snappiness = snappiness;
		this.returnSpeed = returnSpeed;
		this.shootForce = shootForce;
		this.currentAmmo = currentAmmo;
		this.totalAmmo = totalAmmo;
		this.magSize = magSize;
		this.fireRate = fireRate;
		this.reloadTime = reloadTime;
	}

	// //Use IPunObservable
	// public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	// {
	// 	if (stream.IsWriting)
	// 	{
	// 		stream.SendNext(damage);
	// 		stream.SendNext(maxDistance);
	// 		stream.SendNext(autoShoot);
	// 		stream.SendNext(recoilX);
	// 		stream.SendNext(recoilY);
	// 		stream.SendNext(recoilZ);
	// 		stream.SendNext(snappiness);
	// 		stream.SendNext(returnSpeed);
	// 		stream.SendNext(shootForce);
	// 		stream.SendNext(currentAmmo);
	// 		stream.SendNext(totalAmmo);
	// 		stream.SendNext(magSize);
	// 		stream.SendNext(fireRate);
	// 		stream.SendNext(reloadTime);
	// 	}
	// 	else
	// 	{
	// 		damage = (float)stream.ReceiveNext();
	// 		maxDistance = (float)stream.ReceiveNext();
	// 		autoShoot = (bool)stream.ReceiveNext();
	// 		recoilX = (float)stream.ReceiveNext();
	// 		recoilY = (float)stream.ReceiveNext();
	// 		recoilZ = (float)stream.ReceiveNext();
	// 		snappiness = (float)stream.ReceiveNext();
	// 		returnSpeed = (float)stream.ReceiveNext();
	// 		shootForce = (float)stream.ReceiveNext();
	// 		currentAmmo = (int)stream.ReceiveNext();
	// 		totalAmmo = (int)stream.ReceiveNext();
	// 		magSize = (int)stream.ReceiveNext();
	// 		fireRate = (float)stream.ReceiveNext();
	// 		reloadTime = (float)stream.ReceiveNext();
	// 	}
	// }
}
