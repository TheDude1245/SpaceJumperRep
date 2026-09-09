using System.Collections;
using UnityEngine;

public class CrystalEnergyPushable : MonoBehaviour
{
    [Header("Puzzle")]
    [SerializeField] private CrystalEnergyPuzzle puzzle;

    [Header("Push Movement")]
    [Tooltip(
        "The existing PushBlock component that controls " +
        "rail movement, arrows and physical pushing."
    )]
    [SerializeField] private PushBlock pushBlock;

    [Header("Beam References")]
    [Tooltip(
        "Where this crystal's outgoing beam begins."
    )]
    [SerializeField] private Transform beamOrigin;

    [Tooltip(
        "The tapered 3D laser model."
    )]
    [SerializeField] private Transform beamVisual;

    [Tooltip(
        "Renderer used for completion fading."
    )]
    [SerializeField] private Renderer beamRenderer;

    [Header("Beam Settings")]
    [SerializeField] private float maxBeamDistance = 20f;

    [Tooltip(
        "Original Z length of the beam mesh."
    )]
    [SerializeField] private float beamMeshLength = 1f;

    [SerializeField] private float endPadding = 0.02f;

    [SerializeField] private LayerMask beamHitMask = ~0;

    public bool IsReceivingEnergy { get; private set; }

    public bool HasHit { get; private set; }

    public RaycastHit LastHit { get; private set; }

    private Vector3 originalBeamScale;

    private Material beamMaterial;
    private Color originalBeamColor;

    private Component currentEnergyTarget;

    private bool completionFadeRunning;
    private bool movementLockedAfterCompletion;

    // =========================================================
    // UNITY
    // =========================================================

    private void Awake()
    {
        if (pushBlock == null)
        {
            pushBlock =
                GetComponent<PushBlock>();
        }

        if (beamVisual != null)
        {
            originalBeamScale =
                beamVisual.localScale;

            beamVisual.gameObject.SetActive(
                false
            );
        }

        if (beamRenderer == null &&
            beamVisual != null)
        {
            beamRenderer =
                beamVisual.GetComponentInChildren<
                    Renderer
                >();
        }

        if (beamRenderer != null)
        {
            beamMaterial =
                beamRenderer.material;

            originalBeamColor =
                beamMaterial.color;

            SetBeamAlpha(1f);
        }

        IsReceivingEnergy = false;
    }

    private void Update()
    {
        /*
         * Once the puzzle is solved,
         * this crystal can no longer be pushed.
         */
        if (!movementLockedAfterCompletion &&
            puzzle != null &&
            puzzle.IsCompleted)
        {
            movementLockedAfterCompletion =
                true;

            if (pushBlock != null)
            {
                pushBlock.LockInPlace();
            }
        }

        if (completionFadeRunning)
            return;

        if (!IsReceivingEnergy)
            return;

        UpdateBeam();
    }

    // =========================================================
    // RECEIVE ENERGY
    // =========================================================

    public void ReceiveEnergy()
    {
        if (IsReceivingEnergy)
            return;

        IsReceivingEnergy = true;

        SetBeamAlpha(1f);

        if (beamVisual != null)
        {
            beamVisual.gameObject.SetActive(
                true
            );
        }
    }

    public void StopReceivingEnergy()
    {
        if (completionFadeRunning)
            return;

        if (!IsReceivingEnergy)
            return;

        IsReceivingEnergy = false;

        ClearEnergyTarget();

        HasHit = false;
        LastHit = default;

        if (beamVisual != null)
        {
            beamVisual.gameObject.SetActive(
                false
            );
        }
    }

    // =========================================================
    // BEAM
    // =========================================================

    private void UpdateBeam()
    {
        if (beamOrigin == null ||
            beamVisual == null)
        {
            return;
        }

        Vector3 origin =
            beamOrigin.position;

        Vector3 direction =
            beamOrigin.forward;

        if (Physics.Raycast(
                origin,
                direction,
                out RaycastHit hit,
                maxBeamDistance,
                beamHitMask,
                QueryTriggerInteraction.Ignore))
        {
            HasHit = true;

            LastHit = hit;

            float beamLength =
                Mathf.Max(
                    0f,
                    hit.distance -
                    endPadding
                );

            SetBeamLength(
                beamLength
            );

            CheckEnergyTarget(
                hit.collider
            );
        }
        else
        {
            HasHit = false;

            LastHit = default;

            SetBeamLength(
                maxBeamDistance
            );

            ClearEnergyTarget();
        }
    }

    // =========================================================
    // ENERGY TARGET
    // =========================================================

    private void CheckEnergyTarget(
        Collider hitCollider)
    {
        // -----------------------------------------
        // Rotating crystal
        // -----------------------------------------

        CrystalEnergyRotator rotator =
            hitCollider.GetComponentInParent<
                CrystalEnergyRotator
            >();

        if (rotator != null)
        {
            SetEnergyTarget(
                rotator
            );

            return;
        }

        // -----------------------------------------
        // Another pushable crystal
        // -----------------------------------------

        CrystalEnergyPushable pushable =
            hitCollider.GetComponentInParent<
                CrystalEnergyPushable
            >();

        if (pushable == this)
        {
            pushable = null;
        }

        if (pushable != null)
        {
            SetEnergyTarget(
                pushable
            );

            return;
        }

        // -----------------------------------------
        // Final receiver
        // -----------------------------------------

        CrystalEnergyReceiver receiver =
            hitCollider.GetComponentInParent<
                CrystalEnergyReceiver
            >();

        if (receiver != null)
        {
            SetEnergyTarget(
                receiver
            );

            return;
        }

        // Ordinary geometry.
        ClearEnergyTarget();
    }

    private void SetEnergyTarget(
        Component newTarget)
    {
        if (currentEnergyTarget ==
            newTarget)
        {
            return;
        }

        ClearEnergyTarget();

        currentEnergyTarget =
            newTarget;

        if (currentEnergyTarget
            is CrystalEnergyRotator rotator)
        {
            rotator.ReceiveEnergy();
        }
        else if (currentEnergyTarget
                 is CrystalEnergyPushable pushable)
        {
            pushable.ReceiveEnergy();
        }
        else if (currentEnergyTarget
                 is CrystalEnergyReceiver receiver)
        {
            receiver.ReceiveEnergy();
        }
    }

    private void ClearEnergyTarget()
    {
        if (currentEnergyTarget == null)
            return;

        if (currentEnergyTarget
            is CrystalEnergyRotator rotator)
        {
            rotator.StopReceivingEnergy();
        }
        else if (currentEnergyTarget
                 is CrystalEnergyPushable pushable)
        {
            pushable.StopReceivingEnergy();
        }
        else if (currentEnergyTarget
                 is CrystalEnergyReceiver receiver)
        {
            receiver.StopReceivingEnergy();
        }

        currentEnergyTarget = null;
    }

    // =========================================================
    // BEAM LENGTH
    // =========================================================

    private void SetBeamLength(
        float length)
    {
        if (beamVisual == null)
            return;

        float safeMeshLength =
            Mathf.Max(
                beamMeshLength,
                0.0001f
            );

        Vector3 scale =
            originalBeamScale;

        scale.z =
            length /
            safeMeshLength;

        beamVisual.localScale =
            scale;

        Vector3 position =
            beamVisual.localPosition;

        position.x = 0f;
        position.y = 0f;

        position.z =
            length * 0.5f;

        beamVisual.localPosition =
            position;
    }

    // =========================================================
    // COMPLETION FADE
    // =========================================================

    public void BeginCompletionFade(
        float holdTime,
        float fadeDuration)
    {
        if (completionFadeRunning ||
            !IsReceivingEnergy)
        {
            return;
        }

        /*
         * Pass completion down the currently
         * active beam chain.
         */
        if (currentEnergyTarget
            is CrystalEnergyRotator rotator)
        {
            rotator.BeginCompletionFade(
                holdTime,
                fadeDuration
            );
        }
        else if (currentEnergyTarget
                 is CrystalEnergyPushable pushable)
        {
            pushable.BeginCompletionFade(
                holdTime,
                fadeDuration
            );
        }

        StartCoroutine(
            CompletionFadeRoutine(
                holdTime,
                fadeDuration
            )
        );
    }

    private IEnumerator CompletionFadeRoutine(
        float holdTime,
        float fadeDuration)
    {
        completionFadeRunning = true;

        /*
         * Stop recalculating this beam while
         * the completed path fades.
         */

        if (holdTime > 0f)
        {
            yield return new WaitForSeconds(
                holdTime
            );
        }

        if (fadeDuration <= 0f)
        {
            SetBeamAlpha(0f);
        }
        else
        {
            float elapsed = 0f;

            while (elapsed <
                   fadeDuration)
            {
                elapsed +=
                    Time.deltaTime;

                float t =
                    Mathf.Clamp01(
                        elapsed /
                        fadeDuration
                    );

                float smoothT =
                    Mathf.SmoothStep(
                        0f,
                        1f,
                        t
                    );

                float alpha =
                    Mathf.Lerp(
                        1f,
                        0f,
                        smoothT
                    );

                SetBeamAlpha(
                    alpha
                );

                yield return null;
            }

            SetBeamAlpha(0f);
        }

        IsReceivingEnergy = false;

        currentEnergyTarget = null;

        if (beamVisual != null)
        {
            beamVisual.gameObject.SetActive(
                false
            );
        }

        completionFadeRunning = false;
    }

    // =========================================================
    // ALPHA
    // =========================================================

    private void SetBeamAlpha(
        float alpha)
    {
        if (beamMaterial == null)
            return;

        Color color =
            originalBeamColor;

        color.a =
            Mathf.Clamp01(
                alpha
            );

        beamMaterial.color =
            color;
    }

    // =========================================================
    // DEBUG
    // =========================================================

    private void OnDrawGizmosSelected()
    {
        if (beamOrigin == null)
            return;

        Gizmos.DrawLine(
            beamOrigin.position,
            beamOrigin.position +
            beamOrigin.forward *
            maxBeamDistance
        );
    }
}