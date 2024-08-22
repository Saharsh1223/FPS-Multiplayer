using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;
using System.Linq;
using TMPro;

public class GameManager : MonoBehaviourPunCallbacks
{
	public GameObject playerPrefab;
	public GameObject messagePrefab;
	public Transform messageContent;

	[Header("Stats (Read Only)")]
	public int playersCount;
	public int totalKills;
	
	private bool gameEnded = false;
	private int endKillPlayerViewID;
	public bool hasCheckedIfGameOver = false;

	private void Start()
	{
		SpawnPlayer();
		Invoke(nameof(UpdatePlayerGameobjectsTags), 1f);
		Invoke(nameof(UpdatePlayerGameobjectsUsernames), 1f);

		playersCount = PhotonNetwork.PlayerList.Length;
		totalKills = 0;
	}
	
	private void Update()
	{
		if (playersCount == 1 && !hasCheckedIfGameOver)
		{
			GameObject playerPrefab = GameObject.FindWithTag("ThisPlayer");
			
			if (endKillPlayerViewID != playerPrefab.GetComponent<PhotonView>().ViewID)
			{
				playerPrefab.GetComponent<PlayerSetup>().Lose();
				hasCheckedIfGameOver = true;
			}
		}
	}

	public void UpdateStats(int newPlayersCount, int newTotalKills)
	{
		if (gameEnded) return;

		playersCount = newPlayersCount;
		totalKills = newTotalKills;
		Debug.Log($"Players alive count: {playersCount} Total kills: {totalKills}");
	}
	
	public void Message(string byPlayerName, string killedPlayerName, int byPlayerViewID, int toPlayerViewID)
	{
		if (gameEnded) return;
		photonView.RPC("RPC_Message", RpcTarget.All, byPlayerName, killedPlayerName, byPlayerViewID, toPlayerViewID);
	}

	[PunRPC]
	public void RPC_Message(string byPlayerName, string killedPlayerName, int byPlayerViewID, int toPlayerViewID)
	{
		if (gameEnded) return;
	
		UpdateStats(playersCount - 1, totalKills + 1);
		
		GameObject byPlayer = PhotonView.Find(byPlayerViewID).gameObject;
		//GameObject killedPlayer = PhotonView.Find(toPlayerViewID).gameObject;
		
		if (byPlayer.GetComponent<PhotonView>().IsMine)
			byPlayer.GetComponent<PlayerSetup>().kills++;
		
		// Only instantiate the message prefab on the local client
		if (PhotonNetwork.IsMessageQueueRunning)
		{
			InstantiateMessage(byPlayerName, killedPlayerName);
		}

		if (playersCount == 1)
		{
			gameEnded = true;
			
			if (byPlayer.GetComponent<PhotonView>().IsMine)
			{
				// Found the local player who killed this player
				Debug.Log("Enabling win panel");
				byPlayer.GetComponent<PlayerSetup>().Win();
				
				GameManager gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
				gameManager.endKillPlayerViewID = byPlayer.GetComponent<PhotonView>().ViewID;
			}
		}
	}

	private void InstantiateMessage(string byPlayerName, string killedPlayerName)
	{
		string message = $"{byPlayerName} killed {killedPlayerName}!";
		CreateMessageObject(message);

		if (playersCount == 1) // Last player
		{
			string message2 = $"Game Over, {byPlayerName} wins!";
			CreateMessageObject(message2);
		}
		else
		{
			string message2 = $"Only {playersCount} Players remain!";
			CreateMessageObject(message2);
		}
	}

	private void CreateMessageObject(string messageText)
	{
		GameObject messageObj = Instantiate(messagePrefab, messageContent);
		messageObj.GetComponent<TMP_Text>().text = messageText;
		messageObj.GetComponent<RectTransform>().localScale = Vector3.one;
	}

	private void SpawnPlayer()
	{
		if (PhotonNetwork.IsConnected)
		{
			// Generate random spawn position within the limits
			Vector3 randomSpawnPosition = new Vector3(
				Random.Range(-30, 30),
				1f, // Fixed y coordinate
				Random.Range(-30, 30)
			);

			// Instantiate player at the random position
			PhotonNetwork.Instantiate(playerPrefab.name, randomSpawnPosition, Quaternion.identity);
		}
		else
		{
			Debug.LogError("Not connected to Photon Network.");
		}
	}

	// Update player gameobjects tags
	private void UpdatePlayerGameobjectsTags()
	{
		List<GameObject> players = new List<GameObject>();
		List<GameObject> allObjects = FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None).ToList();

		foreach (GameObject obj in allObjects)
		{
			if (obj.GetComponent<SpecialPlayerScript>() != null)
			{
				Debug.Log("Adding " + obj.name + " to players list.");
				players.Add(obj);
			}
		}

		foreach (GameObject player in players)
		{
			PhotonView playerView = player.GetComponent<PhotonView>();

			if (playerView.IsMine)
				player.tag = "ThisPlayer";
			else
				player.tag = "OtherPlayer";
		}

		Debug.Log("Updated player gameobjects tags.");
	}

	// Update player gameobjects usernames
	private void UpdatePlayerGameobjectsUsernames()
	{
		GameObject player = GameObject.FindGameObjectWithTag("ThisPlayer");
		GameObject[] otherPlayers = GameObject.FindGameObjectsWithTag("OtherPlayer");

		player.name = PhotonNetwork.LocalPlayer.NickName;
		foreach (GameObject otherPlayer in otherPlayers)
		{
			Debug.Log(otherPlayer.GetComponent<PhotonView>().Owner.NickName);
			otherPlayer.name = otherPlayer.GetComponent<PhotonView>().Owner.NickName;
		}

		Debug.Log("Updated player gameobjects usernames.");
	}

	// public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
	// {
	// 	playersCount++;
	// }

	// public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
	// {
	// 	playersCount--;
	// }
}