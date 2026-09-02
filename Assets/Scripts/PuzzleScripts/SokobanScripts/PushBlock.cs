using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class PushBlock : MonoBehaviour, IInteractable
{
    [Header("Movement")]
    [SerializeField] private float pushDistance = 2f;
    [SerializeField] private float pushDuration = 0.25f;

    [Header("Collision")]
    [SerializeField] private float collisionSkin = 0.05f;

    private Rigidbody rb;
    private bool isMoving;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        rb.isKinematic = true;
        rb.useGravity = false;
    }

    public void Interact(Transform interactor)
    {
        if (isMoving)
            return;

        Vector3 pushDirection = GetPushDirection(interactor);

        if (pushDirection == Vector3.zero)
            return;

        if (!CanMove(pushDirection))
            return;

        Vector3 targetPosition =
            transform.position + pushDirection * pushDistance;

        StartCoroutine(MoveBlock(targetPosition));
    }

    private Vector3 GetPushDirection(Transform interactor)
    {
        Vector3 directionToPlayer =
            interactor.position - transform.position;

        directionToPlayer.y = 0f;

        if (directionToPlayer.sqrMagnitude <= 0.001f)
            return Vector3.zero;

        // Convert the player's position into the block's
        // local coordinate system.
        Vector3 localDirection =
            transform.InverseTransformDirection(directionToPlayer);

        // Determine which side of the block the player is on.
        if (Mathf.Abs(localDirection.x) >
            Mathf.Abs(localDirection.z))
        {
            // Player is on left/right side.
            if (localDirection.x > 0f)
                return -transform.right;

            return transform.right;
        }
        else
        {
            // Player is on front/back side.
            if (localDirection.z > 0f)
                return -transform.forward;

            return transform.forward;
        }
    }

    private bool CanMove(Vector3 direction)
    {
        float checkDistance =
            Mathf.Max(0f, pushDistance - collisionSkin);

        if (rb.SweepTest(
            direction,
            out RaycastHit hit,
            checkDistance,
            QueryTriggerInteraction.Ignore))
        {
            return false;
        }

        return true;
    }

    private IEnumerator MoveBlock(Vector3 targetPosition)
    {
        isMoving = true;

        Vector3 startPosition = transform.position;
        float elapsed = 0f;

        while (elapsed < pushDuration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(
                elapsed / pushDuration
            );

            // Smooth start and stop.
            t = Mathf.SmoothStep(0f, 1f, t);

            transform.position =
                Vector3.Lerp(
                    startPosition,
                    targetPosition,
                    t
                );

            yield return null;
        }

        transform.position = targetPosition;

        isMoving = false;
    }
}