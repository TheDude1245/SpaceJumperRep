using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyChase : MonoBehaviour
{
	[Header("References")]
	public Transform player;

	[Header("Sensing")]
	public float senseRange = 12f;
	public LayerMask visionBlockers; // set to Default

	[Header("Chase / Attack")]
	public float attackRange = 1.6f;
	public float attackCooldown = 1.5f;
	public float damageAmount = 10f;

	[Header("Rotation")]
	public float turnSpeed = 10f;

	private enum EnemyState { Idle, Chasing, Returning }
	private EnemyState state = EnemyState.Idle;

	private NavMeshAgent agent;
	private Health playerHealth;
	private Vector3 spawnPos;
	private float lastAttackTime;
	private float loseSightTimer;
	private const float loseSightDelay = 2f;

	void Awake()
	{
		agent = GetComponent<NavMeshAgent>();
		agent.updateRotation = false;
		spawnPos = transform.position;

		var rb = GetComponent<Rigidbody>();
		if (rb)
		{
			rb.isKinematic = true;
			rb.useGravity = true;
			rb.interpolation = RigidbodyInterpolation.Interpolate;
		}
	}

	void Start()
	{
		if (player) playerHealth = player.GetComponent<Health>();
	}

	void Update()
	{
		if (!player || !agent.isOnNavMesh) return;

		float dist = Vector3.Distance(transform.position, player.position);
		bool canSee = CanSeePlayer(dist);

		// 👇 allow “interrupt” from Returning → Chasing if player re-enters range
		if (state == EnemyState.Returning && canSee)
		{
			BeginChase();
			return;
		}

		switch (state)
		{
			case EnemyState.Idle:
				if (canSee) BeginChase();
				break;

			case EnemyState.Chasing:
				UpdateChase(dist, canSee);
				break;

			case EnemyState.Returning:
				UpdateReturn();
				break;
		}

		// smooth facing
		if (agent.velocity.sqrMagnitude > 0.01f)
		{
			var look = Quaternion.LookRotation(agent.velocity.normalized);
			transform.rotation = Quaternion.Slerp(transform.rotation, look, Time.deltaTime * turnSpeed);
		}
	}

	bool CanSeePlayer(float dist)
	{
		if (dist > senseRange) return false;

		Vector3 from = transform.position + Vector3.up * 0.5f;
		Vector3 to = player.position + Vector3.up * 0.5f;

		if (Physics.Linecast(from, to, out RaycastHit hit, visionBlockers, QueryTriggerInteraction.Ignore))
			return hit.transform == player;

		return true;
	}

	void BeginChase()
	{
		state = EnemyState.Chasing;
		agent.isStopped = false;
		agent.SetDestination(player.position);
	}

	void UpdateChase(float dist, bool canSee)
	{
		if (!canSee)
		{
			loseSightTimer += Time.deltaTime;
			if (loseSightTimer >= loseSightDelay)
			{
				loseSightTimer = 0f;
				BeginReturn();
				return;
			}
		}
		else loseSightTimer = 0f;

		if (dist > attackRange)
		{
			if (agent.isStopped) agent.isStopped = false;
			if (!agent.pathPending)
				agent.SetDestination(player.position);
		}
		else
		{
			if (!agent.isStopped) agent.isStopped = true;
			FaceTarget(player.position);
			TryAttack();
		}
	}

	void BeginReturn()
	{
		state = EnemyState.Returning;
		agent.isStopped = false;
		agent.SetDestination(spawnPos);
	}

	void UpdateReturn()
	{
		if (Vector3.Distance(transform.position, spawnPos) <= 0.3f)
		{
			agent.ResetPath();
			agent.isStopped = true;
			state = EnemyState.Idle;
		}
	}

	void TryAttack()
	{
		if (Time.time - lastAttackTime < attackCooldown) return;
		lastAttackTime = Time.time;

		if (playerHealth != null)
			playerHealth.TakeDamage(damageAmount);
	}

	void FaceTarget(Vector3 pos)
	{
		Vector3 dir = pos - transform.position;
		dir.y = 0f;
		if (dir.sqrMagnitude > 0.001f)
		{
			var look = Quaternion.LookRotation(dir.normalized);
			transform.rotation = Quaternion.Slerp(transform.rotation, look, Time.deltaTime * turnSpeed);
		}
	}

	void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.cyan;
		Gizmos.DrawWireSphere(transform.position, senseRange);
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(transform.position, attackRange);
	}
}
