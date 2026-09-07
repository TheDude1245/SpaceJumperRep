using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class PushBlock : MonoBehaviour, IInteractable
{
    [Header("Snap Rail")]
    [SerializeField] private PushRail rail;
    [SerializeField] private PushRailPoint startingPoint;

    [Header("Movement")]
    [Tooltip(
        "The AVERAGE movement speed of the push. " +
        "The block starts faster than this and slows down as it travels."
    )]
    [SerializeField] private float pushSpeed = 4f;

    [Tooltip(
        "Controls how strongly the block loses momentum during a push.\n\n" +
        "0 = constant speed.\n" +
        "0.5 = recommended Skylanders-style push. " +
        "The first 50% of the journey happens in about 29% of the total travel time.\n" +
        "Higher values create an even harder initial shove."
    )]
    [Range(0f, 0.9f)]
    [SerializeField] private float decelerationExponent = 0.5f;

    [Tooltip(
        "How strongly the block corrects sideways drift while travelling."
    )]
    [SerializeField] private float centeringStrength = 5f;

    [Tooltip(
        "How close horizontally the block must get to the next point."
    )]
    [SerializeField] private float stopDistance = 0.08f;

    [Header("Landing")]
    [SerializeField] private float settledVerticalSpeed = 0.08f;
    [SerializeField] private float minimumSettleTime = 0.1f;
    [SerializeField] private float maximumSettleTime = 3f;

    [Header("Interaction Proximity")]
    [SerializeField] private InteractablePrompt interactionPrompt;

    [Header("Direction Arrows")]
    [Tooltip("Parent object containing all four directional arrows.")]
    [SerializeField] private Transform arrowGroup;

    [SerializeField] private GameObject arrowNorth;
    [SerializeField] private GameObject arrowSouth;
    [SerializeField] private GameObject arrowEast;
    [SerializeField] private GameObject arrowWest;

    [Header("Arrow Pop Animation")]
    [Tooltip("How quickly the arrows grow to full size.")]
    [SerializeField] private float arrowAppearDuration = 0.12f;

    [Tooltip("How quickly the arrows shrink away.")]
    [SerializeField] private float arrowDisappearDuration = 0.08f;

    [Range(0f, 1f)]
    [SerializeField] private float arrowStartingScale = 0.05f;

    private Rigidbody rb;

    private PushRailPoint currentPoint;
    private PushRailPoint destinationPoint;

    private bool isMoving;
    private bool isLocked;

    // Arrow animation
    private Vector3 arrowFullScale;
    private float arrowVisibility;
    private bool arrowsShouldShow;
    private bool hasVisibleArrow;

    // =========================================================
    // UNITY
    // =========================================================

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        rb.isKinematic = false;
        rb.useGravity = true;

        rb.interpolation =
            RigidbodyInterpolation.Interpolate;

        rb.collisionDetectionMode =
            CollisionDetectionMode.Continuous;

        SetIdleConstraints();

        if (arrowGroup == null &&
            arrowNorth != null)
        {
            arrowGroup =
                arrowNorth.transform.parent;
        }

        if (arrowGroup != null)
        {
            arrowFullScale =
                arrowGroup.localScale;

            arrowVisibility = 0f;

            arrowGroup.localScale =
                arrowFullScale *
                arrowStartingScale;
        }

        DisableAllArrowObjects();
    }

    private void OnEnable()
    {
        if (interactionPrompt != null)
        {
            interactionPrompt.ProximityChanged +=
                HandleProximityChanged;
        }
    }

    private void Start()
    {
        currentPoint = startingPoint;

        if (rail == null)
        {
            Debug.LogError(
                name + " has no PushRail assigned."
            );

            HideAllArrows();
            return;
        }

        if (currentPoint == null)
        {
            Debug.LogError(
                name + " has no Starting Point assigned."
            );

            HideAllArrows();
            return;
        }

        if (!currentPoint.TryOccupy(this))
        {
            Debug.LogError(
                currentPoint.name +
                " is already occupied by another PushBlock."
            );

            HideAllArrows();
            return;
        }

        /*
         * Align the block to its starting rail point.
         *
         * Only X/Z are changed.
         * Terrain/gravity still determine Y.
         */
        Vector3 position =
            rb.position;

        position.x =
            currentPoint.transform.position.x;

        position.z =
            currentPoint.transform.position.z;

        rb.position =
            position;

        StopHorizontalMovement();
        SetIdleConstraints();

        UpdateArrows();
    }

    private void Update()
    {
        UpdateArrowAnimation();
    }

    private void OnDisable()
    {
        if (interactionPrompt != null)
        {
            interactionPrompt.ProximityChanged -=
                HandleProximityChanged;
        }

        ResetArrowAnimationImmediately();
    }

    // =========================================================
    // PROXIMITY
    // =========================================================

    private void HandleProximityChanged(
        bool inRange)
    {
        if (inRange)
        {
            UpdateArrows();
        }
        else
        {
            HideAllArrows();
        }
    }

    // =========================================================
    // INTERACTION
    // =========================================================

    public void Interact(
        Transform interactor)
    {
        if (isMoving ||
            isLocked)
        {
            return;
        }

        if (interactor == null ||
            currentPoint == null)
        {
            return;
        }

        PushRailPoint target =
            FindPushDestination(
                interactor
            );

        if (target == null)
            return;

        if (!target.CanEnter(this))
            return;

        StartCoroutine(
            PushToPoint(target)
        );
    }

    // =========================================================
    // FIND PUSH DIRECTION
    // =========================================================

    private PushRailPoint FindPushDestination(
        Transform interactor)
    {
        /*
         * Push direction is AWAY from
         * the player.
         */
        Vector3 desiredPushDirection =
            transform.position -
            interactor.position;

        desiredPushDirection.y = 0f;

        if (desiredPushDirection.sqrMagnitude <
            0.001f)
        {
            return null;
        }

        desiredPushDirection.Normalize();

        PushRailPoint bestPoint = null;

        float bestAlignment = 0.7f;

        foreach (
            PushRailPoint point
            in currentPoint.ConnectedPoints)
        {
            if (point == null)
                continue;

            Vector3 directionToPoint =
                point.transform.position -
                currentPoint.transform.position;

            directionToPoint.y = 0f;

            if (directionToPoint.sqrMagnitude <
                0.001f)
            {
                continue;
            }

            directionToPoint.Normalize();

            float alignment =
                Vector3.Dot(
                    desiredPushDirection,
                    directionToPoint
                );

            if (alignment >
                bestAlignment)
            {
                bestAlignment =
                    alignment;

                bestPoint =
                    point;
            }
        }

        return bestPoint;
    }

    // =========================================================
    // PHYSICAL PUSH
    // =========================================================

    private IEnumerator PushToPoint(
        PushRailPoint target)
    {
        isMoving = true;

        // Arrows shrink away while moving.
        HideAllArrows();

        destinationPoint =
            target;

        if (!destinationPoint.TryOccupy(this))
        {
            destinationPoint = null;
            isMoving = false;

            SetIdleConstraints();
            UpdateArrows();

            yield break;
        }

        PushRailPoint oldPoint =
            currentPoint;

        Vector3 start =
            oldPoint.transform.position;

        Vector3 end =
            destinationPoint.transform.position;

        Vector3 movementDirection =
            end - start;

        /*
         * The rail only controls horizontal
         * direction.
         *
         * Y remains physical.
         */
        movementDirection.y = 0f;

        if (movementDirection.sqrMagnitude <
            0.001f)
        {
            destinationPoint.Leave(this);

            destinationPoint = null;
            isMoving = false;

            SetIdleConstraints();
            UpdateArrows();

            yield break;
        }

        // Total horizontal rail distance.
        float totalDistance =
            movementDirection.magnitude;

        movementDirection.Normalize();

        SetMovingConstraints();

        oldPoint.Leave(this);

        // =====================================================
        // TRAVEL
        // =====================================================

        while (true)
        {
            Vector3 blockHorizontal =
                new Vector3(
                    rb.position.x,
                    0f,
                    rb.position.z
                );

            Vector3 destinationHorizontal =
                new Vector3(
                    end.x,
                    0f,
                    end.z
                );

            Vector3 toDestination =
                destinationHorizontal -
                blockHorizontal;

            float remainingDistance =
                Vector3.Dot(
                    toDestination,
                    movementDirection
                );

            if (remainingDistance <=
                stopDistance)
            {
                break;
            }

            // =================================================
            // MOMENTUM / DECELERATION
            // =================================================

            /*
             * How much of the journey remains?
             *
             * Start:
             * remainingFraction = 1
             *
             * End:
             * remainingFraction = 0
             */
            float remainingFraction =
                Mathf.Clamp01(
                    remainingDistance /
                    totalDistance
                );

            /*
             * Clamp for safety.
             *
             * Values >= 1 would mathematically
             * create an infinite stopping time.
             */
            float exponent =
                Mathf.Clamp(
                    decelerationExponent,
                    0f,
                    0.9f
                );

            /*
             * This normalization is important.
             *
             * It means pushSpeed still represents
             * the approximate AVERAGE speed.
             *
             * So changing from constant movement
             * to decelerating movement does NOT
             * massively change the total travel time.
             */
            float initialSpeedMultiplier =
                1f /
                (1f - exponent);

            /*
             * Example with exponent = 0.5:
             *
             * Start:
             * speed = pushSpeed * 2
             *
             * Then the block progressively loses
             * momentum as it approaches the target.
             */
            float currentPushSpeed =
                pushSpeed *
                initialSpeedMultiplier *
                Mathf.Pow(
                    remainingFraction,
                    exponent
                );

            Vector3 horizontalVelocity =
                movementDirection *
                currentPushSpeed;

            // =================================================
            // RAIL CENTERING
            // =================================================

            Vector3 railStart =
                new Vector3(
                    start.x,
                    0f,
                    start.z
                );

            Vector3 fromRailStart =
                blockHorizontal -
                railStart;

            float distanceAlongRail =
                Vector3.Dot(
                    fromRailStart,
                    movementDirection
                );

            Vector3 closestPointOnRail =
                railStart +
                movementDirection *
                distanceAlongRail;

            Vector3 sidewaysError =
                closestPointOnRail -
                blockHorizontal;

            horizontalVelocity +=
                sidewaysError *
                centeringStrength;

            // =================================================
            // APPLY HORIZONTAL PUSH
            // =================================================

            Vector3 velocity =
                rb.linearVelocity;

            velocity.x =
                horizontalVelocity.x;

            velocity.z =
                horizontalVelocity.z;

            /*
             * DO NOT touch velocity.y.
             *
             * Gravity, ramps, falling and terrain
             * remain completely physical.
             */
            rb.linearVelocity =
                velocity;

            yield return
                new WaitForFixedUpdate();
        }

        // =====================================================
        // HORIZONTAL ARRIVAL
        // =====================================================

        StopHorizontalMovement();

        /*
         * Correct tiny X/Z physics inaccuracies.
         */
        Vector3 finalPosition =
            rb.position;

        finalPosition.x =
            destinationPoint.transform.position.x;

        finalPosition.z =
            destinationPoint.transform.position.z;

        rb.position =
            finalPosition;

        SetIdleConstraints();

        // =====================================================
        // WAIT FOR VERTICAL PHYSICS
        // =====================================================

        float settleTimer = 0f;

        while (settleTimer <
               maximumSettleTime)
        {
            settleTimer +=
                Time.fixedDeltaTime;

            if (settleTimer >=
                    minimumSettleTime &&
                Mathf.Abs(
                    rb.linearVelocity.y
                ) <= settledVerticalSpeed)
            {
                break;
            }

            yield return
                new WaitForFixedUpdate();
        }

        StopHorizontalMovement();
        SetIdleConstraints();

        currentPoint =
            destinationPoint;

        destinationPoint =
            null;

        isMoving =
            false;

        rb.Sleep();

        /*
         * If the player is still close,
         * new available directions pop in.
         */
        UpdateArrows();
    }

    // =========================================================
    // PHYSICS STATES
    // =========================================================

    private void SetIdleConstraints()
    {
        /*
         * X/Z locked while idle so the player
         * cannot physically shove the block.
         *
         * Y stays free for gravity.
         */
        rb.constraints =
            RigidbodyConstraints.FreezePositionX |
            RigidbodyConstraints.FreezePositionZ |
            RigidbodyConstraints.FreezeRotationX |
            RigidbodyConstraints.FreezeRotationY |
            RigidbodyConstraints.FreezeRotationZ;
    }

    private void SetMovingConstraints()
    {
        /*
         * Position becomes free during a push.
         *
         * Rotation remains locked.
         */
        rb.constraints =
            RigidbodyConstraints.FreezeRotationX |
            RigidbodyConstraints.FreezeRotationY |
            RigidbodyConstraints.FreezeRotationZ;

        rb.WakeUp();
    }

    private void StopHorizontalMovement()
    {
        Vector3 velocity =
            rb.linearVelocity;

        velocity.x = 0f;
        velocity.z = 0f;

        rb.linearVelocity =
            velocity;
    }

    // =========================================================
    // FINAL TARGET LOCK
    // =========================================================

    public void LockInPlace()
    {
        if (isLocked)
            return;

        isLocked = true;

        StopHorizontalMovement();
        SetIdleConstraints();

        HideAllArrows();

        rb.Sleep();
    }

    // =========================================================
    // DIRECTION ARROWS
    // =========================================================

    private void UpdateArrows()
    {
        DisableAllArrowObjects();

        hasVisibleArrow = false;

        if (isLocked ||
            isMoving ||
            currentPoint == null ||
            rail == null)
        {
            HideAllArrows();
            return;
        }

        /*
         * Sokoban arrows only reveal when
         * the player is nearby.
         */
        if (interactionPrompt != null &&
            !interactionPrompt.IsInRange)
        {
            HideAllArrows();
            return;
        }

        foreach (
            PushRailPoint point
            in currentPoint.ConnectedPoints)
        {
            if (point == null)
                continue;

            /*
             * Don't show a direction if another
             * block currently occupies that point.
             */
            if (!point.CanEnter(this))
                continue;

            Vector3 direction =
                point.transform.position -
                currentPoint.transform.position;

            direction.y = 0f;

            if (direction.sqrMagnitude <
                0.001f)
            {
                continue;
            }

            direction.Normalize();

            float forward =
                Vector3.Dot(
                    direction,
                    rail.Forward
                );

            float right =
                Vector3.Dot(
                    direction,
                    rail.Right
                );

            if (Mathf.Abs(forward) >
                Mathf.Abs(right))
            {
                if (forward > 0f)
                {
                    EnableArrow(
                        arrowNorth
                    );
                }
                else
                {
                    EnableArrow(
                        arrowSouth
                    );
                }
            }
            else
            {
                if (right > 0f)
                {
                    EnableArrow(
                        arrowEast
                    );
                }
                else
                {
                    EnableArrow(
                        arrowWest
                    );
                }
            }
        }

        arrowsShouldShow =
            hasVisibleArrow;
    }

    private void EnableArrow(
        GameObject arrow)
    {
        if (arrow == null)
            return;

        arrow.SetActive(true);

        hasVisibleArrow = true;
    }

    private void HideAllArrows()
    {
        /*
         * Do not instantly disable them.
         * Let the scale animation shrink
         * them away first.
         */
        arrowsShouldShow = false;
    }

    private void DisableAllArrowObjects()
    {
        if (arrowNorth != null)
            arrowNorth.SetActive(false);

        if (arrowSouth != null)
            arrowSouth.SetActive(false);

        if (arrowEast != null)
            arrowEast.SetActive(false);

        if (arrowWest != null)
            arrowWest.SetActive(false);
    }

    // =========================================================
    // ARROW POP ANIMATION
    // =========================================================

    private void UpdateArrowAnimation()
    {
        if (arrowGroup == null)
            return;

        float target =
            arrowsShouldShow
                ? 1f
                : 0f;

        float duration =
            arrowsShouldShow
                ? arrowAppearDuration
                : arrowDisappearDuration;

        if (duration <= 0f)
        {
            arrowVisibility =
                target;
        }
        else
        {
            arrowVisibility =
                Mathf.MoveTowards(
                    arrowVisibility,
                    target,
                    Time.deltaTime /
                    duration
                );
        }

        float smoothVisibility =
            Mathf.SmoothStep(
                0f,
                1f,
                arrowVisibility
            );

        float scaleFactor =
            Mathf.Lerp(
                arrowStartingScale,
                1f,
                smoothVisibility
            );

        arrowGroup.localScale =
            arrowFullScale *
            scaleFactor;

        if (!arrowsShouldShow &&
            arrowVisibility <= 0f)
        {
            DisableAllArrowObjects();
        }
    }

    private void ResetArrowAnimationImmediately()
    {
        arrowsShouldShow = false;
        hasVisibleArrow = false;
        arrowVisibility = 0f;

        DisableAllArrowObjects();

        if (arrowGroup != null)
        {
            arrowGroup.localScale =
                arrowFullScale *
                arrowStartingScale;
        }
    }
}