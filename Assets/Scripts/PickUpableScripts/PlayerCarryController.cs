using UnityEngine;

public class PlayerCarryController : MonoBehaviour
{
    [Header("Normal Carry")]
    [Tooltip(
        "Carry point used for exclusive items such as bombs."
    )]
    [SerializeField] private Transform carryPoint;

    [Header("Keys")]
    [Tooltip(
        "Each element represents one key inventory slot. " +
        "The number of assigned points is the maximum number " +
        "of keys this player can carry at once."
    )]
    [SerializeField] private Transform[] keyCarryPoints;

    [Header("Item Use")]
    [SerializeField]
    private KeyCode useItemKey =
        KeyCode.Q;

    public CarryableItem CurrentItem
    {
        get;
        private set;
    }

    private CarryableKey[] carriedKeys;

    public bool IsCarrying =>
        CurrentItem != null ||
        KeyCount > 0;

    public bool IsCarryingNormalItem =>
        CurrentItem != null;

    public Transform CarryPoint =>
        carryPoint;

    public int MaxKeys =>
        keyCarryPoints != null
            ? keyCarryPoints.Length
            : 0;

    public int KeyCount
    {
        get
        {
            if (carriedKeys == null)
                return 0;

            int count = 0;

            foreach (CarryableKey key
                     in carriedKeys)
            {
                if (key != null)
                {
                    count++;
                }
            }

            return count;
        }
    }

    // =========================================================
    // UNITY
    // =========================================================

    private void Awake()
    {
        int keySlotCount =
            keyCarryPoints != null
                ? keyCarryPoints.Length
                : 0;

        carriedKeys =
            new CarryableKey[
                keySlotCount
            ];
    }

    private void Update()
    {
        if (Input.GetKeyDown(
                useItemKey))
        {
            UseCarriedItems();
        }
    }

    // =========================================================
    // PICKUP
    // =========================================================

    public bool TryPickUp(
        CarryableItem item)
    {
        if (item == null)
            return false;

        if (!item.CanBePickedUp)
            return false;

        CarryableKey key =
            item.GetComponent<
                CarryableKey
            >();

        if (key != null)
        {
            return TryPickUpKey(
                item,
                key
            );
        }

        return TryPickUpNormalItem(
            item
        );
    }

    // =========================================================
    // NORMAL / EXCLUSIVE ITEM
    // =========================================================

    private bool TryPickUpNormalItem(
        CarryableItem item)
    {
        if (CurrentItem != null)
            return false;

        if (carryPoint == null)
        {
            Debug.LogWarning(
                name +
                " has no normal CarryPoint assigned."
            );

            return false;
        }

        CurrentItem =
            item;

        item.BeginCarry(
            this,
            carryPoint
        );

        return true;
    }

    // =========================================================
    // KEY PICKUP
    // =========================================================

    private bool TryPickUpKey(
        CarryableItem item,
        CarryableKey key)
    {
        if (carriedKeys == null ||
            carriedKeys.Length == 0)
        {
            Debug.LogWarning(
                name +
                " has no Key Carry Points assigned."
            );

            return false;
        }

        int emptySlot =
            GetFirstEmptyKeySlot();

        if (emptySlot < 0)
        {
            /*
             * Key inventory full.
             */
            return false;
        }

        Transform keyCarryPoint =
            keyCarryPoints[
                emptySlot
            ];

        if (keyCarryPoint == null)
        {
            Debug.LogWarning(
                name +
                " has an empty Key Carry Point " +
                "reference at slot " +
                emptySlot +
                "."
            );

            return false;
        }

        carriedKeys[
            emptySlot
        ] = key;

        /*
         * Each key remembers its own CarryPoint.
         */
        item.BeginCarry(
            this,
            keyCarryPoint
        );

        return true;
    }

    private int GetFirstEmptyKeySlot()
    {
        if (carriedKeys == null)
            return -1;

        for (int i = 0;
             i < carriedKeys.Length;
             i++)
        {
            if (carriedKeys[i] == null)
            {
                return i;
            }
        }

        return -1;
    }

    // =========================================================
    // GET NEXT FREE KEY
    // =========================================================

    public CarryableKey GetAvailableKey()
    {
        if (carriedKeys == null)
            return null;

        foreach (CarryableKey key
                 in carriedKeys)
        {
            if (key == null)
                continue;

            /*
             * A key already sitting inside a lock
             * is not available for another lock.
             */
            if (key.IsPresentedToLock)
                continue;

            return key;
        }

        return null;
    }

    // =========================================================
    // Q
    // =========================================================

    private void UseCarriedItems()
    {
        /*
         * HIGHEST PRIORITY:
         *
         * Turn ALL keys that are currently
         * presented inside locks.
         */
        bool usedPresentedKey =
            UseAllPresentedKeys();

        /*
         * If at least one key was inserted,
         * Q belongs to the keys this frame.
         *
         * Therefore a carried bomb will NOT also
         * be thrown at the same time.
         */
        if (usedPresentedKey)
            return;

        // =====================================================
        // NORMAL CARRIED ITEM
        // =====================================================

        if (CurrentItem == null)
            return;

        ICarriedItemUse usableItem =
            CurrentItem.GetComponent<
                ICarriedItemUse
            >();

        if (usableItem == null)
            return;

        usableItem.Use(
            transform
        );
    }

    // =========================================================
    // TURN ALL PRESENTED KEYS
    // =========================================================

    private bool UseAllPresentedKeys()
    {
        if (carriedKeys == null)
            return false;

        bool usedAnyKey =
            false;

        foreach (CarryableKey key
                 in carriedKeys)
        {
            if (key == null)
                continue;

            if (!key.IsPresentedToLock)
                continue;

            /*
             * Every presented key receives Use()
             * during the same frame.
             *
             * Therefore all locks turn together.
             */
            key.Use(
                transform
            );

            usedAnyKey =
                true;
        }

        return usedAnyKey;
    }

    // =========================================================
    // RELEASE
    // =========================================================

    public void ReleaseItem(
        CarryableItem item)
    {
        if (item == null)
            return;

        CarryableKey key =
            item.GetComponent<
                CarryableKey
            >();

        if (key != null)
        {
            ReleaseKey(
                key
            );

            return;
        }

        if (CurrentItem != item)
            return;

        CurrentItem =
            null;
    }

    private void ReleaseKey(
        CarryableKey key)
    {
        if (carriedKeys == null)
            return;

        for (int i = 0;
             i < carriedKeys.Length;
             i++)
        {
            if (carriedKeys[i] ==
                key)
            {
                carriedKeys[i] =
                    null;

                return;
            }
        }
    }
}