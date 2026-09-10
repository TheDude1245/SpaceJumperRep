using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class SpikeDamageHitbox : MonoBehaviour
{
    [Header("Damage")]
    [SerializeField] private float damageAmount = 20f;

    [Tooltip(
        "If enabled, only Health components marked as isPlayer can be damaged."
    )]
    [SerializeField] private bool onlyDamagePlayer = true;

    public bool IsDangerous { get; private set; }

    private Collider damageCollider;

    /*
     * Keeps track of everything already damaged
     * during the CURRENT spike activation.
     *
     * This means one rise = one hit per target.
     */
    private readonly HashSet<Health> damagedThisActivation =
        new HashSet<Health>();

    // =========================================================
    // UNITY
    // =========================================================

    private void Awake()
    {
        damageCollider =
            GetComponent<Collider>();

        damageCollider.isTrigger =
            true;

        SetDangerous(false);
    }

    // =========================================================
    // HAZARD STATE
    // =========================================================

    public void SetDangerous(bool dangerous)
    {
        IsDangerous =
            dangerous;

        if (!dangerous)
        {
            /*
             * Reset once the spikes become safe.
             *
             * Next activation can damage the
             * player again.
             */
            damagedThisActivation.Clear();
        }
    }

    // =========================================================
    // COLLISION
    // =========================================================

    private void OnTriggerEnter(Collider other)
    {
        TryDamage(
            other
        );
    }

    private void OnTriggerStay(Collider other)
    {
        /*
         * Important:
         *
         * If the player is already standing inside
         * the hitbox when the spikes activate,
         * OnTriggerEnter may already have happened
         * while the trap was safe.
         *
         * OnTriggerStay guarantees they still get hit.
         */
        TryDamage(
            other
        );
    }

    // =========================================================
    // DAMAGE
    // =========================================================

    private void TryDamage(Collider other)
    {
        if (!IsDangerous)
            return;

        /*
         * GetComponentInParent is intentional.
         *
         * Your spawned character may have its
         * collision collider on a child while
         * Health sits on the player root.
         */
        Health health =
            other.GetComponentInParent<Health>();

        if (health == null)
            return;

        if (onlyDamagePlayer &&
            !health.isPlayer)
        {
            return;
        }

        /*
         * Already damaged this character during
         * this activation?
         */
        if (damagedThisActivation.Contains(
                health))
        {
            return;
        }

        damagedThisActivation.Add(
            health
        );

        health.TakeDamage(
            damageAmount
        );

        Debug.Log(
            name +
            " dealt " +
            damageAmount +
            " spike damage to " +
            health.name
        );
    }
}