using UnityEngine;

[RequireComponent(typeof(Collider))]
public class KeyInsertZone : MonoBehaviour
{
    [Header("Door")]
    [SerializeField] private KeyDoor keyDoor;

    private Collider triggerCollider;

    private PlayerCarryController currentCarrier;
    private CarryableKey currentKey;
    private KeyLockSocket currentSocket;

    // =========================================================
    // UNITY
    // =========================================================

    private void Awake()
    {
        triggerCollider =
            GetComponent<Collider>();

        triggerCollider.isTrigger =
            true;

        if (keyDoor == null)
        {
            keyDoor =
                GetComponentInParent<
                    KeyDoor
                >();
        }
    }

    // =========================================================
    // ENTER / STAY
    // =========================================================

    private void OnTriggerEnter(
        Collider other)
    {
        TryPresentPlayerKey(
            other
        );
    }

    private void OnTriggerStay(
        Collider other)
    {
        /*
         * Useful if the player enters without
         * a key and somehow obtains one while
         * still inside the zone.
         */
        TryPresentPlayerKey(
            other
        );
    }

    private void TryPresentPlayerKey(
        Collider other)
    {
        if (keyDoor == null ||
            keyDoor.IsUnlocked)
        {
            return;
        }

        if (currentKey != null)
            return;

        /*
         * Runtime detection of whichever spawned
         * player entered this door zone.
         */
        PlayerCarryController carrier =
            other.GetComponentInParent<
                PlayerCarryController
            >();

        if (carrier == null)
            return;

        if (!carrier.IsCarrying)
            return;

        CarryableItem carriedItem =
            carrier.CurrentItem;

        if (carriedItem == null)
            return;

        CarryableKey key =
            carriedItem.GetComponent<
                CarryableKey
            >();

        /*
         * Bombs and other carried objects are ignored.
         */
        if (key == null)
            return;

        KeyLockSocket socket =
            keyDoor.GetNextAvailableLock();

        if (socket == null)
            return;

        if (!socket.TryPresentKey(
                key))
        {
            return;
        }

        currentCarrier =
            carrier;

        currentKey =
            key;

        currentSocket =
            socket;
    }

    // =========================================================
    // EXIT
    // =========================================================

    private void OnTriggerExit(
        Collider other)
    {
        if (currentCarrier == null ||
            currentKey == null ||
            currentSocket == null)
        {
            return;
        }

        PlayerCarryController carrier =
            other.GetComponentInParent<
                PlayerCarryController
            >();

        if (carrier !=
            currentCarrier)
        {
            return;
        }

        /*
         * If Q has already turned the key,
         * the socket is permanently unlocked
         * and nothing should return.
         */
        if (!currentSocket.IsUnlocked)
        {
            currentSocket.CancelPresentedKey(
                currentKey
            );
        }

        ClearCurrentPresentation();
    }

    private void ClearCurrentPresentation()
    {
        currentCarrier =
            null;

        currentKey =
            null;

        currentSocket =
            null;
    }
}