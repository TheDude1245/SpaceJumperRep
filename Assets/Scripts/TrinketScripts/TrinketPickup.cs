using UnityEngine;

public class TrinketPickup : MonoBehaviour
{
    [Header("Pickup Settings")]
    [SerializeField] private bool destroyAfterPickup = true;

    [Header("Optional")]
    [SerializeField] private GameObject pickupEffect;

    private bool hasBeenCollected = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasBeenCollected)
            return;

        if (!other.CompareTag("Player"))
            return;

        CollectTrinket();
    }

    private void CollectTrinket()
    {
        hasBeenCollected = true;

        if (CurrentSaveManager.Instance == null)
        {
            Debug.LogWarning("No CurrentSaveManager found. Trinket was not saved.");
            return;
        }

        CurrentSaveManager.Instance.UnlockTrinket();

        Debug.Log("Trinket collected and saved.");

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