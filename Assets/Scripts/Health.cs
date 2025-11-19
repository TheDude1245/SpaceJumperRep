using UnityEngine;
using System; // Needed for Action

public class Health : MonoBehaviour
{
	[Header("Entity Type")]
	public bool isPlayer = false;

	[Header("Health Settings")]
	public float maxHealth = 100f;
	public float currentHealth;

	[Header("Death Settings")]
	public bool destroyOnDeath = true;
	public GameObject deathEffect;

	[Header("Experience Reward")]
	[SerializeField] private float expReward = 0f; // EXP given when this entity dies
	public float ExpReward => expReward; // Read-only property

	public event Action<float> OnDamaged;

	private void Start()
	{
		PlayerLevelSystem levelSys = FindAnyObjectByType<PlayerLevelSystem>();
		if (isPlayer && levelSys != null)
		{
			levelSys.OnLevelUp += HandleLevelUp;
		}
	}

	private void Awake()
	{
		currentHealth = maxHealth;
	}

	private void HandleLevelUp()
	{
		float healAmount = maxHealth * 0.30f;  // Heal 30% max HP
		Heal(healAmount);
	}

	public void Heal(float amount)
	{
		currentHealth += amount;
		currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
		Debug.Log($"{gameObject.name} healed {amount}. HP: {currentHealth}/{maxHealth}");
	}

	public void TakeDamage(float amount)
	{
		currentHealth -= amount;
		currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

		Debug.Log($"{gameObject.name} took {amount} damage (HP: {currentHealth}/{maxHealth})");

		OnDamaged?.Invoke(amount);

		if (currentHealth <= 0)
			Die();
	}

	private void Die()
	{
		Debug.Log($"{gameObject.name} died!");

		if (deathEffect != null)
			Instantiate(deathEffect, transform.position, Quaternion.identity);

		PlayerLevelSystem levelSystem = FindAnyObjectByType<PlayerLevelSystem>();
		if (levelSystem != null)
		{
			Debug.Log($"Gave {expReward} EXP to player");
			levelSystem.AddExp(expReward);
		}

		if (destroyOnDeath)
			Destroy(gameObject);
	}
}
