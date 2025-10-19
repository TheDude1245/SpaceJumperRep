using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class EnemyChase : MonoBehaviour
{
	[Header("References")]
	public Transform player; // assign your Player cube here in Inspector

	[Header("Detection Settings")]
	public float detectionRange = 10f;
	public float fieldOfView = 90f; // degrees
	public float stopDistance = 1.5f;

	[Header("Movement Settings")]
	public float moveSpeed = 3f;
	public float rotationSpeed = 5f;

	private Rigidbody rb;

	private void Awake()
	{
		rb = GetComponent<Rigidbody>();
	}

	private void FixedUpdate()
	{
		if (player == null) return;

		Vector3 directionToPlayer = (player.position - transform.position).normalized;
		float distanceToPlayer = Vector3.Distance(player.position, transform.position);

		// Check if player is within range
		if (distanceToPlayer <= detectionRange)
		{
			// Check if player is within field of view
			float angle = Vector3.Angle(transform.forward, directionToPlayer);
			if (angle <= fieldOfView / 2f)
			{
				// Rotate toward player smoothly
				Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
				rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime));

				// Move toward player if not too close
				if (distanceToPlayer > stopDistance)
				{
					Vector3 move = directionToPlayer * moveSpeed * Time.fixedDeltaTime;
					rb.MovePosition(rb.position + move);
				}
			}
		}
	}

	// Optional: visualize detection range + FOV
	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireSphere(transform.position, detectionRange);

		Vector3 leftBoundary = Quaternion.Euler(0, -fieldOfView / 2f, 0) * transform.forward;
		Vector3 rightBoundary = Quaternion.Euler(0, fieldOfView / 2f, 0) * transform.forward;

		Gizmos.color = Color.red;
		Gizmos.DrawRay(transform.position, leftBoundary * detectionRange);
		Gizmos.DrawRay(transform.position, rightBoundary * detectionRange);
	}
}
