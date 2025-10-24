using UnityEngine;

public class CameraFollow : MonoBehaviour
{
	[Header("Target")]
	public Transform target;

	[Header("Position Settings")]
	public Vector3 offset = new Vector3(0f, 8f, -10f);
	public float followSpeed = 5f;

	[Header("Rotation Settings")]
	public float rotationLerp = 4f;

	// --- Zone data ---
	[HideInInspector] public bool hasZoneOffset = false;
	[HideInInspector] public Vector3 zoneOffset;
	[HideInInspector] public bool hasZoneRotation = false;
	[HideInInspector] public Vector3 zoneRotation;

	// --- Default rotation store ---
	private Quaternion defaultRotation;

	private void Start()
	{
		// Remember the camera's starting rotation so we can return to it later
		defaultRotation = transform.rotation;
	}

	private void LateUpdate()
	{
		if (!target) return;

		// --- Position follow ---
		Vector3 useOffset = hasZoneOffset ? zoneOffset : offset;
		Vector3 desiredPos = target.position + transform.rotation * useOffset;
		transform.position = Vector3.Lerp(transform.position, desiredPos, followSpeed * Time.deltaTime);

		// --- Rotation behaviour ---
		if (hasZoneRotation)
		{
			// Inside a zone → rotate to that angle
			Quaternion desiredRot = Quaternion.Euler(zoneRotation);
			transform.rotation = Quaternion.Slerp(transform.rotation, desiredRot, rotationLerp * Time.deltaTime);
		}
		else
		{
			// No zone active → rotate back to default
			transform.rotation = Quaternion.Slerp(transform.rotation, defaultRotation, rotationLerp * Time.deltaTime);
		}
	}

	// --- Called by CameraZone.cs ---
	public void SetZoneOffset(Vector3? newOffset)
	{
		if (newOffset.HasValue)
		{
			hasZoneOffset = true;
			zoneOffset = newOffset.Value;
		}
		else hasZoneOffset = false;
	}

	public void SetZoneRotation(Vector3? newEuler)
	{
		if (newEuler.HasValue)
		{
			hasZoneRotation = true;
			zoneRotation = newEuler.Value;
		}
		else hasZoneRotation = false;
	}
}
