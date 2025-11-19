using UnityEngine;
using TMPro;

public class CoinManager : MonoBehaviour
{
	public static CoinManager Instance;

	[Header("UI Reference")]
	public TMP_Text coinText;

	public int coins = 0;

	private void Awake()
	{
		if (Instance == null)
			Instance = this;
		else
			Destroy(gameObject);
	}

	private void Start()
	{
		UpdateUI();
	}

	public void AddCoin(int amount)
	{
		coins += amount;
		UpdateUI();
	}

	private void UpdateUI()
	{
		coinText.text = coins.ToString();
	}
}
