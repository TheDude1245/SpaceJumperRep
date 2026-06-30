using UnityEngine;

public class CoinPickup : MonoBehaviour
{
    [Header("Coin Settings")]
    [SerializeField] private int coinAmount = 1;

    [Header("Pickup Settings")]
    [SerializeField] private bool destroyAfterPickup = true;

    [Header("Optional")]
    [SerializeField] private GameObject pickupEffect;

    private bool hasBeenCollectedThisSession = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasBeenCollectedThisSession)
            return;

        if (!other.CompareTag("Player"))
            return;

        CollectCoin();
    }

    private void CollectCoin()
    {
        hasBeenCollectedThisSession = true;

        if (CurrentSaveManager.Instance == null)
        {
            Debug.LogWarning("No CurrentSaveManager found. Coin was not saved.");
            return;
        }

        CurrentSaveManager.Instance.AddCoins(coinAmount);

        if (pickupEffect != null)
        {
            Instantiate(pickupEffect, transform.position, Quaternion.identity);
        }

        if (destroyAfterPickup)
        {
            Destroy(gameObject);
        }
    }
}