using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class TimedSpikeHazard : MonoBehaviour
{
    [Header("References")]
    [Tooltip(
        "The object containing the actual moving spike mesh."
    )]
    [SerializeField] private Transform spikeMover;

    [Tooltip(
        "Position where the spikes are completely hidden."
    )]
    [SerializeField] private Transform safePoint;

    [Tooltip(
        "Position where the spikes are fully raised."
    )]
    [SerializeField] private Transform activePoint;

    [Tooltip(
        "Trigger responsible for damaging the player."
    )]
    [SerializeField] private SpikeDamageHitbox damageHitbox;

    [Header("Timing")]
    [Tooltip(
        "Delay before this trap starts its first cycle."
    )]
    [SerializeField] private float startOffset = 0f;

    [Tooltip(
        "How long the spikes remain completely safe."
    )]
    [SerializeField] private float safeDuration = 1.5f;

    [Tooltip(
        "Short warning period before the spikes rise."
    )]
    [SerializeField] private float warningDuration = 0.4f;

    [Tooltip(
        "How quickly the spikes shoot upward."
    )]
    [SerializeField] private float riseDuration = 0.15f;

    [Tooltip(
        "How long the spikes remain fully raised."
    )]
    [SerializeField] private float activeDuration = 1f;

    [Tooltip(
        "How quickly the spikes retract."
    )]
    [SerializeField] private float retractDuration = 0.25f;

    [Header("Behaviour")]
    [SerializeField] private bool loop = true;

    [Header("Events")]
    [SerializeField] private UnityEvent onWarning;
    [SerializeField] private UnityEvent onActivated;
    [SerializeField] private UnityEvent onRetracted;

    public bool IsActive { get; private set; }
    public bool IsWarning { get; private set; }

    private Coroutine cycleRoutine;

    // =========================================================
    // UNITY
    // =========================================================

    private void Start()
    {
        if (spikeMover == null ||
            safePoint == null ||
            activePoint == null)
        {
            Debug.LogWarning(
                name +
                " is missing SpikeMover, SafePoint or ActivePoint."
            );

            return;
        }

        /*
         * Start completely hidden and harmless.
         */
        spikeMover.position =
            safePoint.position;

        IsActive =
            false;

        IsWarning =
            false;

        if (damageHitbox != null)
        {
            damageHitbox.SetDangerous(
                false
            );
        }

        cycleRoutine =
            StartCoroutine(
                HazardCycle()
            );
    }

    // =========================================================
    // MAIN LOOP
    // =========================================================

    private IEnumerator HazardCycle()
    {
        // =====================================================
        // INITIAL OFFSET
        // =====================================================

        if (startOffset > 0f)
        {
            yield return new WaitForSeconds(
                startOffset
            );
        }

        do
        {
            // =================================================
            // SAFE
            // =================================================

            IsActive =
                false;

            IsWarning =
                false;

            spikeMover.position =
                safePoint.position;

            if (damageHitbox != null)
            {
                damageHitbox.SetDangerous(
                    false
                );
            }

            if (safeDuration > 0f)
            {
                yield return new WaitForSeconds(
                    safeDuration
                );
            }

            // =================================================
            // WARNING
            // =================================================

            IsWarning =
                true;

            onWarning?.Invoke();

            /*
             * Still harmless during warning.
             */
            if (damageHitbox != null)
            {
                damageHitbox.SetDangerous(
                    false
                );
            }

            if (warningDuration > 0f)
            {
                yield return new WaitForSeconds(
                    warningDuration
                );
            }

            IsWarning =
                false;

            // =================================================
            // RISING
            // =================================================

            /*
             * The instant the spikes begin shooting
             * upward, they become dangerous.
             */
            if (damageHitbox != null)
            {
                damageHitbox.SetDangerous(
                    true
                );
            }

            yield return MoveSpikes(
                safePoint.position,
                activePoint.position,
                riseDuration
            );

            // =================================================
            // ACTIVE
            // =================================================

            IsActive =
                true;

            onActivated?.Invoke();

            if (activeDuration > 0f)
            {
                yield return new WaitForSeconds(
                    activeDuration
                );
            }

            // =================================================
            // BEGIN RETRACTION
            // =================================================

            IsActive =
                false;

            /*
             * As soon as they begin retracting,
             * they're safe again.
             */
            if (damageHitbox != null)
            {
                damageHitbox.SetDangerous(
                    false
                );
            }

            // =================================================
            // RETRACT
            // =================================================

            yield return MoveSpikes(
                activePoint.position,
                safePoint.position,
                retractDuration
            );

            spikeMover.position =
                safePoint.position;

            onRetracted?.Invoke();
        }
        while (loop);

        cycleRoutine =
            null;
    }

    // =========================================================
    // MOVEMENT
    // =========================================================

    private IEnumerator MoveSpikes(
        Vector3 startPosition,
        Vector3 targetPosition,
        float duration)
    {
        if (duration <= 0f)
        {
            spikeMover.position =
                targetPosition;

            yield break;
        }

        float elapsed =
            0f;

        while (elapsed <
               duration)
        {
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

            spikeMover.position =
                Vector3.Lerp(
                    startPosition,
                    targetPosition,
                    smoothT
                );

            yield return null;
        }

        spikeMover.position =
            targetPosition;
    }

    // =========================================================
    // DEBUG
    // =========================================================

    private void OnDrawGizmosSelected()
    {
        if (safePoint != null)
        {
            Gizmos.DrawWireSphere(
                safePoint.position,
                0.15f
            );
        }

        if (activePoint != null)
        {
            Gizmos.DrawWireSphere(
                activePoint.position,
                0.15f
            );

            if (safePoint != null)
            {
                Gizmos.DrawLine(
                    safePoint.position,
                    activePoint.position
                );
            }
        }
    }
}