using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class KeyInsertZone : MonoBehaviour
{
    [Header("Door")]
    [SerializeField] private KeyDoor keyDoor;

    private Collider triggerCollider;

    /*
     * All keys that THIS zone has moved
     * into locks.
     *
     * We keep these so they can all return
     * to the player if the player walks away.
     */
    private readonly List<CarryableKey>
        presentedKeys =
            new List<CarryableKey>();

    /*
     * A player may have multiple colliders.
     *
     * This prevents one child collider leaving
     * the trigger from incorrectly returning
     * all keys while another player collider
     * is still inside.
     */
    private readonly HashSet<Collider>
        playerCollidersInside =
            new HashSet<Collider>();

    private PlayerCarryController
        currentCarrier;

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
    // ENTER
    // =========================================================

    private void OnTriggerEnter(
        Collider other)
    {
        PlayerCarryController carrier =
            other.GetComponentInParent<
                PlayerCarryController
            >();

        if (carrier == null)
            return;

        /*
         * Only deal with one player using this
         * particular door zone at a time.
         */
        if (currentCarrier != null &&
            currentCarrier != carrier)
        {
            return;
        }

        currentCarrier =
            carrier;

        playerCollidersInside.Add(
            other
        );

        PresentAllPossibleKeys(
            carrier
        );
    }

    // =========================================================
    // STAY
    // =========================================================

    private void OnTriggerStay(
        Collider other)
    {
        PlayerCarryController carrier =
            other.GetComponentInParent<
                PlayerCarryController
            >();

        if (carrier == null)
            return;

        if (currentCarrier != null &&
            currentCarrier != carrier)
        {
            return;
        }

        currentCarrier =
            carrier;

        playerCollidersInside.Add(
            other
        );

        CleanupPresentedKeyList();

        /*
         * If the player somehow gains another key
         * while already standing here, or a slot
         * becomes available, present it too.
         */
        PresentAllPossibleKeys(
            carrier
        );
    }

    // =========================================================
    // PRESENT ALL POSSIBLE KEYS
    // =========================================================

    private void PresentAllPossibleKeys(
        PlayerCarryController carrier)
    {
        if (carrier == null)
            return;

        if (keyDoor == null ||
            keyDoor.IsUnlocked)
        {
            return;
        }

        /*
         * Safety limit prevents a malformed setup
         * from ever causing an infinite loop.
         */
        int safetyCounter =
            0;

        int maxAttempts =
            Mathf.Max(
                1,
                carrier.MaxKeys +
                keyDoor.RequiredKeys
            );

        while (safetyCounter <
               maxAttempts)
        {
            safetyCounter++;

            // =============================================
            // GET AN UNUSED KEY
            // =============================================

            CarryableKey key =
                carrier.GetAvailableKey();

            if (key == null)
            {
                /*
                 * Player has no more free keys.
                 */
                break;
            }

            // =============================================
            // GET AN UNUSED LOCK
            // =============================================

            KeyLockSocket socket =
                keyDoor.GetNextAvailableLock();

            if (socket == null)
            {
                /*
                 * Door has no more empty locks.
                 */
                break;
            }

            // =============================================
            // PRESENT
            // =============================================

            bool presented =
                socket.TryPresentKey(
                    key
                );

            if (!presented)
            {
                /*
                 * Something prevented this socket
                 * from accepting a key.
                 *
                 * Stop rather than repeatedly trying
                 * the same pair forever.
                 */
                break;
            }

            presentedKeys.Add(
                key
            );

            /*
             * Loop continues.
             *
             * Because this key is now marked as
             * IsPresentedToLock, GetAvailableKey()
             * will automatically return the NEXT key.
             *
             * Likewise, the occupied socket is skipped
             * by GetNextAvailableLock().
             */
        }
    }

    // =========================================================
    // EXIT
    // =========================================================

    private void OnTriggerExit(
        Collider other)
    {
        PlayerCarryController carrier =
            other.GetComponentInParent<
                PlayerCarryController
            >();

        if (carrier == null)
            return;

        if (carrier !=
            currentCarrier)
        {
            return;
        }

        playerCollidersInside.Remove(
            other
        );

        /*
         * Don't return the keys until the entire
         * player has actually left the trigger.
         */
        if (playerCollidersInside.Count > 0)
            return;

        ReturnUnusedKeys();

        currentCarrier =
            null;
    }

    // =========================================================
    // RETURN KEYS
    // =========================================================

    private void ReturnUnusedKeys()
    {
        foreach (CarryableKey key
                 in presentedKeys)
        {
            if (key == null)
                continue;

            KeyLockSocket socket =
                key.CurrentSocket;

            /*
             * If CurrentSocket is null, that key
             * has already been permanently committed.
             */
            if (socket == null)
                continue;

            /*
             * Completed locks keep their key.
             */
            if (socket.IsUnlocked)
                continue;

            socket.CancelPresentedKey(
                key
            );
        }

        presentedKeys.Clear();
    }

    // =========================================================
    // CLEANUP
    // =========================================================

    private void CleanupPresentedKeyList()
    {
        for (int i =
                 presentedKeys.Count - 1;
             i >= 0;
             i--)
        {
            CarryableKey key =
                presentedKeys[i];

            if (key == null ||
                !key.IsPresentedToLock)
            {
                presentedKeys.RemoveAt(
                    i
                );
            }
        }
    }
}