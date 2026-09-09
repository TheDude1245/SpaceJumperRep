using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(CarryableItem))]
[RequireComponent(typeof(Rigidbody))]
public class CarryableBomb :
    MonoBehaviour,
    ICarriedItemUse
{
    [Header("Throw")]
    [SerializeField] private float throwSpeed = 10f;

    [SerializeField] private float upwardThrowSpeed = 3f;

    [SerializeField] private float throwSpin = 4f;

    [Header("Arming")]
    [Tooltip(
        "Short delay after throwing before the bomb becomes armed."
    )]
    [SerializeField] private float armingDelay = 0.1f;

    [Header("Explosion")]
    [Tooltip(
        "Radius in which destructible objects are affected."
    )]
    [SerializeField] private float explosionRadius = 3f;

    [Tooltip(
        "Layers checked by the explosion."
    )]
    [SerializeField] private LayerMask explosionMask = ~0;

    [Tooltip(
        "How long the bomb GameObject remains after exploding. " +
        "Useful later for explosion particles."
    )]
    [SerializeField] private float bombDestroyDelay = 0f;

    [Header("Pickup")]
    [SerializeField] private float repickupDelay = 0.5f;

    [Header("Visual")]
    [Tooltip(
        "Optional bomb model. " +
        "If assigned, it disappears immediately when the bomb explodes."
    )]
    [SerializeField] private GameObject bombVisual;

    [Header("Events")]
    [Tooltip(
        "Called whenever the bomb explodes. " +
        "Later this can spawn particles, sounds, camera shake, etc."
    )]
    [SerializeField] private UnityEvent onExploded;

    private CarryableItem carryableItem;
    private Rigidbody rb;

    private Collider[] bombColliders;

    private bool hasBeenThrown;
    private bool isArmed;
    private bool hasExploded;

    private float armTime;

    /*
     * If the bomb hits a destructible during the
     * tiny arming delay, remember it.
     *
     * Once the delay finishes, it will still explode.
     */
    private BombDestructible pendingDestructibleHit;

    // =========================================================
    // UNITY
    // =========================================================

    private void Awake()
    {
        carryableItem =
            GetComponent<CarryableItem>();

        rb =
            GetComponent<Rigidbody>();

        bombColliders =
            GetComponentsInChildren<Collider>(
                true
            );
    }

    private void Update()
    {
        if (!hasBeenThrown ||
            isArmed ||
            hasExploded)
        {
            return;
        }

        if (Time.time >= armTime)
        {
            isArmed = true;

            /*
             * If we already struck a destructible
             * during the tiny grace period,
             * explode now.
             */
            if (pendingDestructibleHit != null)
            {
                Explode(
                    pendingDestructibleHit
                );
            }
        }
    }

    // =========================================================
    // Q
    // =========================================================

    public void Use(Transform user)
    {
        Throw(
            user
        );
    }

    // =========================================================
    // THROW
    // =========================================================

    private void Throw(
        Transform thrower)
    {
        if (thrower == null)
            return;

        if (carryableItem == null ||
            !carryableItem.IsCarried)
        {
            return;
        }

        if (hasExploded)
            return;

        Vector3 throwDirection =
            thrower.forward;

        throwDirection.y = 0f;

        if (throwDirection.sqrMagnitude <
            0.001f)
        {
            return;
        }

        throwDirection.Normalize();

        // =====================================================
        // STATE
        // =====================================================

        hasBeenThrown = true;
        isArmed = false;

        pendingDestructibleHit =
            null;

        armTime =
            Time.time +
            Mathf.Max(
                0f,
                armingDelay
            );

        // =====================================================
        // RELEASE
        // =====================================================

        carryableItem.PreventPickupFor(
            repickupDelay
        );

        /*
         * TRUE:
         * Rigidbody + gravity become active.
         */
        carryableItem.ReleaseToWorld(
            true
        );

        // =====================================================
        // VELOCITY
        // =====================================================

        Vector3 throwVelocity =
            throwDirection *
            throwSpeed;

        throwVelocity +=
            Vector3.up *
            upwardThrowSpeed;

        rb.linearVelocity =
            throwVelocity;

        rb.angularVelocity =
            thrower.right *
            throwSpin;

        /*
         * If arming delay is zero,
         * arm immediately.
         */
        if (armingDelay <= 0f)
        {
            isArmed = true;
        }
    }

    // =========================================================
    // COLLISION
    // =========================================================

    private void OnCollisionEnter(
        Collision collision)
    {
        CheckCollisionForExplosion(
            collision
        );
    }

    private void OnCollisionStay(
        Collision collision)
    {
        CheckCollisionForExplosion(
            collision
        );
    }

    private void CheckCollisionForExplosion(
        Collision collision)
    {
        if (!hasBeenThrown ||
            hasExploded)
        {
            return;
        }

        BombDestructible destructible =
            collision.collider
                .GetComponentInParent<
                    BombDestructible
                >();

        /*
         * Normal ground/wall?
         *
         * Do absolutely nothing.
         */
        if (destructible == null)
            return;

        /*
         * Hit a valid destructible during
         * the short arming delay.
         */
        if (!isArmed)
        {
            pendingDestructibleHit =
                destructible;

            return;
        }

        /*
         * Armed + valid target.
         */
        Explode(
            destructible
        );
    }

    // =========================================================
    // EXPLOSION
    // =========================================================

    private void Explode(
        BombDestructible directHit)
    {
        if (hasExploded)
            return;

        hasExploded = true;

        // =====================================================
        // STOP BOMB PHYSICS
        // =====================================================

        rb.linearVelocity =
            Vector3.zero;

        rb.angularVelocity =
            Vector3.zero;

        rb.useGravity =
            false;

        rb.isKinematic =
            true;

        /*
         * Bomb itself disappears visually
         * immediately.
         */
        if (bombVisual != null)
        {
            bombVisual.SetActive(
                false
            );
        }

        /*
         * Disable all bomb colliders,
         * including the PickupHitbox.
         */
        if (bombColliders != null)
        {
            foreach (Collider col
                     in bombColliders)
            {
                if (col != null)
                {
                    col.enabled =
                        false;
                }
            }
        }

        // =====================================================
        // FIND DESTRUCTIBLES IN EXPLOSION
        // =====================================================

        Collider[] hits =
            Physics.OverlapSphere(
                transform.position,
                explosionRadius,
                explosionMask,
                QueryTriggerInteraction.Collide
            );

        /*
         * One destructible might have several
         * colliders.
         *
         * HashSet prevents it receiving the same
         * explosion multiple times.
         */
        HashSet<BombDestructible>
            destructibles =
                new HashSet<
                    BombDestructible
                >();

        /*
         * Always include whatever directly
         * caused the explosion.
         */
        if (directHit != null)
        {
            destructibles.Add(
                directHit
            );
        }

        foreach (Collider hit
                 in hits)
        {
            if (hit == null)
                continue;

            BombDestructible destructible =
                hit.GetComponentInParent<
                    BombDestructible
                >();

            if (destructible != null)
            {
                destructibles.Add(
                    destructible
                );
            }
        }

        // =====================================================
        // DESTROY THEM
        // =====================================================

        foreach (
            BombDestructible destructible
            in destructibles)
        {
            if (destructible != null)
            {
                destructible.DestroyFromBomb();
            }
        }

        // =====================================================
        // EXPLOSION EVENT
        // =====================================================

        Debug.Log(
            name +
            " exploded and hit " +
            destructibles.Count +
            " destructible object(s)."
        );

        onExploded?.Invoke();

        // =====================================================
        // REMOVE BOMB
        // =====================================================

        if (bombDestroyDelay <= 0f)
        {
            Destroy(
                gameObject
            );
        }
        else
        {
            StartCoroutine(
                DestroyBombAfterDelay()
            );
        }
    }

    private IEnumerator DestroyBombAfterDelay()
    {
        yield return new WaitForSeconds(
            bombDestroyDelay
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