using UnityEngine;
using UnityEngine.Events;

public class KeyDoor : MonoBehaviour
{
    [Header("Locks")]
    [SerializeField] private KeyLockSocket[] locks;

    [Header("Door Collision")]
    [Tooltip(
        "The collider blocking the player from walking through the door."
    )]
    [SerializeField] private Collider doorCollider;

    [Header("Door Complete")]
    [SerializeField] private UnityEvent onDoorUnlocked;

    public bool IsUnlocked
    {
        get;
        private set;
    }

    public int RequiredKeys =>
        locks != null
            ? locks.Length
            : 0;

    public int InsertedKeys
    {
        get
        {
            if (locks == null)
                return 0;

            int count = 0;

            foreach (KeyLockSocket socket in locks)
            {
                if (socket != null &&
                    socket.IsUnlocked)
                {
                    count++;
                }
            }

            return count;
        }
    }

    // =========================================================
    // FIND NEXT AVAILABLE LOCK
    // =========================================================

    public KeyLockSocket GetNextAvailableLock()
    {
        if (locks == null)
            return null;

        foreach (KeyLockSocket socket in locks)
        {
            if (socket == null)
                continue;

            if (socket.IsUnlocked)
                continue;

            if (socket.HasPresentedKey)
                continue;

            return socket;
        }

        return null;
    }

    // =========================================================
    // LOCK COMPLETED
    // =========================================================

    public void NotifyLockUnlocked(
        KeyLockSocket socket)
    {
        if (IsUnlocked)
            return;

        if (InsertedKeys < RequiredKeys)
            return;

        UnlockDoor();
    }

    // =========================================================
    // DOOR COMPLETE
    // =========================================================

    private void UnlockDoor()
    {
        if (IsUnlocked)
            return;

        IsUnlocked = true;

        /*
         * For now:
         *
         * Door stays visually closed,
         * but its blocking collider is disabled.
         *
         * Later this can happen during the
         * gate-opening animation instead.
         */
        if (doorCollider != null)
        {
            doorCollider.enabled = false;
        }

        Debug.Log(
            name +
            " unlocked with " +
            RequiredKeys +
            " key(s)."
        );

        onDoorUnlocked?.Invoke();
    }
}