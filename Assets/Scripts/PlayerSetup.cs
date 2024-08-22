using UnityEngine;
using Photon.Pun;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.Rendering.Universal;
using TMPro;

public class PlayerSetup : MonoBehaviourPunCallbacks
{
	[Header("Player Components")]
	public GameObject playerCamera;
	public GameObject playerCanvas;
	public GameObject cameraHolder;
	public GameObject weaponHolder;
	public GameObject winPanel;
	public GameObject losePanel;
	public GameObject canvasObject;
	public TMP_Text killCountText;
	public TMP_Text winKillCountText;
	public TMP_Text loseKillCountText;
	public TMP_Text pingText;
	[HideInInspector] public GameObject weaponObject;

	[Header("Script References")]
	public WeaponSwitcher weaponSwitcher;
	public GrapplingRope grapplingRope;

	[Header("Other")]
	public MeshRenderer playerMesh;
	public Material playerMaterial1;
	public Material playerMaterial2;
	
	[Header("Stats")]
	public int kills = 0;

	// Regular method to find all children recursively, including inactive ones
	public List<GameObject> FindAllChildren(GameObject parent)
	{
		List<GameObject> allChildren = new List<GameObject>();

		void GetChildren(Transform parentTransform)
		{
			foreach (Transform child in parentTransform)
			{
				allChildren.Add(child.gameObject);
				GetChildren(child);
			}
		}

		foreach (Transform child in parent.GetComponentsInChildren<Transform>(true))
		{
			if (child != parent.transform)
			{
				allChildren.Add(child.gameObject);
			}
		}

		return allChildren;
	}

	private void Start()
	{
		if (photonView.IsMine)
		{
			// This is the local player's camera and canvas; keep them active
			EnableLocalPlayerComponents();

			playerMesh.material = playerMaterial1;
			weaponHolder.transform.localPosition = new Vector3(0.5f, -0.7f, 0f);
		}
		else
		{
			// This is not the local player's camera and canvas; disable them
			DisableOtherPlayerComponents();

			playerMesh.material = playerMaterial2;
			cameraHolder.transform.localPosition = new Vector3(0f, 0.7f, 0f);
			weaponHolder.transform.localPosition = new Vector3(0.9f, -0.7f, -0.4f);

			gameObject.layer = LayerMask.NameToLayer("Default");
		}
	}
	
	private void Update()
	{
		killCountText.text = "KILLS: " + kills.ToString();
		pingText.text = "PING: " + PhotonNetwork.GetPing();
	}

	private void EnableLocalPlayerComponents()
	{
		if (playerCamera != null)
			playerCamera.GetComponent<Camera>().enabled = true;

		if (playerCanvas != null)
			playerCanvas.SetActive(true);
	}

	private void DisableOtherPlayerComponents()
	{
		if (playerCamera != null)
		{
			Destroy(playerCamera.GetComponent<UniversalAdditionalCameraData>());
			Destroy(playerCamera.GetComponent<Camera>());
		}

		if (playerCanvas != null)
			playerCanvas.SetActive(false);
	}

	public void OnGunShot(Vector3 hitPoint, Vector3 hitPos, int targetViewID, int viewID, string weaponName, float shootForce, float damage, bool isPlayer)
	{
		photonView.RPC("RPC_OnGunShot", RpcTarget.All, hitPoint, hitPos, targetViewID, viewID, weaponName, shootForce, damage, isPlayer);
	}

	[PunRPC]
	private void RPC_OnGunShot(Vector3 hitPoint, Vector3 hitPos, int targetViewID, int viewID, string weaponName, float shootForce, float damage, bool isPlayer)
	{
		Debug.Log("Shot!");

		GameObject playerObject = PhotonView.Find(viewID).gameObject;

		//Get all the children of the playerObject
		GameObject[] children = FindAllChildren(playerObject).ToArray();

		// Get the weaponObject from children array
		foreach (GameObject child in children)
		{
			if (child.name == weaponName)
			{
				weaponObject = child;
				break;
			}
		}

		GameObject recoilObject = null;

		// Get the weaponObject from children array
		foreach (GameObject child in children)
		{
			if (child.name == "Main Camera")
			{
				recoilObject = child;
				break;
			}
		}

		Animator anim = weaponObject.GetComponent<Animator>();
		AudioSource shootAudio = weaponObject.GetComponent<AudioSource>();
		ParticleSystem muzzleFlash = weaponObject.GetComponentInChildren<ParticleSystem>();
		Recoil recoil = recoilObject.GetComponent<Recoil>();

		anim.CrossFadeInFixedTime(weaponName + "_Shoot", 0f);
		//CrossfadeAnimation(weaponName + "_Shoot", photonView.ViewID, weaponName);

		recoil.RecoilFire();
		muzzleFlash.Play();
		shootAudio.PlayOneShot(shootAudio.clip);

		if (targetViewID != -1)
		{
			PhotonView targetPhotonView = PhotonView.Find(targetViewID);
			if (targetPhotonView != null)
			{
				if (!isPlayer)
				{
					Rigidbody targetRigidbody = targetPhotonView.GetComponent<Rigidbody>();
					targetRigidbody?.AddForce((hitPos - hitPoint).normalized * shootForce, ForceMode.VelocityChange);
				}

				IDamagable targetDamagable = targetPhotonView.GetComponent<IDamagable>();
				string byPlayerName = photonView.Owner.NickName;
				int byPlayerViewID = photonView.ViewID;
				int toPlayerViewID = targetViewID;
				targetDamagable?.Damage(damage, isPlayer, byPlayerName, byPlayerViewID, toPlayerViewID);
			}
		}
	}

	public void CrossfadeAnimation(string animationName, int viewID, string weaponName)
	{
		photonView.RPC("RPC_CrossfadeAnimation", RpcTarget.All, animationName, viewID, weaponName);
	}

	[PunRPC]
	private void RPC_CrossfadeAnimation(string animationName, int viewID, string weaponName)
	{
		GameObject player = PhotonView.Find(viewID).gameObject;

		GameObject gunObject = null;

		GameObject[] children = FindAllChildren(player).ToArray();

		// Get the weaponObject from children array
		foreach (GameObject child in children)
		{
			if (child.name == weaponName)
			{
				gunObject = child;
				break;
			}
		}

		Animator anim = gunObject.GetComponent<Animator>();
		anim.CrossFadeInFixedTime(animationName, 0f);

		Debug.Log("Crossfaded " + animationName + " for " + weaponName);
	}

	public void EnableWeapon(GameObject weapon, int viewID)
	{
		string weaponName = weapon.name;
		photonView.RPC("RPC_Enable", RpcTarget.All, viewID, weaponName);
	}

	public void DisableWeapon(GameObject weapon, int viewID)
	{
		string weaponName = weapon.name;
		photonView.RPC("RPC_Disable", RpcTarget.All, viewID, weaponName);
	}

	public void DestroyWeapon(int viewID)
	{
		Debug.Log("Destroying " + PhotonView.Find(viewID).gameObject.name);
		photonView.RPC("RPC_DestroyWeapon", RpcTarget.All, viewID);
	}

	public void ChangeInstantiatedWeaponName(string name, string tag)
	{
		photonView.RPC("RPC_ChangeInstantiatedWeaponName", RpcTarget.All, name, tag);
	}

	[PunRPC]
	private void RPC_Disable(int viewID, string weaponName)
	{

		GameObject playerObject = PhotonView.Find(viewID).gameObject;
		//Get all the children of the playerObject
		GameObject[] children = FindAllChildren(playerObject).ToArray();

		// Get the weaponObject from children array
		foreach (GameObject child in children)
		{
			if (child.name == weaponName)
			{
				weaponObject = child;
				break;
			}
		}

		// Find weaponObject within the playerObject and disable the weaponObject within playerObject
		weaponObject.gameObject.SetActive(false);

		Debug.Log("Disabled " + weaponObject.name);
	}

	[PunRPC]
	private void RPC_Enable(int viewID, string weaponName)
	{
		//weaponObject.SetActive(true);
		GameObject playerObject = PhotonView.Find(viewID).gameObject;
		//Get all the children of the playerObject
		GameObject[] children = FindAllChildren(playerObject).ToArray();

		// Get the weaponObject from children array
		foreach (GameObject child in children)
		{
			if (child.name == weaponName)
			{
				weaponObject = child;
				break;
			}
		}

		// Find weaponObject within the playerObject and enable the weaponObject within playerObject
		weaponObject.gameObject.SetActive(true);

		Debug.Log("Enabled " + weaponObject.name);
	}

	[PunRPC]
	private void RPC_DestroyWeapon(int viewID)
	{
		PhotonView view = PhotonView.Find(viewID);
		if (view != null)
		{
			Destroy(view.gameObject);
			Debug.Log("Destroyed " + view.name);
		}
	}

	[PunRPC]
	private void RPC_ChangeInstantiatedWeaponName(string name, string tag)
	{
		GameObject instantiatedWeapon = GameObject.Find(name);

		if (instantiatedWeapon != null)
		{
			instantiatedWeapon.name = name.Replace("(Clone)", "");
		}
		else
		{
			Debug.Log("Could not find " + name + "(Clone)");
		}

		instantiatedWeapon.tag = tag;

		Debug.Log("Changed name of " + name.Replace("(Clone)", ""));
	}

	[PunRPC]
	public void SyncRopePositions(Vector3[] positions)
	{
		if (grapplingRope != null)
		{
			grapplingRope.UpdateRopePositions(positions);
		}
	}

	[PunRPC]
	public void SetRopeVisibility(bool isVisible)
	{
		if (grapplingRope != null)
		{
			grapplingRope.SetRopeVisibility(isVisible);
		}
	}

	public void SendRopePositions(Vector3[] positions)
	{
		photonView.RPC("SyncRopePositions", RpcTarget.Others, positions);
	}

	public void SetRopeVisibilityForAll(bool isVisible)
	{
		photonView.RPC("SetRopeVisibility", RpcTarget.All, isVisible);
	}

	public void DestroyTarget(int viewID) =>
		photonView.RPC(nameof(RPC_DestroyTarget), RpcTarget.AllBuffered, viewID);

	public void KillPlayer(int viewID) =>
		photonView.RPC(nameof(RPC_KillPlayer), RpcTarget.OthersBuffered, viewID);


	public void Message(string byPlayerName, string killedPlayerName, int byPlayerViewID, int toPlayerViewID)
	{
		if (photonView.IsMine) // Only the local player can send messages
		{
			GameManager gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
			gameManager.Message(byPlayerName, killedPlayerName, byPlayerViewID, toPlayerViewID);
		}
	}
	
	[PunRPC]
	private void RPC_DestroyTarget(int viewID)
	{
		Destroy(PhotonView.Find(viewID).gameObject);
	}

	[PunRPC]
	private void RPC_KillPlayer(int viewID)
	{
		PhotonView.Find(viewID).gameObject.SetActive(false);
	}
	
	
	public void Win()
	{
		winPanel.SetActive(true);
		winKillCountText.text = kills > 1 ? kills + " KILLS" : kills + " KILL";
		
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
	}
	
	public void Lose()
	{
		Debug.Log($"Lost {gameObject.name} the game!");
		playerCanvas.SetActive(true);
		playerCanvas.GetComponent<Canvas>().enabled = true;
		losePanel.SetActive(true);
		canvasObject.transform.Find("Sway").gameObject.SetActive(false);
		loseKillCountText.text = kills != 1 ? kills + " KILLS" : kills + " KILL";
	}
}