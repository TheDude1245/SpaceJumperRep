using System;
using UnityEngine;

public class InteractablePrompt : MonoBehaviour
{
    [Header("Prompt")]
    [SerializeField] private GameObject promptVisual;

    [Header("Scale Animation")]
    [Tooltip("How quickly the prompt grows to full size.")]
    [SerializeField] private float appearDuration = 0.12f;

    [Tooltip("How quickly the prompt shrinks away.")]
    [SerializeField] private float disappearDuration = 0.08f;

    [Tooltip("Starting size when the prompt begins appearing.")]
    [Range(0f, 1f)]
    [SerializeField] private float startingScale = 0.05f;

    [Header("Player Facing")]
    [Tooltip(
        "The object that rotates toward the player. " +
        "Normally this should be the World Space Canvas."
    )]
    [SerializeField] private Transform rotationTarget;

    [Tooltip(
        "If the Canvas faces backwards, try setting this to 180."
    )]
    [SerializeField] private float yRotationOffset = 0f;

    [Tooltip("How quickly the prompt rotates toward the player.")]
    [SerializeField] private float rotationSpeed = 720f;

    public bool IsInRange { get; private set; }

    public event Action<bool> ProximityChanged;

    private Vector3 fullScale;

    private Quaternion originalLocalRotation;
    private Vector3 originalLocalEuler;

    private Transform playerToFace;

    private float visibility;
    private bool shouldShow;

    private void Awake()
    {
        if (rotationTarget == null)
            rotationTarget = transform;

        originalLocalRotation =
            rotationTarget.localRotation;

        originalLocalEuler =
            originalLocalRotation.eulerAngles;

        if (promptVisual == null)
            return;

        fullScale =
            promptVisual.transform.localScale;

        visibility = 0f;
        shouldShow = false;
        IsInRange = false;

        promptVisual.transform.localScale =
            fullScale * startingScale;

        promptVisual.SetActive(false);
    }

    private void Update()
    {
        if (promptVisual == null)
            return;

        UpdateScaleAnimation();
        UpdateRotation();
    }

    // =========================================================
    // SCALE ANIMATION
    // =========================================================

    private void UpdateScaleAnimation()
    {
        float target =
            shouldShow ? 1f : 0f;

        float duration =
            shouldShow
                ? appearDuration
                : disappearDuration;

        if (duration <= 0f)
        {
            visibility = target;
        }
        else
        {
            visibility =
                Mathf.MoveTowards(
                    visibility,
                    target,
                    Time.deltaTime / duration
                );
        }

        float smoothVisibility =
            Mathf.SmoothStep(
                0f,
                1f,
                visibility
            );

        float scaleFactor =
            Mathf.Lerp(
                startingScale,
                1f,
                smoothVisibility
            );

        promptVisual.transform.localScale =
            fullScale * scaleFactor;

        if (!shouldShow &&
            visibility <= 0f)
        {
            promptVisual.SetActive(false);
        }
    }

    // =========================================================
    // ROTATE TOWARD PLAYER - Y AXIS ONLY
    // =========================================================

    private void UpdateRotation()
    {
        if (!shouldShow ||
            playerToFace == null ||
            rotationTarget == null)
        {
            return;
        }

        Vector3 direction =
            playerToFace.position -
            rotationTarget.position;

        // Ignore height difference completely.
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.001f)
            return;

        direction.Normalize();

        Quaternion worldLookRotation =
            Quaternion.LookRotation(
                direction,
                Vector3.up
            );

        Quaternion localLookRotation;

        if (rotationTarget.parent != null)
        {
            localLookRotation =
                Quaternion.Inverse(
                    rotationTarget.parent.rotation
                ) *
                worldLookRotation;
        }
        else
        {
            localLookRotation =
                worldLookRotation;
        }

        Vector3 lookEuler =
            localLookRotation.eulerAngles;

        /*
         * X and Z stay exactly as they were.
         * Only Y is allowed to rotate.
         */
        Quaternion targetRotation =
            Quaternion.Euler(
                originalLocalEuler.x,
                lookEuler.y + yRotationOffset,
                originalLocalEuler.z
            );

        rotationTarget.localRotation =
            Quaternion.RotateTowards(
                rotationTarget.localRotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
    }

    // =========================================================
    // SHOW / HIDE
    // =========================================================

    public void Show(Transform player)
    {
        if (promptVisual == null)
            return;

        playerToFace = player;
        shouldShow = true;

        if (!IsInRange)
        {
            IsInRange = true;

            ProximityChanged?.Invoke(true);
        }

        if (!promptVisual.activeSelf)
            promptVisual.SetActive(true);
    }

    public void Hide()
    {
        shouldShow = false;
        playerToFace = null;

        if (IsInRange)
        {
            IsInRange = false;

            ProximityChanged?.Invoke(false);
        }

        ResetRotation();
    }

    private void ResetRotation()
    {
        if (rotationTarget != null)
        {
            rotationTarget.localRotation =
                originalLocalRotation;
        }
    }

    private void OnDisable()
    {
        shouldShow = false;
        playerToFace = null;

        if (IsInRange)
        {
            IsInRange = false;

            ProximityChanged?.Invoke(false);
        }

        visibility = 0f;

        ResetRotation();

        if (promptVisual == null)
            return;

        promptVisual.transform.localScale =
            fullScale * startingScale;

        promptVisual.SetActive(false);
    }
}