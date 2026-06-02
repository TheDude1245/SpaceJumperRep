using UnityEngine;

public class DynamicSkyCamera : MonoBehaviour
{
    [Header("Target")]
    public Transform player;

    [Header("Default Camera Style")]
    public float defaultDistance = 8f;
    public float defaultHeight = 5f;
    public float defaultAngle = 0f;
    public float defaultFov = 60f;
    public float defaultLookDownOffset = -2f;

    [Header("Smoothing")]
    public float positionSmoothSpeed = 5f;
    public float rotationSmoothSpeed = 7f;
    public float fovSmoothSpeed = 5f;

    private Camera cam;
    private CameraInfluencePoint[] points;

    private void Start()
    {
        cam = GetComponent<Camera>();
        points = FindObjectsOfType<CameraInfluencePoint>();
    }

    private void LateUpdate()
    {
        if (player == null)
            return;

        float finalDistance = defaultDistance;
        float finalHeight = defaultHeight;
        float finalAngle = defaultAngle;
        float finalFov = defaultFov;
        float finalLookDownOffset = defaultLookDownOffset;

        float totalWeight = 1f;

        foreach (CameraInfluencePoint point in points)
        {
            float weight = point.GetWeight(player.position);

            if (weight <= 0f)
                continue;

            finalDistance += point.distance * weight;
            finalHeight += point.height * weight;
            finalAngle += point.angle * weight;
            finalFov += point.fov * weight;
            finalLookDownOffset += point.lookDownOffset * weight;

            totalWeight += weight;
        }

        finalDistance /= totalWeight;
        finalHeight /= totalWeight;
        finalAngle /= totalWeight;
        finalFov /= totalWeight;
        finalLookDownOffset /= totalWeight;

        Quaternion cameraRotationAroundPlayer = Quaternion.Euler(0f, finalAngle, 0f);

        Vector3 offset = cameraRotationAroundPlayer * new Vector3(0f, finalHeight, -finalDistance);
        Vector3 desiredPosition = player.position + offset;

        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            Time.deltaTime * positionSmoothSpeed
        );

        Vector3 lookTarget = player.position + Vector3.up * finalLookDownOffset;
        Vector3 lookDirection = lookTarget - transform.position;

        Quaternion desiredRotation = Quaternion.LookRotation(lookDirection);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            desiredRotation,
            Time.deltaTime * rotationSmoothSpeed
        );

        cam.fieldOfView = Mathf.Lerp(
            cam.fieldOfView,
            finalFov,
            Time.deltaTime * fovSmoothSpeed
        );
    }
}