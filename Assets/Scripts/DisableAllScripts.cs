using UnityEngine;
using Photon.Pun;

public class DisableAllScripts : MonoBehaviourPunCallbacks
{
	public GameObject[] audioSources;
	public SpecialPlayerScript specialPlayerScript;
	public PlayerName playerName;
	public CapsuleCollider capsuleCollider;
	public Target target;

	private void Start()
	{
		if (!photonView.IsMine)
		{
			DisableScripts();
			
			// Disable all audio sources
			foreach (GameObject audioSource in audioSources)
			{
				audioSource.SetActive(false);
			}
		}
	}

	private void DisableScripts()
	{
		// Get all MonoBehaviour components in the GameObject and its children
		MonoBehaviour[] allScripts = GetComponentsInChildren<MonoBehaviour>(true);

		foreach (MonoBehaviour script in allScripts)
		{
			// Ignore scripts belonging to specific namespaces
			if (script.GetType().Namespace == "Photon.Pun" || 
				script.GetType().Namespace == "TMPro" || 
				script.GetType().Namespace == "UnityEngine")
			{
				script.enabled = true;
			}
			else
			{
				// Disable all other scripts
				script.enabled = false;
			}
		}

		// Enable LineRenderer components
		LineRenderer[] lineRenderers = GetComponentsInChildren<LineRenderer>(true);
		foreach (LineRenderer lineRenderer in lineRenderers)
		{
			lineRenderer.enabled = true;
		}
		
		// Enable Spring components
		Spring[] springs = GetComponentsInChildren<Spring>(true);
		foreach (Spring spring in springs)
		{
			spring.enabled = true;
		}

		// Enable GrapplingRope components
		GrapplingRope[] grapplingRopes = GetComponentsInChildren<GrapplingRope>(true);
		foreach (GrapplingRope grapplingRope in grapplingRopes)
		{
			grapplingRope.enabled = true;
		}
		
		// Enable specific scripts
		specialPlayerScript.enabled = true;
		playerName.enabled = true;
		capsuleCollider.enabled = true;
		//target.enabled = true;
	}
}
