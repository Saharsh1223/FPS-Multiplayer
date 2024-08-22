using System;
using UnityEngine;
using Photon.Pun;

public class PlayerMovement : MonoBehaviourPun, IPunObservable
{
	[Header("Movement")]
	[SerializeField] private float moveSpeed = 6f;
	public float airMultiplier = 0.2f;
	private float movementMultiplier = 10f;

	[Header("Sprinting")]
	[SerializeField] private float walkSpeed = 4f;
	[SerializeField] private float sprintSpeed = 6f;
	[SerializeField] private float acceleration = 10f;

	[Header("Jumping")]
	public float jumpForce = 5f;

	[Header("Keybinds")]
	[SerializeField] private KeyCode jumpKey = KeyCode.Space;
	[SerializeField] private KeyCode sprintKey = KeyCode.LeftShift;

	[Header("Drag")]
	[SerializeField] private float groundDrag = 6f;
	[SerializeField] private float airDrag = 2f;

	[SerializeField] private Transform orientation;

	private float horizontalMovement;
	private float verticalMovement;

	[Header("Ground Detection")]
	[SerializeField] private Transform groundCheck;
	[SerializeField] private LayerMask groundMask;
	[SerializeField] private float groundDistance = 0.2f;
	public bool isGrounded { get; private set; }

	private Vector3 moveDirection;
	private Vector3 slopeMoveDirection;

	private Rigidbody rb;

	private RaycastHit slopeHit;

	private float playerHeight = 2f;

	[HideInInspector] public bool isMoving;
	[HideInInspector] public bool isDead;

	private bool OnSlope()
	{
		if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight / 2 + 0.5f))
		{
			return slopeHit.normal != Vector3.up;
		}
		return false;
	}

	private void Start()
	{
		rb = GetComponent<Rigidbody>();
		rb.freezeRotation = true;
	}

	private void Update()
	{
		isDead = GetComponent<Target>().isDead;
		
		if (Physics.CheckSphere(groundCheck.position, groundDistance, groundMask))
		{
			if (isDead)
				isGrounded = false;
			else
				isGrounded = true;
		}
		else
		{
			isGrounded = false;
		}
		isMoving = rb.linearVelocity.magnitude > 0.1f;

		MyInput();
		ControlDrag();
		ControlSpeed();

		if (Input.GetKeyDown(jumpKey) && isGrounded && !isDead)
			Jump();
			
		if (isDead)
			Fly();

		slopeMoveDirection = Vector3.ProjectOnPlane(moveDirection, slopeHit.normal);
	}
	
	private void Fly()
	{
		//Move Up when Space Bar is pressed and Move Down when Shift is pressed
		if (Input.GetKey(KeyCode.Space))
		{
			rb.AddForce(transform.up * jumpForce * 0.2f, ForceMode.Impulse);
		}
		else if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
		{
			rb.AddForce(transform.up * -jumpForce * 0.2f, ForceMode.Impulse);
		}
	}

	private void MyInput()
	{
		horizontalMovement = Input.GetAxisRaw("Horizontal");
		verticalMovement = Input.GetAxisRaw("Vertical");

		moveDirection = orientation.forward * verticalMovement + orientation.right * horizontalMovement;
	}

	private void Jump()
	{
		if (isGrounded)
		{
			rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
			rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
		}
	}

	private void ControlSpeed()
	{
		if (isDead)
		{
			moveSpeed = Mathf.Lerp(moveSpeed, sprintSpeed * 2.5f, acceleration * Time.deltaTime);
			return;
		}
		
		if (Input.GetKey(sprintKey) && isGrounded)
		{
			moveSpeed = Mathf.Lerp(moveSpeed, sprintSpeed, acceleration * Time.deltaTime);
		}
		else
		{
			moveSpeed = Mathf.Lerp(moveSpeed, walkSpeed, acceleration * Time.deltaTime);
		}
	}

	private void ControlDrag()
	{
		if (isDead)
		{
			rb.linearDamping = airDrag;
			return;
		}
		
		if (isGrounded)
		{
			rb.linearDamping = groundDrag;
		}
		else
		{
			rb.linearDamping = airDrag;
		}
	}

	private void FixedUpdate()
	{
		MovePlayer();
	}

	private void MovePlayer()
	{
		if (isGrounded && !OnSlope())
		{
			rb.AddForce(moveDirection.normalized * moveSpeed * movementMultiplier, ForceMode.Acceleration);
		}
		else if (isGrounded && OnSlope())
		{
			rb.AddForce(slopeMoveDirection.normalized * moveSpeed * movementMultiplier, ForceMode.Acceleration);
		}
		else if (!isGrounded)
		{
			rb.AddForce(moveDirection.normalized * moveSpeed * movementMultiplier * airMultiplier, ForceMode.Acceleration);
		}
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.IsWriting)
		{
			// Send movement data
			stream.SendNext(transform.position);
			stream.SendNext(transform.rotation);
			stream.SendNext(rb.linearVelocity);
		}
		else
		{
			// Receive movement data
			transform.position = (Vector3)stream.ReceiveNext();
			transform.rotation = (Quaternion)stream.ReceiveNext();
			rb.linearVelocity = (Vector3)stream.ReceiveNext();
		}
	}
}
