using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Health))]
public class ExplosiveBarrel : MonoBehaviour
{
    [Header("Explosion")]
    [Tooltip("Damage dealt to everything inside the explosion radius.")]
    [SerializeField] private float explosionDamage = 50f;

    [Tooltip("Radius of the explosion.")]
    [SerializeField] private float explosionRadius = 3f;

    [Tooltip("Layers that can be affected by the explosion.")]
    [SerializeField] private LayerMask explosionMask = ~0;

    [Header("Destruction")]
    [Tooltip(
        "Optional visual/model to hide immediately when the barrel explodes."
    )]
    [SerializeField] private GameObject barrelVisual;

    [Tooltip(
        "How long before the barrel GameObject is destroyed. " +
        "Useful later for explosion particles."
    )]
    [SerializeField] private float destroyDelay = 0f;

    [Header("Events")]
    [Tooltip(
        "Called when the barrel explodes. " +
        "Later use this for particles, sound, camera shake, etc."
    )]
    [SerializeField] private UnityEvent onExploded;

    private Health health;

    private Collider[] barrelColliders;

    private bool hasExploded;

    // =========================================================
    // UNITY
    // =========================================================

    private void Awake()
    {
        health =
            GetComponent<Health>();

        barrelColliders =
            GetComponentsInChildren<Collider>(
                true
            );
    }

    private void OnEnable()
    {
        if (health != null)
        {
            health.OnDamaged +=
                HandleDamaged;
        }
    }

    private void OnDisable()
    {
        if (health != null)
        {
            health.OnDamaged -=
                HandleDamaged;
        }
    }

    // =========================================================
    // DAMAGE RECEIVED
    // =========================================================

    private void HandleDamaged(
        float damageAmount)
    {
        if (hasExploded)
            return;

        /*
         * Health.TakeDamage() changes currentHealth
         * BEFORE invoking OnDamaged.
         *
         * So by the time we get here we can simply
         * check whether the barrel reached zero.
         */
        if (health.currentHealth <= 0f)
        {
            Explode();
        }
    }

    // =========================================================
    // EXPLOSION
    // =========================================================

    private void Explode()
    {
        if (hasExploded)
            return;

        /*
         * Set this FIRST.
         *
         * This prevents recursive explosions when
         * this barrel's own Health happens to be
         * found inside its explosion radius.
         */
        hasExploded =
            true;

        // =====================================================
        // HIDE BARREL
        // =====================================================

        if (barrelVisual != null)
        {
            barrelVisual.SetActive(
                false
            );
        }

        // =====================================================
        // DISABLE BARREL COLLIDERS
        // =====================================================

        if (barrelColliders != null)
        {
            foreach (Collider col
                     in barrelColliders)
            {
                if (col != null)
                {
                    col.enabled =
                        false;
                }
            }
        }

        // =====================================================
        // FIND EVERYTHING IN EXPLOSION
        // =====================================================

        Collider[] hits =
            Physics.OverlapSphere(
                transform.position,
                explosionRadius,
                explosionMask,
                QueryTriggerInteraction.Collide
            );

        /*
         * A character or barrel might contain
         * several colliders.
         *
         * HashSet ensures each Health component
         * is damaged only once.
         */
        HashSet<Health> damagedTargets =
            new HashSet<Health>();

        foreach (Collider hit in hits)
        {
            if (hit == null)
                continue;

            Health targetHealth =
                hit.GetComponentInParent<Health>();

            if (targetHealth == null)
                continue;

            /*
             * Don't damage this barrel itself.
             */
            if (targetHealth == health)
                continue;

            if (damagedTargets.Contains(
                    targetHealth))
            {
                continue;
            }

            damagedTargets.Add(
                targetHealth
            );

            targetHealth.TakeDamage(
                explosionDamage
            );
        }

        // =====================================================
        // EVENT
        // =====================================================

        Debug.Log(
            name +
            " exploded and dealt " +
            explosionDamage +
            " damage in a radius of " +
            explosionRadius +
            "."
        );

        onExploded?.Invoke();

        // =====================================================
        // REMOVE BARREL
        // =====================================================

        if (destroyDelay <= 0f)
        {
            Destroy(
                gameObject
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

        Destroy(
            gameObject
        );
    }

    // =========================================================
    // DEBUG
    // =========================================================

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(
            transform.position,
            explosionRadius
        );
    }
}