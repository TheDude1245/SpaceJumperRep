using UnityEngine;

public class PlayerInteractor : MonoBehaviour
{
    [Header("Interaction")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private float interactionRange = 2f;
    [SerializeField] private float maxInteractionAngle = 70f;
    [SerializeField] private LayerMask interactableLayers;

    private void Update()
    {
        if (Input.GetKeyDown(interactKey))
        {
            TryInteract();
        }
    }

    private void TryInteract()
    {
        Collider[] nearbyColliders = Physics.OverlapSphere(
            transform.position,
            interactionRange,
            interactableLayers,
            QueryTriggerInteraction.Collide
        );

        IInteractable bestInteractable = null;
        Transform bestTransform = null;
        float bestDistance = Mathf.Infinity;

        foreach (Collider col in nearbyColliders)
        {
            IInteractable interactable =
                col.GetComponentInParent<IInteractable>();

            if (interactable == null)
                continue;

            Transform targetTransform = col.transform;

            Vector3 directionToTarget =
                targetTransform.position - transform.position;

            directionToTarget.y = 0f;

            if (directionToTarget.sqrMagnitude <= 0.001f)
                continue;

            float angle = Vector3.Angle(
                transform.forward,
                directionToTarget
            );

            if (angle > maxInteractionAngle)
                continue;

            float distance = directionToTarget.sqrMagnitude;

            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestInteractable = interactable;
                bestTransform = targetTransform;
            }
        }

        if (bestInteractable != null)
        {
            bestInteractable.Interact(transform);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}