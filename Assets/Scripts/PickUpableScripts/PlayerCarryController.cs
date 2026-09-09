using UnityEngine;

public class PlayerCarryController : MonoBehaviour
{
    [Header("Carry")]
    [Tooltip("Point above this character where carried items float.")]
    [SerializeField] private Transform carryPoint;

    [Header("Item Use")]
    [Tooltip("Button used to activate the currently carried item.")]
    [SerializeField] private KeyCode useItemKey = KeyCode.Q;

    public CarryableItem CurrentItem { get; private set; }

    public bool IsCarrying =>
        CurrentItem != null;

    public Transform CarryPoint =>
        carryPoint;

    private void Update()
    {
        if (Input.GetKeyDown(useItemKey))
        {
            UseCurrentItem();
        }
    }

    // =========================================================
    // PICKUP
    // =========================================================

    public bool TryPickUp(CarryableItem item)
    {
        if (item == null)
            return false;

        // One carried object at a time.
        if (CurrentItem != null)
            return false;

        if (carryPoint == null)
        {
            Debug.LogWarning(
                name + " has no CarryPoint assigned."
            );

            return false;
        }

        if (!item.CanBePickedUp)
            return false;

        CurrentItem = item;

        item.BeginCarry(
            this,
            carryPoint
        );

        return true;
    }

    // =========================================================
    // Q - USE CARRIED ITEM
    // =========================================================

    private void UseCurrentItem()
    {
        if (CurrentItem == null)
            return;

        ICarriedItemUse usableItem =
            CurrentItem.GetComponent<ICarriedItemUse>();

        if (usableItem == null)
            return;

        usableItem.Use(transform);
    }

    // =========================================================
    // RELEASE OWNERSHIP
    // =========================================================

    public void ReleaseItem(CarryableItem item)
    {
        if (item == null)
            return;

        if (CurrentItem != item)
            return;

        CurrentItem = null;
    }
}