using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BRIOBoardController : MonoBehaviour
{
    [Header("Tilt Settings")]
    [SerializeField] private float maxTiltAngle = 12f;
    [SerializeField] private float tiltSpeed = 60f;

    [Header("Controls")]
    [SerializeField] private KeyCode forwardKey = KeyCode.W;
    [SerializeField] private KeyCode backwardKey = KeyCode.S;
    [SerializeField] private KeyCode leftKey = KeyCode.A;
    [SerializeField] private KeyCode rightKey = KeyCode.D;

    private Rigidbody rb;
    private Quaternion startingRotation;

    private float horizontalInput;
    private float verticalInput;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        rb.isKinematic = true;
        rb.useGravity = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        startingRotation = rb.rotation;
    }

    private void Update()
    {
        horizontalInput = 0f;
        verticalInput = 0f;

        if (Input.GetKey(leftKey))
            horizontalInput -= 1f;

        if (Input.GetKey(rightKey))
            horizontalInput += 1f;

        if (Input.GetKey(forwardKey))
            verticalInput += 1f;

        if (Input.GetKey(backwardKey))
            verticalInput -= 1f;
    }

    private void FixedUpdate()
    {
        float targetTiltX = verticalInput * maxTiltAngle;
        float targetTiltZ = -horizontalInput * maxTiltAngle;

        Quaternion tiltRotation = Quaternion.Euler(
            targetTiltX,
            0f,
            targetTiltZ
        );

        Quaternion targetRotation =
            startingRotation * tiltRotation;

        Quaternion newRotation =
            Quaternion.RotateTowards(
                rb.rotation,
                targetRotation,
                tiltSpeed * Time.fixedDeltaTime
            );

        rb.MoveRotation(newRotation);
    }
}