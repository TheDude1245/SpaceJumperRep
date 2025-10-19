using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
	public GameObject attackHitbox;
	public float attackDuration = 0.3f;
	public float cooldown = 0.8f;

	private bool canAttack = true;

	void Update()
	{
		if (Input.GetMouseButtonDown(0) && canAttack)
		{
			StartCoroutine(AttackRoutine());
		}
	}

	private System.Collections.IEnumerator AttackRoutine()
	{
		canAttack = false;

		// Enable attack collider
		attackHitbox.SetActive(true);
		yield return new WaitForSeconds(attackDuration);
		attackHitbox.SetActive(false);

		// Wait for cooldown
		yield return new WaitForSeconds(cooldown);
		canAttack = true;
	}
}
