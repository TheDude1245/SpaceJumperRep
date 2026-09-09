using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PickupHitbox : MonoBehaviour
{
    [Header("Item")]
    [Tooltip(
        "The CarryableItem this pickup radius belongs to."
    )]
    [SerializeField] private CarryableItem item;

    private Collider triggerCollider;

    // =========================================================
    // UNITY
    // =========================================================

    private void Awake()
    {
        triggerCollider =
            GetComponent<Collider>();

        /*
         * This collider exists ONLY for
         * automatic pickup detection.
         */
        triggerCollider.isTrigger =
            true;

        /*
         * Automatically find the CarryableItem
         * on the parent if nothing was assigned.
         */
        if (item == null)
        {
            item =
                GetComponentInParent<
                    CarryableItem
                >();
        }
    }

    // =========================================================
    // PLAYER ENTERS PICKUP RADIUS
    // =========================================================

    private void OnTriggerEnter(
        Collider other)
    {
        if (item == null)
            return;

        if (item.IsCarried)
            return;

        /*
         * THIS is what makes the system work
         * with dynamically spawned characters.
         *
         * We discover whichever actual player
         * entered the trigger.
         */
        PlayerCarryController carrier =
            other.GetComponentInParent<
                PlayerCarryController
            >();

        if (carrier == null)
            return;

        carrier.TryPickUp(
            item
        );
    }
}