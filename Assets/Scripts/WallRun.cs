using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class WallRun : MonoBehaviourPun, IPunObservable
{
	[Header("Rigidbody")]
	[SerializeField] private Rigidbody rb;

	[Header("Movement")]
	[SerializeField] private Transform orientation;

	[Header("Detection")]
	[SerializeField] private float wallDistance = .5f;
	[SerializeField] private float minimumJumpHeight = 1.5f;

	[Header("Wall Running")]
	[SerializeField] private float wallRunGravity;
	[SerializeField] private float wallRunJumpForce;

	[Header("Layer Mask")]
	[SerializeField] private LayerMask whatIsWallRunnable;

	[Header("Camera")]
	[SerializeField] private Camera cam;
	[SerializeField] private float fov;
	[SerializeField] private float wallRunfov;
	[SerializeField] private float wallRunfovTime;
	[SerializeField] private float camTilt;
	[SerializeField] private float camTiltTime;

	public float tilt { get; private set; }

	private bool wallLeft = false;
	private bool wallRight = false;

	RaycastHit leftWallHit;
	RaycastHit rightWallHit;

	private bool isWallRunning = false;
	[HideInInspector] public bool isDead;

	bool CanWallRun()
	{
		return !Physics.Raycast(transform.position, Vector3.down, minimumJumpHeight) && !isDead;
	}

	void CheckWall()
	{
		wallLeft = Physics.Raycast(transform.position, -orientation.right, out leftWallHit, wallDistance, whatIsWallRunnable);
		wallRight = Physics.Raycast(transform.position, orientation.right, out rightWallHit, wallDistance, whatIsWallRunnable);
	}

	private void Update()
	{
		if (!photonView.IsMine) return; // Apply wall running only if this PhotonView belongs to the local player

		CheckWall();

		if (CanWallRun())
		{
			if (wallLeft || wallRight)
			{
				StartWallRun();
				isWallRunning = true;
			}
			else
			{
				StopWallRun();
				isWallRunning = false;
			}
		}
		else
		{
			StopWallRun();
			isWallRunning = false;
		}
	}

	void StartWallRun()
	{
		rb.useGravity = false;

		rb.AddForce(Vector3.down * wallRunGravity, ForceMode.Force);

		cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, wallRunfov, wallRunfovTime * Time.deltaTime);

		if (wallLeft)
			tilt = Mathf.Lerp(tilt, -camTilt, camTiltTime * Time.deltaTime);
		else if (wallRight)
			tilt = Mathf.Lerp(tilt, camTilt, camTiltTime * Time.deltaTime);

		if (Input.GetKeyDown(KeyCode.Space))
		{
			if (wallLeft)
			{
				Vector3 wallRunJumpDirection = transform.up + leftWallHit.normal;
				rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
				rb.AddForce(wallRunJumpDirection * wallRunJumpForce * 100, ForceMode.Force);
			}
			else if (wallRight)
			{
				Vector3 wallRunJumpDirection = transform.up + rightWallHit.normal;
				rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z); 
				rb.AddForce(wallRunJumpDirection * wallRunJumpForce * 100, ForceMode.Force);
			}
		}
	}

	void StopWallRun()
	{
		bool isDead = GetComponent<Target>().isDead;
		if (!isDead)
			rb.useGravity = true;
		else
			rb.useGravity = false;

		cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, fov, wallRunfovTime * Time.deltaTime);
		tilt = Mathf.Lerp(tilt, 0, camTiltTime * Time.deltaTime);
	}

	// Photon PUN 2 synchronization
	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.IsWriting)
		{
			// Send wall running state and tilt to other players
			stream.SendNext(isWallRunning);
			stream.SendNext(tilt);
		}
		else
		{
			// Receive wall running state and tilt from other players
			isWallRunning = (bool)stream.ReceiveNext();
			tilt = (float)stream.ReceiveNext();
		}
	}
}
