using UnityEngine;
using Photon.Pun;
using TMPro;

public class PlayerName : MonoBehaviour
{
	public PhotonView photonView;
	public TMP_Text playerNameText;
	
	private Camera cam;
	
	private void Start()
	{
		Invoke("UpdateUsername", 2f);
	}
	
	private void UpdateUsername()
	{
		if (photonView.IsMine)
			gameObject.SetActive(false);
		else
			playerNameText.text = photonView.Owner.NickName;
	}
	
	private void Update()
	{
		if (cam == null)
			cam = FindFirstObjectByType<Camera>();
		
		if (cam == null)
			return;
			
		transform.LookAt(cam.transform);
	}
}