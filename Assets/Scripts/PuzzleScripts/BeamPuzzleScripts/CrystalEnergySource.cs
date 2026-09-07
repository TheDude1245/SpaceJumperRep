using System.Collections;
using UnityEngine;

public class CrystalEnergySource : MonoBehaviour
{
    [Header("Beam References")]
    [Tooltip(
        "Where the beam begins and which direction it fires."
    )]
    [SerializeField] private Transform beamOrigin;

    [Tooltip(
        "The tapered 3D beam model."
    )]
    [SerializeField] private Transform beamVisual;

    [Tooltip(
        "Renderer used to fade the laser. " +
        "If left empty, the script will try to find one automatically."
    )]
    [SerializeField] private Renderer beamRenderer;

    [Header("Beam Settings")]
    [SerializeField] private float maxBeamDistance = 20f;

    [Tooltip(
        "Original Z length of the beam mesh."
    )]
    [SerializeField] private float beamMeshLength = 1f;

    [Tooltip(
        "Prevents the visible beam from clipping into whatever it hits."
    )]
    [SerializeField] private float endPadding = 0.02f;

    [SerializeField] private LayerMask beamHitMask = ~0;

    [Header("State")]
    [SerializeField] private bool startActive = true;

    public bool IsActive { get; private set; }

    public bool HasHit { get; private set; }

    public RaycastHit LastHit { get; private set; }

    private Vector3 originalBeamScale;

    private CrystalEnergyReceiver currentReceiver;

    private bool completionFadeRunning;

    /*
     * We create a local material instance so fading this
     * beam does NOT change every other object using the
     * same material.
     */
    private Material beamMaterial;

    private Color originalBeamColor;

    // =========================================================
    // UNITY
    // =========================================================

    private void Awake()
    {
        if (beamVisual != null)
        {
            originalBeamScale =
                beamVisual.localScale;
        }

        /*
         * Automatically find the renderer if
         * one wasn't assigned manually.
         */
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
            /*
             * .material gives this beam its own
             * material instance.
             */
            beamMaterial =
                beamRenderer.material;

            originalBeamColor =
                beamMaterial.color;
        }

        IsActive =
            startActive;

        SetBeamAlpha(1f);

        UpdateBeamVisibility();
    }

    private void Update()
    {
        /*
         * While fading after completion,
         * freeze the beam path exactly as it was.
         *
         * The coroutine now controls the visual.
         */
        if (completionFadeRunning)
            return;

        UpdateBeam();
    }

    // =========================================================
    // NORMAL BEAM
    // =========================================================

    private void UpdateBeam()
    {
        if (beamOrigin == null ||
            beamVisual == null)
        {
            return;
        }

        if (!IsActive)
        {
            ClearReceiver();

            HasHit = false;
            LastHit = default;

            UpdateBeamVisibility();

            return;
        }

        UpdateBeamVisibility();

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

            LastHit =
                hit;

            float beamLength =
                Mathf.Max(
                    0f,
                    hit.distance -
                    endPadding
                );

            SetBeamLength(
                beamLength
            );

            CheckReceiver(
                hit.collider
            );
        }
        else
        {
            HasHit = false;

            LastHit =
                default;

            SetBeamLength(
                maxBeamDistance
            );

            ClearReceiver();
        }
    }

    // =========================================================
    // RECEIVER
    // =========================================================

    private void CheckReceiver(
        Collider hitCollider)
    {
        CrystalEnergyReceiver receiver =
            hitCollider.GetComponentInParent<
                CrystalEnergyReceiver
            >();

        if (receiver ==
            currentReceiver)
        {
            return;
        }

        ClearReceiver();

        if (receiver != null)
        {
            currentReceiver =
                receiver;

            currentReceiver.ReceiveEnergy();
        }
    }

    private void ClearReceiver()
    {
        if (currentReceiver == null)
            return;

        currentReceiver.StopReceivingEnergy();

        currentReceiver = null;
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

        /*
         * Only change beam length.
         *
         * X and Y thickness remain exactly
         * as configured.
         */
        scale.z =
            length /
            safeMeshLength;

        beamVisual.localScale =
            scale;

        /*
         * Our beam model's origin is in its center.
         *
         * Therefore the visual must move forward by
         * half of its total length.
         */
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
        if (completionFadeRunning)
            return;

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
        completionFadeRunning =
            true;

        /*
         * IMPORTANT:
         *
         * We stop updating the raycast while
         * completionFadeRunning is true.
         *
         * Therefore the completed laser path
         * visually freezes in place.
         */

        if (holdTime > 0f)
        {
            yield return new WaitForSeconds(
                holdTime
            );
        }

        // =====================================================
        // FADE WHOLE BEAM
        // =====================================================

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

                /*
                 * Smooth rather than perfectly
                 * linear fading.
                 */
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

        // =====================================================
        // NOW IT IS INVISIBLE
        // =====================================================

        /*
         * Only now do we actually deactivate it.
         *
         * Player never sees the technical
         * "beam switching off".
         */
        IsActive = false;

        if (beamVisual != null)
        {
            beamVisual.gameObject.SetActive(
                false
            );
        }

        /*
         * Do NOT tell the completed receiver
         * to deactivate.
         *
         * It stays visually powered.
         */
        currentReceiver = null;

        completionFadeRunning =
            false;
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
    // MANUAL STATE
    // =========================================================

    public void SetBeamActive(
        bool active)
    {
        StopAllCoroutines();

        completionFadeRunning =
            false;

        IsActive =
            active;

        /*
         * If we ever reactivate the laser,
         * restore full opacity.
         */
        if (IsActive)
        {
            SetBeamAlpha(1f);
        }
        else
        {
            ClearReceiver();
        }

        UpdateBeamVisibility();
    }

    private void UpdateBeamVisibility()
    {
        if (beamVisual == null)
            return;

        beamVisual.gameObject.SetActive(
            IsActive
        );
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