using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
	[Header("Attack Hitboxes (Optional)")]
	public GameObject attackHHitbox; // For key H
	public GameObject attackJHitbox; // For key J
	public GameObject attackKHitbox; // For key K
	public GameObject attackLHitbox; // For key L

	[Header("Attack Settings")]
	public float attackDuration = 0.3f; // How long the hitbox stays active
	public float cooldown = 0.5f;       // Time before another attack can start

	private bool canAttack = true;

	// ------------------------------
	// Input Action Events
	// ------------------------------

	public void OnAttackH(InputAction.CallbackContext context)
	{
		if (context.performed && canAttack)
		{
			StartCoroutine(PerformAttack(attackHHitbox, "H"));
		}
	}

	public void OnAttackJ(InputAction.CallbackContext context)
	{
		if (context.performed && canAttack)
		{
			StartCoroutine(PerformAttack(attackJHitbox, "J"));
		}
	}

	public void OnAttackK(InputAction.CallbackContext context)
	{
		if (context.performed && canAttack)
		{
			StartCoroutine(PerformAttack(attackKHitbox, "K"));
		}
	}

	public void OnAttackL(InputAction.CallbackContext context)
	{
		if (context.performed && canAttack)
		{
			StartCoroutine(PerformAttack(attackLHitbox, "L"));
		}
	}

	// ------------------------------
	// Core Attack Functionality
	// ------------------------------
	private System.Collections.IEnumerator PerformAttack(GameObject hitbox, string attackKey)
	{
		canAttack = false;

		Debug.Log($"Attack {attackKey} triggered!");

		if (hitbox != null)
			hitbox.SetActive(true);

		yield return new WaitForSeconds(attackDuration);

		if (hitbox != null)
			hitbox.SetActive(false);

		yield return new WaitForSeconds(cooldown);
		canAttack = true;
	}
}
