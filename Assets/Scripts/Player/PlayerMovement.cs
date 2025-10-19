using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
	[Header("Movement Settings")]
	public float moveSpeed = 5f;
	public float rotationSpeed = 10f;
	public float jumpForce = 7f;

	[Header("Ground Check Settings")]
	public Transform groundCheckPoint; // small empty object under player
	public float groundCheckRadius = 0.3f;
	public LayerMask groundLayer;

	private PlayerControls controls;
	private Vector2 moveInput;
	private Rigidbody rb;

	private bool isGrounded = true;
	private bool jumpPressed = false;

	private void Awake()
	{
		controls = new PlayerControls();
		rb = GetComponent<Rigidbody>();
	}

	private void OnEnable()
	{
		controls.Player.Enable();

		// Movement input
		controls.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
		controls.Player.Move.canceled += ctx => moveInput = Vector2.zero;

		// Jump input
		controls.Player.Jump.performed += ctx => OnJump();
	}

	private void OnDisable()
	{
		controls.Player.Disable();
	}

	private void FixedUpdate()
	{
		// --- GROUNDCHECK ---
		isGrounded = Physics.CheckSphere(groundCheckPoint.position, groundCheckRadius, groundLayer);

		// --- MOVEMENT ---
		Vector3 moveDirection = new Vector3(moveInput.x, 0, moveInput.y);

		if (moveDirection.magnitude > 0.1f)
		{
			// Move the player
			Vector3 move = moveDirection.normalized * moveSpeed * Time.fixedDeltaTime;
			rb.MovePosition(rb.position + move);

			// Rotate smoothly toward movement direction
			Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
			rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime));
		}

		// --- JUMP ---
		if (jumpPressed && isGrounded)
		{
			rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
			isGrounded = false;
			jumpPressed = false;
		}
	}

	private void OnJump()
	{
		if (isGrounded)
			jumpPressed = true;
	}

	// --- DEBUG VISUALIZATION ---
	private void OnDrawGizmosSelected()
	{
		if (groundCheckPoint != null)
		{
			Gizmos.color = Color.red;
			Gizmos.DrawWireSphere(groundCheckPoint.position, groundCheckRadius);
		}
	}
}
