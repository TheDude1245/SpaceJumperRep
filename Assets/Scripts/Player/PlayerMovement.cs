using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
	[Header("Movement Settings")]
	public float moveSpeed = 5f;
	public float rotationSpeed = 10f; // how fast to rotate toward movement direction

	private PlayerControls controls;
	private Vector2 moveInput;
	private Rigidbody rb;

	private void Awake()
	{
		controls = new PlayerControls();
		rb = GetComponent<Rigidbody>();
	}

	private void OnEnable()
	{
		controls.Player.Enable();
		controls.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
		controls.Player.Move.canceled += ctx => moveInput = Vector2.zero;
	}

	private void OnDisable()
	{
		controls.Player.Disable();
	}

	private void FixedUpdate()
	{
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
	}
}
