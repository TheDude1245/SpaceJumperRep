using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class CameraZone : MonoBehaviour
{
	[Header("Offset override")]
	public bool overrideOffset = false;
	public Vector3 zoneOffset = new Vector3(0, 10, -12);

	[Header("Rotation override")]
	public bool overrideRotation = false;
	public Vector3 zoneRotation = new Vector3(25, 45, 0);

	private void OnTriggerEnter(Collider other)
	{
		if (!other.CompareTag("Player")) return;

		CameraFollow cam = FindObjectOfType<CameraFollow>();
		if (cam == null) return;

		// Apply new zone overrides
		if (overrideOffset)
			cam.SetZoneOffset(zoneOffset);
		if (overrideRotation)
			cam.SetZoneRotation(zoneRotation);
	}

	private void OnTriggerExit(Collider other)
	{
		if (!other.CompareTag("Player")) return;

		CameraFollow cam = FindObjectOfType<CameraFollow>();
		if (cam == null) return;

		// Reset both offset and rotation back to defaults
		cam.SetZoneOffset(null);
		cam.SetZoneRotation(null);
	}
}
