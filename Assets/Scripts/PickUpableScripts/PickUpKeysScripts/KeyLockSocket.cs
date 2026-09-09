using System.Collections;
using UnityEngine;

public class KeyLockSocket : MonoBehaviour
{
    [Header("References")]
    [Tooltip(
        "Exact position and rotation where the key should sit."
    )]
    [SerializeField] private Transform insertPoint;

    [Tooltip(
        "Transform that rotates when the key is turned."
    )]
    [SerializeField] private Transform turnPivot;

    [SerializeField] private KeyDoor keyDoor;

    [Header("Turning")]
    [Tooltip(
        "Local axis the key rotates around. " +
        "Examples: X = (1,0,0), Y = (0,1,0), Z = (0,0,1)"
    )]
    [SerializeField]
    private Vector3 turnAxis =
        Vector3.forward;

    [SerializeField]
    private float turnDegrees =
        90f;

    [SerializeField]
    private float turnDuration =
        0.35f;

    public Transform InsertPoint =>
        insertPoint;

    public bool IsUnlocked
    {
        get;
        private set;
    }

    public bool HasPresentedKey
    {
        get;
        private set;
    }

    private CarryableKey presentedKey;

    private bool isTurning;

    // =========================================================
    // UNITY
    // =========================================================

    private void Awake()
    {
        if (turnPivot == null)
        {
            turnPivot =
                insertPoint;
        }

        if (keyDoor == null)
        {
            keyDoor =
                GetComponentInParent<KeyDoor>();
        }
    }

    // =========================================================
    // PRESENT KEY
    // =========================================================

    public bool TryPresentKey(
        CarryableKey key)
    {
        if (key == null)
            return false;

        if (insertPoint == null)
        {
            Debug.LogWarning(
                name +
                " has no Key Insert Point assigned."
            );

            return false;
        }

        if (IsUnlocked ||
            HasPresentedKey ||
            isTurning)
        {
            return false;
        }

        presentedKey =
            key;

        HasPresentedKey =
            true;

        key.PresentToSocket(
            this
        );

        return true;
    }

    // =========================================================
    // CANCEL PRESENTATION
    // =========================================================

    public void CancelPresentedKey(
        CarryableKey key)
    {
        if (IsUnlocked ||
            isTurning)
        {
            return;
        }

        if (presentedKey != key)
            return;

        presentedKey =
            null;

        HasPresentedKey =
            false;

        key.ReturnToPlayer();
    }

    // =========================================================
    // Q
    // =========================================================

    public void TryUseKey(
        CarryableKey key)
    {
        if (IsUnlocked ||
            isTurning)
        {
            return;
        }

        if (!HasPresentedKey)
            return;

        if (presentedKey != key)
            return;

        StartCoroutine(
            TurnKeyRoutine(key)
        );
    }

    // =========================================================
    // TURN KEY
    // =========================================================

    private IEnumerator TurnKeyRoutine(
        CarryableKey key)
    {
        isTurning =
            true;

        HasPresentedKey =
            false;

        presentedKey =
            null;

        if (turnPivot == null)
        {
            turnPivot =
                insertPoint;
        }

        if (turnPivot == null)
        {
            Debug.LogWarning(
                name +
                " has no Turn Pivot or Insert Point."
            );

            isTurning =
                false;

            yield break;
        }

        Vector3 axis =
            turnAxis.normalized;

        if (axis.sqrMagnitude <
            0.001f)
        {
            axis =
                Vector3.forward;
        }

        Quaternion startRotation =
            turnPivot.localRotation;

        Quaternion targetRotation =
            startRotation *
            Quaternion.AngleAxis(
                turnDegrees,
                axis
            );

        if (turnDuration > 0f)
        {
            float elapsed =
                0f;

            while (elapsed <
                   turnDuration)
            {
                elapsed +=
                    Time.deltaTime;

                float t =
                    Mathf.Clamp01(
                        elapsed /
                        turnDuration
                    );

                t =
                    Mathf.SmoothStep(
                        0f,
                        1f,
                        t
                    );

                turnPivot.localRotation =
                    Quaternion.Slerp(
                        startRotation,
                        targetRotation,
                        t
                    );

                yield return null;
            }
        }

        turnPivot.localRotation =
            targetRotation;

        key.CommitToSocket(
            insertPoint
        );

        IsUnlocked =
            true;

        isTurning =
            false;

        if (keyDoor != null)
        {
            keyDoor.NotifyLockUnlocked(
                this
            );
        }
    }
}