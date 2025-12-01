using UnityEngine;
using System.Collections;


public class FlashOnHit : MonoBehaviour
{
	[Header("Flash Settings")]
	public Color flashColor = Color.blue;
	public float flashDuration = 0.2f;

	public Renderer rend;
	private Color originalColor;
	private Material materialInstance;

	private void Awake()
	{
		// Get renderer and create a material instance (so we don't edit shared material)
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

		float t = 0f;
		while (t < 1f)
		{
			t += Time.deltaTime / flashDuration;
			materialInstance.color = Color.Lerp(flashColor, originalColor, t);
			yield return null;
		}
	}

}
