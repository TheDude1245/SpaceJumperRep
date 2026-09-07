using System.Collections.Generic;
using UnityEngine;

public class PlayerInteractor : MonoBehaviour
{
    [Header("Interaction")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private float interactionRange = 2f;

    [Tooltip("Player must still face the object within this angle to actually interact with it.")]
    [SerializeField] private float maxInteractionAngle = 70f;

    [SerializeField] private LayerMask interactableLayers;

    private IInteractable currentInteractable;

    // Prompts that were visible on the previous frame.
    private readonly HashSet<InteractablePrompt> visiblePrompts =
        new HashSet<InteractablePrompt>();

    // Prompts currently inside interaction range.
    private readonly HashSet<InteractablePrompt> promptsThisFrame =
        new HashSet<InteractablePrompt>();

    private void Update()
    {
        UpdateInteractionAndPrompts();

        if (Input.GetKeyDown(interactKey))
        {
            TryInteract();
        }
    }

    private void UpdateInteractionAndPrompts()
    {
        Collider[] nearbyColliders =
            Physics.OverlapSphere(
                transform.position,
                interactionRange,
                interactableLayers,
                QueryTriggerInteraction.Collide
            );

        promptsThisFrame.Clear();

        IInteractable bestInteractable = null;
        float bestDistance = Mathf.Infinity;

        foreach (Collider col in nearbyColliders)
        {
            IInteractable interactable =
                col.GetComponentInParent<IInteractable>();

            if (interactable == null)
                continue;

            Component interactableComponent =
                interactable as Component;

            if (interactableComponent == null)
                continue;

            // =====================================================
            // PROMPT
            //
            // IMPORTANT:
            // Facing direction does NOT matter here.
            // Being inside interactionRange is enough.
            // =====================================================

            InteractablePrompt prompt =
                interactableComponent
                    .GetComponentInChildren<InteractablePrompt>(true);

            if (prompt != null)
            {
                promptsThisFrame.Add(prompt);

                // Tell the prompt which player it should face.
                prompt.Show(transform);
            }

            // =====================================================
            // ACTUAL INTERACTION
            //
            // This keeps the SAME facing requirement as before.
            // =====================================================

            Vector3 directionToTarget =
                interactableComponent.transform.position -
                transform.position;

            directionToTarget.y = 0f;

            if (directionToTarget.sqrMagnitude <= 0.001f)
                continue;

            float angle =
                Vector3.Angle(
                    transform.forward,
                    directionToTarget
                );

            // Still need to face the object to interact.
            if (angle > maxInteractionAngle)
                continue;

            float distance =
                directionToTarget.sqrMagnitude;

            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestInteractable = interactable;
            }
        }

        // Hide prompts that are no longer inside the range.
        foreach (InteractablePrompt prompt in visiblePrompts)
        {
            if (prompt == null)
                continue;

            if (!promptsThisFrame.Contains(prompt))
            {
                prompt.Hide();
            }
        }

        visiblePrompts.Clear();

        foreach (InteractablePrompt prompt in promptsThisFrame)
        {
            visiblePrompts.Add(prompt);
        }

        currentInteractable = bestInteractable;
    }

    private void TryInteract()
    {
        if (currentInteractable == null)
            return;

        currentInteractable.Interact(transform);
    }

    private void OnDisable()
    {
        foreach (InteractablePrompt prompt in visiblePrompts)
        {
            if (prompt != null)
                prompt.Hide();
        }

        visiblePrompts.Clear();
        promptsThisFrame.Clear();

        currentInteractable = null;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(
            transform.position,
            interactionRange
        );
    }
}