using UnityEngine;

public class DestroyOnPlayerCollision : MonoBehaviour
{
	[Header("Tag Settings")]
	public string playerTag = "Player";

	[Header("Pickup Settings")]
	public int coinValue = 1;   // how many coins this object gives

    private void OnCollisionEnter(Collision collision)
	{
		if (collision.gameObject.CompareTag(playerTag))
		{

			// Add coin(s)
			if (CoinManager.Instance != null)
				CoinManager.Instance.AddCoin(coinValue);

            // Destroy object
            Destroy(gameObject);
		}
	}
}
