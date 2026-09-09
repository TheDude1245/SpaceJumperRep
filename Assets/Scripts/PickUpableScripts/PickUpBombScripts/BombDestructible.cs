using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class BombDestructible : MonoBehaviour
{
    [Header("Destruction")]
    [Tooltip(
        "The object that will be destroyed. " +
        "Leave empty to destroy this GameObject."
    )]
    [SerializeField] private GameObject objectToDestroy;

    [Tooltip(
        "How long after receiving the explosion before " +
        "the object is actually destroyed."
    )]
    [SerializeField] private float destroyDelay = 0f;

    [Header("Events")]
    [Tooltip(
        "Called when this object is destroyed by a bomb. " +
        "Useful later for particles, sounds, animations, gates, etc."
    )]
    [SerializeField] private UnityEvent onDestroyedByBomb;

    public bool IsDestroyed { get; private set; }

    private Collider[] destructibleColliders;

    // =========================================================
    // UNITY
    // =========================================================

    private void Awake()
    {
        if (objectToDestroy == null)
        {
            objectToDestroy =
                gameObject;
        }

        destructibleColliders =
            GetComponentsInChildren<Collider>(
                true
            );
    }

    // =========================================================
    // BOMB DESTRUCTION
    // =========================================================

    public void DestroyFromBomb()
    {
        if (IsDestroyed)
            return;

        IsDestroyed = true;

        /*
         * Disable collision immediately.
         *
         * This prevents another bomb / physics object
         * interacting with something that is already
         * supposed to be destroyed.
         */
        if (destructibleColliders != null)
        {
            foreach (Collider col
                     in destructibleColliders)
            {
                if (col != null)
                {
                    col.enabled = false;
                }
            }
        }

        onDestroyedByBomb?.Invoke();

        if (destroyDelay <= 0f)
        {
            Destroy(
                objectToDestroy
            );
        }
        else
        {
            StartCoroutine(
                DestroyAfterDelay()
            );
        }
    }

    private IEnumerator DestroyAfterDelay()
    {
        yield return new WaitForSeconds(
            destroyDelay
        );

        if (objectToDestroy != null)
        {
            Destroy(
                objectToDestroy
            );
        }
    }
}