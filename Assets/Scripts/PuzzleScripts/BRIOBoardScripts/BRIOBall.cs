using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BRIOBall : MonoBehaviour
{
    [Header("Reset")]
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float resetDelay = 0.25f;

    private Rigidbody rb;
    private bool resetting;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void ResetBall()
    {
        if (resetting)
            return;

        StartCoroutine(ResetRoutine());
    }

    public void ResetImmediately()
    {
        StopAllCoroutines();

        resetting = false;

        rb.isKinematic = true;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        transform.position = spawnPoint.position;
        transform.rotation = spawnPoint.rotation;

        rb.isKinematic = false;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    public void Freeze(bool freeze)
    {
        if (freeze)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        rb.isKinematic = freeze;
    }

    private IEnumerator ResetRoutine()
    {
        resetting = true;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.isKinematic = true;

        yield return new WaitForSeconds(resetDelay);

        transform.position = spawnPoint.position;
        transform.rotation = spawnPoint.rotation;

        rb.isKinematic = false;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        resetting = false;
    }
}