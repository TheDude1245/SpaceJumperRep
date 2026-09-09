using UnityEngine;

[RequireComponent(typeof(CarryableItem))]
public class CarryableKey :
    MonoBehaviour,
    ICarriedItemUse
{
    public KeyLockSocket CurrentSocket
    {
        get;
        private set;
    }

    public bool IsPresentedToLock =>
        CurrentSocket != null;

    private CarryableItem carryableItem;

    private void Awake()
    {
        carryableItem =
            GetComponent<CarryableItem>();
    }

    // =========================================================
    // PRESENT TO LOCK
    // =========================================================

    public void PresentToSocket(
        KeyLockSocket socket)
    {
        if (socket == null)
            return;

        if (carryableItem == null ||
            !carryableItem.IsCarried)
        {
            return;
        }

        CurrentSocket =
            socket;

        carryableItem.PresentAt(
            socket.InsertPoint
        );
    }

    // =========================================================
    // RETURN TO PLAYER
    // =========================================================

    public void ReturnToPlayer()
    {
        if (CurrentSocket == null)
            return;

        CurrentSocket =
            null;

        if (carryableItem != null &&
            carryableItem.IsCarried)
        {
            carryableItem.ReturnToCarryPoint();
        }
    }

    // =========================================================
    // Q
    // =========================================================

    public void Use(
        Transform user)
    {
        /*
         * Q does nothing with a key unless
         * that key is currently sitting in
         * a door lock.
         */
        if (CurrentSocket == null)
            return;

        CurrentSocket.TryUseKey(
            this
        );
    }

    // =========================================================
    // SOCKET CONFIRMED KEY
    // =========================================================

    public void CommitToSocket(
        Transform finalParent)
    {
        CurrentSocket =
            null;

        if (carryableItem != null)
        {
            carryableItem.ConsumeFromCarrier(
                finalParent
            );
        }
    }
}