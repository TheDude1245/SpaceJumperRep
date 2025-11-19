using UnityEngine;
using System; // Needed for Action

public class Health : MonoBehaviour
{
	[Header("Health Settings")]
	public float maxHealth = 100f;
	public float currentHealth;

	[Header("Death Settings")]
	public bool destroyOnDeath = true;
	public GameObject deathEffect;

	[Header("Experience Reward")]
	[SerializeField] private float expReward = 0f; // EXP given when this entity dies
	public float ExpReward => expReward; // Read-only property

	// Event triggered when taking damage
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

		// Trigger damage event (flash, etc.)
		OnDamaged?.Invoke(amount);

		if (currentHealth <= 0)
			Die();
	}

	private void Die()
	{
		Debug.Log($"{gameObject.name} died!");

		// Spawn death effect
		if (deathEffect != null)
			Instantiate(deathEffect, transform.position, Quaternion.identity);

		// Award EXP to Player
		PlayerLevelSystem levelSystem = FindAnyObjectByType<PlayerLevelSystem>();
		if (levelSystem != null)
		{
			Debug.Log($"Gave {expReward} EXP to player");
			levelSystem.AddExp(expReward);
		}

		// Destroy object
		if (destroyOnDeath)
			Destroy(gameObject);
	}
}
