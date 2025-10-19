using UnityEngine;

[System.Serializable]
public class CharacterStats : MonoBehaviour
{
    [Header("Base Stats")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float damagePerHit = 10f;

    private float currentHealth;

    [Header("Optional Settings")]
    public bool destroyOnDeath = true;
    public GameObject deathEffect; // optional particles, etc.

    private void Start()
    {
        currentHealth = maxHealth;
    }

    // --- HEALTH SYSTEM ---
    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Max(0, currentHealth);

        Debug.Log($"{gameObject.name} took {amount} damage! HP = {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
    }

    private void Die()
    {
        Debug.Log($"{gameObject.name} died!");

        if (deathEffect)
            Instantiate(deathEffect, transform.position, Quaternion.identity);

        if (destroyOnDeath)
            Destroy(gameObject);
    }

    // --- ATTACK SYSTEM ---
    public void DealDamage(CharacterStats target)
    {
        if (target == null) return;
        target.TakeDamage(damagePerHit);
    }

    // --- Getters for other scripts ---
    public float GetHealth() => currentHealth;
    public float GetMaxHealth() => maxHealth;
    public float GetDamage() => damagePerHit;
}
