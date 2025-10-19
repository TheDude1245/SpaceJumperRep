using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Renderer))]
public class FlashOnHit : MonoBehaviour
{
	[Header("Flash Settings")]
	public Color flashColor = Color.red;
	public float flashDuration = 0.1f;

	private Renderer rend;
	private Color originalColor;
	private Material materialInstance;

	private void Awake()
	{
		// Get renderer and create a material instance (so we don't edit shared material)
		rend = GetComponent<Renderer>();
		materialInstance = rend.material;
		originalColor = materialInstance.color;

		// Connect to Health script (if it exists)
		Health health = GetComponent<Health>();
		if (health != null)
			health.OnDamaged += HandleDamage;
	}

	private void OnDestroy()
	{
		// Unsubscribe when destroyed
		Health health = GetComponent<Health>();
		if (health != null)
			health.OnDamaged -= HandleDamage;
	}

	private void HandleDamage(float dmg)
	{
		StopAllCoroutines();
		StartCoroutine(FlashRoutine());
	}

	private IEnumerator FlashRoutine()
	{
		materialInstance.color = flashColor;
		yield return new WaitForSeconds(flashDuration);
		materialInstance.color = originalColor;
	}
}
