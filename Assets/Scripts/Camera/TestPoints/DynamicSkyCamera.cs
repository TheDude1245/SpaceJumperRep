using UnityEngine;

public class DynamicSkyCamera : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform player;

    [Header("Default Camera Style")]
    [SerializeField] private float defaultDistance = 8f;
    [SerializeField] private float defaultHeight = 5f;
    [SerializeField] private float defaultAngle = 0f;
    [SerializeField] private float defaultFov = 60f;
    [SerializeField] private float defaultLookDownOffset = -2f;

    [Header("Smoothing")]
    [SerializeField] private float positionSmoothSpeed = 5f;
    [SerializeField] private float rotationSmoothSpeed = 7f;
    [SerializeField] private float fovSmoothSpeed = 5f;

    private Camera cam;
    private CameraInfluencePoint[] points;

    private void Awake()
    {
        cam = GetComponent<Camera>();
    }

    private void Start()
    {
        RefreshCameraPoints();

        if (player == null)
        {
            GameObject foundPlayer = GameObject.FindGameObjectWithTag("Player");

            if (foundPlayer != null)
            {
                SetPlayerTarget(foundPlayer.transform);
            }
        }
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

        for (int i = 0; i < points.Length; i++)
        {
            CameraInfluencePoint point = points[i];

            if (point == null)
                continue;

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

        if (lookDirection.sqrMagnitude > 0.001f)
        {
            Quaternion desiredRotation = Quaternion.LookRotation(lookDirection);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                desiredRotation,
                Time.deltaTime * rotationSmoothSpeed
            );
        }

        if (cam != null)
        {
            cam.fieldOfView = Mathf.Lerp(
                cam.fieldOfView,
                finalFov,
                Time.deltaTime * fovSmoothSpeed
            );
        }
    }

    public void SetPlayerTarget(Transform newPlayerTarget)
    {
        if (newPlayerTarget == null)
        {
            Debug.LogWarning("Tried to set DynamicSkyCamera target, but target was null.");
            return;
        }

        player = newPlayerTarget;

        Debug.Log("DynamicSkyCamera target set to: " + player.name);
    }

    public void RefreshCameraPoints()
    {
        points = FindObjectsOfType<CameraInfluencePoint>();
    }
}