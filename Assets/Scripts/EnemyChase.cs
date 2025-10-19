using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class EnemyChase : MonoBehaviour
{
	[Header("References")]
	public Transform player; // assign your Player cube here
	public float damageAmount = 10f;

	[Header("Detection Settings")]
	public float detectionRange = 10f;    // how far the enemy can see
	public float fieldOfView = 90f;       // degrees (cone)
	public float passiveRange = 4f;       // how close before enemy senses player
	public float stopDistance = 1.5f;

	[Header("Movement Settings")]
	public float moveSpeed = 3f;
	public float rotationSpeed = 5f;

	[Header("Attack Settings")]
	public float attackCooldown = 1.5f; // seconds between attacks
	private float lastAttackTime;

	private Rigidbody rb;
	private Health playerHealth;

	private void Awake()
	{
		rb = GetComponent<Rigidbody>();
	}

	private void Start()
	{
		if (player != null)
			playerHealth = player.GetComponent<Health>();
	}

	private void FixedUpdate()
	{
		if (player == null) return;

		Vector3 directionToPlayer = (player.position - transform.position).normalized;
		float distanceToPlayer = Vector3.Distance(player.position, transform.position);

		// 👁 Check line-of-sight vision (FOV cone)
		bool canSeePlayer = false;
		if (distanceToPlayer <= detectionRange)
		{
			float angle = Vector3.Angle(transform.forward, directionToPlayer);
			if (angle <= fieldOfView / 2f)
			{
				// Optional: add wall detection here
				canSeePlayer = true;
			}
		}

		// 🌀 Check passive perception (close range)
		bool canSensePlayer = distanceToPlayer <= passiveRange;

		// ✅ Combined detection
		bool shouldChase = canSeePlayer || canSensePlayer;

		if (shouldChase)
		{
			// Rotate smoothly toward player
			Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
			rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime));

			// Move toward player if not too close
			if (distanceToPlayer > stopDistance)
			{
				Vector3 move = directionToPlayer * moveSpeed * Time.fixedDeltaTime;
				rb.MovePosition(rb.position + move);
			}
			else
			{
				// Attack if cooldown is ready
				TryAttack();
			}
		}
	}

	private void TryAttack()
	{
		if (Time.time - lastAttackTime >= attackCooldown)
		{
			lastAttackTime = Time.time;
			Attack();
		}
	}

	private void Attack()
	{
		if (playerHealth != null)
		{
			playerHealth.TakeDamage(damageAmount);
			Debug.Log($"{gameObject.name} attacked the player for {damageAmount} damage!");
		}
	}

	private void OnDrawGizmosSelected()
	{
		// Vision range (yellow)
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireSphere(transform.position, detectionRange);

		// Passive perception range (cyan)
		Gizmos.color = Color.cyan;
		Gizmos.DrawWireSphere(transform.position, passiveRange);

		// Field of View cone (red lines)
		Vector3 leftBoundary = Quaternion.Euler(0, -fieldOfView / 2f, 0) * transform.forward;
		Vector3 rightBoundary = Quaternion.Euler(0, fieldOfView / 2f, 0) * transform.forward;

		Gizmos.color = Color.red;
		Gizmos.DrawRay(transform.position, leftBoundary * detectionRange);
		Gizmos.DrawRay(transform.position, rightBoundary * detectionRange);
	}
}
