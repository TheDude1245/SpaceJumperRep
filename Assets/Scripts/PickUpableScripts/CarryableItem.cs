using System.Collections;
using UnityEngine;

public class CarryableItem : MonoBehaviour
{
    [Header("Pickup Movement")]
    [SerializeField] private float pickupDuration = 0.25f;

    [Header("Carry Position")]
    [SerializeField]
    private Vector3 carryPositionOffset =
        Vector3.zero;

    [SerializeField]
    private Vector3 carryRotation =
        Vector3.zero;

    [Header("Carried Floating")]
    [SerializeField] private float floatHeight = 0.12f;
    [SerializeField] private float floatSpeed = 2f;

    [Header("World Floating")]
    [SerializeField] private bool floatWhileInWorld = true;
    [SerializeField] private float worldFloatHeight = 0.12f;
    [SerializeField] private float worldFloatSpeed = 1.5f;

    [Header("Presentation")]
    [Tooltip(
        "How long the item takes to fly from above the player " +
        "to something such as a keyhole."
    )]
    [SerializeField] private float presentationDuration = 0.25f;

    [Tooltip(
        "How long the item takes to return from a keyhole " +
        "to above the player."
    )]
    [SerializeField] private float returnDuration = 0.2f;

    public bool IsCarried { get; private set; }

    public bool IsPresented { get; private set; }

    public PlayerCarryController Carrier { get; private set; }

    public bool CanBePickedUp =>
        !IsCarried &&
        Time.time >= nextAllowedPickupTime;

    private Transform carryPoint;
    private Transform presentationTarget;

    private Rigidbody rb;
    private Collider[] itemColliders;

    private Coroutine movementRoutine;

    private float floatTimer;
    private float worldFloatTimer;

    private float nextAllowedPickupTime;

    private Vector3 worldFloatBasePosition;

    private bool usingWorldPhysics;

    // =========================================================
    // UNITY
    // =========================================================

    private void Awake()
    {
        rb =
            GetComponent<Rigidbody>();

        itemColliders =
            GetComponentsInChildren<Collider>(
                true
            );

        /*
         * World pickups begin as controlled,
         * non-physical floating objects.
         */
        if (rb != null)
        {
            rb.linearVelocity =
                Vector3.zero;

            rb.angularVelocity =
                Vector3.zero;

            rb.useGravity =
                false;

            rb.isKinematic =
                true;
        }

        usingWorldPhysics =
            false;

        worldFloatBasePosition =
            transform.position;
    }

    private void Update()
    {
        // =====================================================
        // CARRIED
        // =====================================================

        if (IsCarried)
        {
            /*
             * Only float above player while actually
             * at the CarryPoint.
             *
             * Presented keys do not bob inside locks.
             */
            if (!IsPresented &&
                carryPoint != null &&
                movementRoutine == null)
            {
                UpdateCarriedFloating();
            }

            return;
        }

        // =====================================================
        // WORLD PICKUP
        // =====================================================

        if (!usingWorldPhysics &&
            floatWhileInWorld)
        {
            UpdateWorldFloating();
        }
    }

    // =========================================================
    // WORLD FLOATING
    // =========================================================

    private void UpdateWorldFloating()
    {
        worldFloatTimer +=
            Time.deltaTime *
            worldFloatSpeed;

        float offset =
            Mathf.Sin(
                worldFloatTimer
            ) *
            worldFloatHeight;

        Vector3 position =
            worldFloatBasePosition;

        position.y +=
            offset;

        transform.position =
            position;
    }

    // =========================================================
    // PICK UP
    // =========================================================

    public void BeginCarry(
        PlayerCarryController carrier,
        Transform targetCarryPoint)
    {
        if (!CanBePickedUp)
            return;

        if (carrier == null ||
            targetCarryPoint == null)
        {
            return;
        }

        Carrier =
            carrier;

        carryPoint =
            targetCarryPoint;

        IsCarried =
            true;

        IsPresented =
            false;

        usingWorldPhysics =
            false;

        floatTimer =
            0f;

        DisableWorldPhysics();

        StartMovement(
            carryPoint,
            carryPositionOffset,
            Quaternion.Euler(carryRotation),
            pickupDuration,
            true
        );
    }

    // =========================================================
    // PRESENT ITEM
    // =========================================================

    public void PresentAt(
        Transform target)
    {
        if (!IsCarried)
            return;

        if (target == null)
            return;

        presentationTarget =
            target;

        IsPresented =
            true;

        /*
         * Key flies exactly to the socket:
         *
         * Local Position = zero
         * Local Rotation = identity
         */
        StartMovement(
            target,
            Vector3.zero,
            Quaternion.identity,
            presentationDuration,
            true
        );
    }

    // =========================================================
    // RETURN TO PLAYER
    // =========================================================

    public void ReturnToCarryPoint()
    {
        if (!IsCarried)
            return;

        if (carryPoint == null)
            return;

        presentationTarget =
            null;

        IsPresented =
            false;

        StartMovement(
            carryPoint,
            carryPositionOffset,
            Quaternion.Euler(carryRotation),
            returnDuration,
            true
        );
    }

    // =========================================================
    // GENERIC SMOOTH MOVEMENT
    // =========================================================

    private void StartMovement(
        Transform target,
        Vector3 targetLocalPosition,
        Quaternion targetLocalRotation,
        float duration,
        bool parentWhenFinished)
    {
        if (movementRoutine != null)
        {
            StopCoroutine(
                movementRoutine
            );
        }

        movementRoutine =
            StartCoroutine(
                MoveToTargetRoutine(
                    target,
                    targetLocalPosition,
                    targetLocalRotation,
                    duration,
                    parentWhenFinished
                )
            );
    }

    private IEnumerator MoveToTargetRoutine(
        Transform target,
        Vector3 targetLocalPosition,
        Quaternion targetLocalRotation,
        float duration,
        bool parentWhenFinished)
    {
        if (target == null)
        {
            movementRoutine = null;
            yield break;
        }

        /*
         * Detach while flying so changing parent
         * transforms don't distort the movement.
         */
        transform.SetParent(
            null,
            true
        );

        Vector3 startPosition =
            transform.position;

        Quaternion startRotation =
            transform.rotation;

        if (duration <= 0f)
        {
            FinishMovement(
                target,
                targetLocalPosition,
                targetLocalRotation,
                parentWhenFinished
            );

            yield break;
        }

        float elapsed =
            0f;

        while (elapsed <
               duration)
        {
            if (target == null)
            {
                movementRoutine =
                    null;

                yield break;
            }

            /*
             * Recalculate every frame because the
             * player may still be moving.
             */
            Vector3 targetWorldPosition =
                target.TransformPoint(
                    targetLocalPosition
                );

            Quaternion targetWorldRotation =
                target.rotation *
                targetLocalRotation;

            elapsed +=
                Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    elapsed /
                    duration
                );

            float smoothT =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    t
                );

            transform.position =
                Vector3.Lerp(
                    startPosition,
                    targetWorldPosition,
                    smoothT
                );

            transform.rotation =
                Quaternion.Slerp(
                    startRotation,
                    targetWorldRotation,
                    smoothT
                );

            yield return null;
        }

        FinishMovement(
            target,
            targetLocalPosition,
            targetLocalRotation,
            parentWhenFinished
        );
    }

    private void FinishMovement(
        Transform target,
        Vector3 targetLocalPosition,
        Quaternion targetLocalRotation,
        bool parentWhenFinished)
    {
        if (target == null)
        {
            movementRoutine =
                null;

            return;
        }

        if (parentWhenFinished)
        {
            transform.SetParent(
                target,
                false
            );

            transform.localPosition =
                targetLocalPosition;

            transform.localRotation =
                targetLocalRotation;
        }
        else
        {
            transform.position =
                target.TransformPoint(
                    targetLocalPosition
                );

            transform.rotation =
                target.rotation *
                targetLocalRotation;
        }

        movementRoutine =
            null;

        if (!IsPresented)
        {
            floatTimer =
                0f;
        }
    }

    // =========================================================
    // CARRIED FLOAT
    // =========================================================

    private void UpdateCarriedFloating()
    {
        floatTimer +=
            Time.deltaTime *
            floatSpeed;

        float offset =
            Mathf.Sin(
                floatTimer
            ) *
            floatHeight;

        Vector3 position =
            carryPositionOffset;

        position.y +=
            offset;

        transform.localPosition =
            position;
    }

    // =========================================================
    // DISABLE WORLD PHYSICS
    // =========================================================

    private void DisableWorldPhysics()
    {
        usingWorldPhysics =
            false;

        if (rb != null)
        {
            rb.linearVelocity =
                Vector3.zero;

            rb.angularVelocity =
                Vector3.zero;

            rb.useGravity =
                false;

            rb.isKinematic =
                true;
        }

        if (itemColliders != null)
        {
            foreach (Collider col
                     in itemColliders)
            {
                if (col != null)
                {
                    col.enabled =
                        false;
                }
            }
        }
    }

    // =========================================================
    // ITEM IS PERMANENTLY USED
    // =========================================================

    public void ConsumeFromCarrier(
        Transform newParent)
    {
        if (!IsCarried)
            return;

        if (movementRoutine != null)
        {
            StopCoroutine(
                movementRoutine
            );

            movementRoutine =
                null;
        }

        PlayerCarryController oldCarrier =
            Carrier;

        Carrier =
            null;

        carryPoint =
            null;

        presentationTarget =
            null;

        IsCarried =
            false;

        IsPresented =
            false;

        usingWorldPhysics =
            false;

        if (newParent != null)
        {
            transform.SetParent(
                newParent,
                true
            );
        }

        if (oldCarrier != null)
        {
            oldCarrier.ReleaseItem(
                this
            );
        }
    }

    // =========================================================
    // RELEASE INTO PHYSICS WORLD
    // =========================================================

    public void ReleaseToWorld(
        bool enablePhysics)
    {
        if (!IsCarried)
            return;

        if (movementRoutine != null)
        {
            StopCoroutine(
                movementRoutine
            );

            movementRoutine =
                null;
        }

        PlayerCarryController oldCarrier =
            Carrier;

        Carrier =
            null;

        carryPoint =
            null;

        presentationTarget =
            null;

        IsCarried =
            false;

        IsPresented =
            false;

        transform.SetParent(
            null,
            true
        );

        if (itemColliders != null)
        {
            foreach (Collider col
                     in itemColliders)
            {
                if (col != null)
                {
                    col.enabled =
                        true;
                }
            }
        }

        usingWorldPhysics =
            enablePhysics;

        if (rb != null)
        {
            rb.linearVelocity =
                Vector3.zero;

            rb.angularVelocity =
                Vector3.zero;

            rb.useGravity =
                enablePhysics;

            rb.isKinematic =
                !enablePhysics;

            if (enablePhysics)
            {
                rb.WakeUp();
            }
        }

        if (!enablePhysics)
        {
            worldFloatBasePosition =
                transform.position;

            worldFloatTimer =
                0f;
        }

        if (oldCarrier != null)
        {
            oldCarrier.ReleaseItem(
                this
            );
        }
    }

    // =========================================================
    // PICKUP COOLDOWN
    // =========================================================

    public void PreventPickupFor(
        float duration)
    {
        nextAllowedPickupTime =
            Mathf.Max(
                nextAllowedPickupTime,
                Time.time +
                Mathf.Max(
                    0f,
                    duration
                )
            );
    }
}