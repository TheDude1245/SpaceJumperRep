using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class EnemyChase : MonoBehaviour
{
	[Header("References")]
	public Transform player;
	public float damageAmount = 10f;

	[Header("Detection Settings")]
	public float detectionRange = 10f;
	public float fieldOfView = 90f;
	public float passiveRange = 4f;
	public float stopDistance = 1.5f;
	public LayerMask obstructionMask; // 🧠 Added

	[Header("Movement Settings")]
	public float moveSpeed = 3f;
	public float rotationSpeed = 5f;

	[Header("Attack Settings")]
	public float attackCooldown = 1.5f;
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

		// 👁 Vision (FOV) check
		bool canSeePlayer = false;
		if (distanceToPlayer <= detectionRange)
		{
			float angle = Vector3.Angle(transform.forward, directionToPlayer);
			if (angle <= fieldOfView / 2f)
			{
				Vector3 origin = transform.position + Vector3.up * 0.5f;
				Vector3 target = player.position + Vector3.up * 0.5f;

				if (Physics.Raycast(origin, (target - origin).normalized, out RaycastHit hit, detectionRange))
				{
					if (hit.transform == player)
						canSeePlayer = true;
				}
			}
		}

		// 🌀 Passive detection (proximity + line-of-sight check)
		bool canSensePlayer = false;
		if (distanceToPlayer <= passiveRange)
		{
			Vector3 origin = transform.position + Vector3.up * 0.5f;
			Vector3 target = player.position + Vector3.up * 0.5f;

			if (Physics.Raycast(origin, (target - origin).normalized, out RaycastHit hit, passiveRange))
			{
				if (hit.transform == player)
					canSensePlayer = true;
			}
		}

		// ✅ Combined logic
		bool shouldChase = canSeePlayer || canSensePlayer;

		if (shouldChase)
		{
			Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
			rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime));

			if (distanceToPlayer > stopDistance)
			{
				Vector3 move = directionToPlayer * moveSpeed * Time.fixedDeltaTime;
				rb.MovePosition(rb.position + move);
			}
			else
			{
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
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireSphere(transform.position, detectionRange);

		Gizmos.color = Color.cyan;
		Gizmos.DrawWireSphere(transform.position, passiveRange);

		Vector3 leftBoundary = Quaternion.Euler(0, -fieldOfView / 2f, 0) * transform.forward;
		Vector3 rightBoundary = Quaternion.Euler(0, fieldOfView / 2f, 0) * transform.forward;

		Gizmos.color = Color.red;
		Gizmos.DrawRay(transform.position, leftBoundary * detectionRange);
		Gizmos.DrawRay(transform.position, rightBoundary * detectionRange);
	}
}
