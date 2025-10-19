using UnityEngine;
using System; // Needed for Action

public class Health : MonoBehaviour
{
	[Header("Health Settings")]
	public float maxHealth = 100f;
	private float currentHealth;

	public bool destroyOnDeath = true;
	public GameObject deathEffect;

	// ? Event that triggers when taking damage
	public event Action<float> OnDamaged;

	private void Awake()
	{
		currentHealth = maxHealth;
	}

	public void TakeDamage(float amount)
	{
		currentHealth -= amount;
		currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
		Debug.Log($"{gameObject.name} took {amount} damage (HP: {currentHealth}/{maxHealth})");

		// ? Invoke damage event
		OnDamaged?.Invoke(amount);

		if (currentHealth <= 0)
			Die();
	}

	private void Die()
	{
		Debug.Log($"{gameObject.name} died!");
		if (deathEffect)
			Instantiate(deathEffect, transform.position, Quaternion.identity);
		if (destroyOnDeath)
			Destroy(gameObject);
	}
}
