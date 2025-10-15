using UnityEngine;
using UnityEngine.InputSystem; // Required for the new Input System

public class PlayerMovement : MonoBehaviour
{
	public float moveSpeed = 5f;
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
		Vector3 move = new Vector3(moveInput.x, 0, moveInput.y);
		rb.MovePosition(rb.position + move * moveSpeed * Time.fixedDeltaTime);
	}
}
