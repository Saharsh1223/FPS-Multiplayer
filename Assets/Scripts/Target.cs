using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class Target : MonoBehaviourPunCallbacks, IPunObservable, IDamagable
{
	public float health = 100f;
	public float maxHealth = 100f; // Maximum health value
	public Slider healthBar;
	public GameObject deathPanel;
	public GameObject swayPanel;
	public GameObject holdableObjects;
	public PlayerSetup playerSetup;

	public float regenSpeed = 5f; // Health points regenerated per second

	private float targetHealth; // Target value for health
	[HideInInspector] public bool isDead = false;

	void Start()
	{
		if (!photonView.IsMine)
		{
			// Disable the script for non-local players
			this.enabled = false;
			return;
		}
		
		// Initialize targetHealth with the current health
		targetHealth = health;
	}

	void Update()
	{
		// Smoothly update the health bar value over time
		if (healthBar != null)
		{
			healthBar.value = Mathf.Lerp(healthBar.value, targetHealth, Time.deltaTime * 10f);
		}

		// Linearly regenerate health over time
		if (health < maxHealth)
		{
			health += regenSpeed * Time.deltaTime;  // Linear regeneration
			health = Mathf.Clamp(health, 0, maxHealth); // Ensure health doesn't exceed maxHealth
			targetHealth = health;
		}
	}

	public void Damage(float damage, bool isPlayer, string byPlayerName, int byPlayerViewID, int toPlayerViewID)
	{
		if (!photonView.IsMine)
		{
			return;
		}

		health -= damage;
		targetHealth = health;

		if (health <= 0 && isPlayer)
		{
			string killedPlayerName = photonView.Owner.NickName;
			
			playerSetup.KillPlayer(photonView.ViewID);
			playerSetup.Message(byPlayerName, killedPlayerName, byPlayerViewID, toPlayerViewID);
			
			OnDeath();
		}
	}

	private void OnDeath()
	{
		isDead = true;

		if (deathPanel != null)
		{
			GameManager gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
			if (gameManager.playersCount > 1)
				deathPanel.SetActive(true);
			else
				playerSetup.Lose();
		}
		
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
	}

	public void LeaveGame()
	{
		PhotonNetwork.LeaveRoom();
		SceneManager.LoadScene("Lobby");
	}

	public void SpectateGame()
	{
		gameObject.GetComponent<Rigidbody>().useGravity = false;
		gameObject.GetComponent<CapsuleCollider>().enabled = false;
		transform.Find("Orientation").Find("Mesh").GetComponent<MeshRenderer>().enabled = false;
		deathPanel.SetActive(false);
		//deathPanel.transform.parent.gameObject.SetActive(false);
		swayPanel.SetActive(false);
		holdableObjects.SetActive(false);
		gameObject.GetComponent<PlayerMovement>().isDead = true;
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
	}

	// Photon PUN 2 synchronization
	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.IsWriting)
		{
			// Send health value to other players
			stream.SendNext(health);
		}
		else
		{
			// Receive health value from other players
			health = (float)stream.ReceiveNext();
			targetHealth = health; // Update target health for smooth animation
		}
	}
}