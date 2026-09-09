using UnityEngine;

public class PlayerLevelSystem : MonoBehaviour
{
	[Header("Level Settings")]
	public int level = 1;
	public float currentExp = 0f;
	public float expToNextLevel = 100f;

	public delegate void ExpChanged();
	public event ExpChanged OnExpChanged;

	public event System.Action OnLevelUp;

	public bool HasWrappedThisFrame { get; private set; }
	public float OverflowExp { get; private set; }

	public void AddExp(float amount)
	{
		HasWrappedThisFrame = false;
		OverflowExp = 0f;

		currentExp += amount;

		if (currentExp >= expToNextLevel)
		{
			// Track leftover BEFORE resetting
			OverflowExp = currentExp - expToNextLevel;

			// Cap EXP so UI can animate up to full
			currentExp = expToNextLevel;

			HasWrappedThisFrame = true;
		}

		OnExpChanged?.Invoke();
	}

	public void CompleteLevelUp()
	{
		// Level goes up
		level++;
		expToNextLevel *= 1.25f;

		// Notify listeners (e.g., the Health script)
		OnLevelUp?.Invoke();

		// Reset EXP to 0 before applying leftover
		currentExp = 0f;

		if (OverflowExp > 0)
		{
			AddExp(OverflowExp);
		}
	}

	public float GetExpPercent()
	{
		return currentExp / expToNextLevel;
	}
}
