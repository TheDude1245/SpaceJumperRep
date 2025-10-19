using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Damage : MonoBehaviour
{
	[Header("Damage Settings")]
	public float damageAmount = 20f;
	public bool destroyOnHit = false; // for projectiles, etc.

	private void OnTriggerEnter(Collider other)
	{
		Health health = other.GetComponent<Health>();

		if (health != null)
		{
			health.TakeDamage(damageAmount);
			Debug.Log($"{name} dealt {damageAmount} damage to {other.name}");
		}

		if (destroyOnHit)
			Destroy(gameObject);
	}
}
