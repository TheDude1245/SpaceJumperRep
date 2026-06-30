using UnityEngine;

public class TrinketPickup : MonoBehaviour
{
    [Header("Unique Trinket ID")]
    [SerializeField] private string trinketId;

    [Header("Pickup Settings")]
    [SerializeField] private bool destroyAfterPickup = true;

    [Header("Optional")]
    [SerializeField] private GameObject pickupEffect;

    private bool hasBeenCollectedThisSession = false;

    private void Start()
    {
        if (CurrentSaveManager.Instance == null)
        {
            Debug.LogWarning("No CurrentSaveManager found when checking trinket: " + trinketId);
            return;
        }

        if (CurrentSaveManager.Instance.IsTrinketCollected(trinketId))
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasBeenCollectedThisSession)
            return;

        if (!other.CompareTag("Player"))
            return;

        CollectTrinket();
    }

    private void CollectTrinket()
    {
        hasBeenCollectedThisSession = true;

        if (CurrentSaveManager.Instance == null)
        {
            Debug.LogWarning("No CurrentSaveManager found. Trinket was not saved.");
            return;
        }

        bool collectedSuccessfully = CurrentSaveManager.Instance.TryCollectTrinket(trinketId);

        if (!collectedSuccessfully)
            return;

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