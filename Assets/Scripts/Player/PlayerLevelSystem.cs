using UnityEngine;

public class PlayerLevelSystem : MonoBehaviour
{
	[Header("Level Settings")]
	public int level = 1;
	public float currentExp = 0f;
	public float expToNextLevel = 100f; // EXP needed for level 2

	public delegate void ExpChanged();
	public event ExpChanged OnExpChanged;

	public void AddExp(float amount)
	{
		currentExp += amount;

		// Level Up
		while (currentExp >= expToNextLevel)
		{
			currentExp -= expToNextLevel;
			level++;

			// Optional EXP scaling
			expToNextLevel *= 1.2f;

			Debug.Log($"LEVEL UP! New level: {level}");
		}

		OnExpChanged?.Invoke();
	}

	public float GetExpPercent()
	{
		return currentExp / expToNextLevel;
	}
}
