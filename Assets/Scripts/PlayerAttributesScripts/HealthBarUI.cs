using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
	[Header("References")]
	public Image mainFill;        // instant bar
	public Image shadowFill;      // delayed bar
	public Health playerHealth;

	[Header("Animation Settings")]
	public float smoothSpeed = 8f;     // speed for main bar
	public float shadowSpeed = 2f;     // speed for delayed bar

	private float targetFill;

	private void Start()
	{
		if (playerHealth == null)
			playerHealth = FindAnyObjectByType<Health>();
	}

	private void Update()
	{
		// Calculate target fill amount
		targetFill = playerHealth.currentHealth / playerHealth.maxHealth;

		// MAIN BAR = instantly follows health, but smoothed
		mainFill.fillAmount = Mathf.Lerp(mainFill.fillAmount, targetFill, Time.deltaTime * smoothSpeed);

		// SHADOW BAR = slower “lagging” health bar
		if (shadowFill.fillAmount > mainFill.fillAmount)
		{
			// shadow catches up slowly
			shadowFill.fillAmount = Mathf.Lerp(shadowFill.fillAmount, mainFill.fillAmount, Time.deltaTime * shadowSpeed);
		}
		else
		{
			// if healing, shadow jumps to main instantly
			shadowFill.fillAmount = mainFill.fillAmount;
		}
	}
}
