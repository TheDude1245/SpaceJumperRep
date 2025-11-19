using UnityEngine;
using UnityEngine.UI;

public class ExpBarUI : MonoBehaviour
{
	[Header("References")]
	public Image mainFill;
	public Image shadowFill;
	public PlayerLevelSystem levelSystem;

	[Header("Animation Speeds")]
	public float mainSpeed = 3f;
	public float shadowSpeed = 6f;

	private float targetPercent;
	private bool wrapping = false;

	private void Start()
	{
		if (levelSystem == null)
			levelSystem = FindAnyObjectByType<PlayerLevelSystem>();

		Setup(mainFill);
		Setup(shadowFill);

		mainFill.fillAmount = 0;
		shadowFill.fillAmount = 0;

		levelSystem.OnExpChanged += OnExpChanged;
	}

	private void OnDestroy()
	{
		if (levelSystem != null)
			levelSystem.OnExpChanged -= OnExpChanged;
	}

	private void Setup(Image img)
	{
		img.type = Image.Type.Filled;
		img.fillMethod = Image.FillMethod.Horizontal;
		img.fillOrigin = 0; // Left. Use 1 if your bar fills right→left.
	}

	private void OnExpChanged()
	{
		float percent = levelSystem.GetExpPercent();

		if (levelSystem.HasWrappedThisFrame)
		{
			// Step 1: animate to FULL
			wrapping = true;
			targetPercent = 1f;
		}
		else
		{
			// Normal EXP gain
			targetPercent = percent;
		}
	}

	private void Update()
	{
		if (wrapping)
		{
			// Animate both to 100%
			AnimateTo(mainFill, 1f, mainSpeed);
			AnimateTo(shadowFill, 1f, shadowSpeed);

			if (mainFill.fillAmount >= 0.99f)
			{
				// FULL reached → tell LevelSystem to process wrap
				wrapping = false;

				levelSystem.CompleteLevelUp();

				// Reset bars to 0
				mainFill.fillAmount = 0f;
				shadowFill.fillAmount = 0f;

				// New target is leftover EXP
				targetPercent = levelSystem.GetExpPercent();
			}
		}
		else
		{
			// Normal animation
			AnimateTo(mainFill, targetPercent, mainSpeed);

			// Shadow leads (moves faster toward target)
			AnimateTo(shadowFill, targetPercent, shadowSpeed);
		}
	}

	private void AnimateTo(Image img, float target, float speed)
	{
		img.fillAmount = Mathf.Lerp(img.fillAmount, target, Time.deltaTime * speed);
	}
}
