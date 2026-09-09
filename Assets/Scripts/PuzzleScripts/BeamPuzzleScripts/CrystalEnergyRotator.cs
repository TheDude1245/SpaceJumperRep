using System.Collections;
using UnityEngine;

public class CrystalEnergyRotator : MonoBehaviour, IInteractable
{
    [Header("Puzzle")]
    [SerializeField] private CrystalEnergyPuzzle puzzle;

    [Header("Rotation")]
    [SerializeField] private Transform rotationPivot;

    [Min(1)]
    [SerializeField] private int rotationSteps = 8;

    [SerializeField] private float rotationDuration = 0.15f;

    [SerializeField] private bool rotateClockwise = true;

    [Header("Beam References")]
    [SerializeField] private Transform beamOrigin;
    [SerializeField] private Transform beamVisual;
    [SerializeField] private Renderer beamRenderer;

    [Header("Beam Settings")]
    [SerializeField] private float maxBeamDistance = 20f;
    [SerializeField] private float beamMeshLength = 1f;
    [SerializeField] private float endPadding = 0.02f;
    [SerializeField] private LayerMask beamHitMask = ~0;

    public bool IsReceivingEnergy { get; private set; }
    public bool IsRotating { get; private set; }

    public bool HasHit { get; private set; }
    public RaycastHit LastHit { get; private set; }

    private Vector3 originalBeamScale;

    private Material beamMaterial;
    private Color originalBeamColor;

    private Component currentEnergyTarget;

    private Quaternion baseRotation;
    private int currentRotationStep;

    private bool completionFadeRunning;

    // =========================================================
    // UNITY
    // =========================================================

    private void Awake()
    {
        if (rotationPivot == null)
        {
            rotationPivot = transform;
        }

        baseRotation =
            rotationPivot.localRotation;

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
        IsRotating = false;

        currentRotationStep = 0;
    }

    private void Update()
    {
        if (completionFadeRunning)
            return;

        if (!IsReceivingEnergy)
            return;

        UpdateBeam();
    }

    // =========================================================
    // INTERACTION
    // =========================================================

    public void Interact(
        Transform interactor)
    {
        if (IsRotating)
            return;

        if (puzzle != null &&
            puzzle.IsCompleted)
        {
            return;
        }

        StartCoroutine(
            RotateOneStep()
        );
    }

    private IEnumerator RotateOneStep()
    {
        IsRotating = true;

        int stepDirection =
            rotateClockwise
                ? 1
                : -1;

        currentRotationStep +=
            stepDirection;

        currentRotationStep =
            ((currentRotationStep %
              rotationSteps)
             + rotationSteps)
            % rotationSteps;

        float degreesPerStep =
            360f /
            rotationSteps;

        float targetY =
            currentRotationStep *
            degreesPerStep *
            (rotateClockwise
                ? 1f
                : -1f);

        Quaternion startRotation =
            rotationPivot.localRotation;

        Quaternion targetRotation =
            baseRotation *
            Quaternion.Euler(
                0f,
                targetY,
                0f
            );

        if (rotationDuration <= 0f)
        {
            rotationPivot.localRotation =
                targetRotation;

            IsRotating = false;

            yield break;
        }

        float elapsed = 0f;

        while (elapsed <
               rotationDuration)
        {
            elapsed +=
                Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    elapsed /
                    rotationDuration
                );

            t =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    t
                );

            rotationPivot.localRotation =
                Quaternion.Slerp(
                    startRotation,
                    targetRotation,
                    t
                );

            yield return null;
        }

        rotationPivot.localRotation =
            targetRotation;

        IsRotating = false;
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
        CrystalEnergyRotator rotator =
            hitCollider.GetComponentInParent<
                CrystalEnergyRotator
            >();

        if (rotator == this)
        {
            rotator = null;
        }

        if (rotator != null)
        {
            SetEnergyTarget(
                rotator
            );

            return;
        }

        CrystalEnergyPushable pushable =
            hitCollider.GetComponentInParent<
                CrystalEnergyPushable
            >();

        if (pushable != null)
        {
            SetEnergyTarget(
                pushable
            );

            return;
        }

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