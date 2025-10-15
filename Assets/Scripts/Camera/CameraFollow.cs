using UnityEngine;

public class CameraFollow : MonoBehaviour
{
	public Transform target;        // The player
	public Vector3 offset = new Vector3(0, 0, 0);
	public float followSpeed = 5f;

	private Quaternion fixedRotation;

	private void Start()
	{
		// Store the starting rotation so the camera never changes its angle
		fixedRotation = transform.rotation;
	}

	private void LateUpdate()
	{
		if (!target) return;

		// Follow player's position only (no rotation)
		Vector3 desiredPosition = target.position + fixedRotation * offset;
		transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);

		// Keep the same rotation forever
		transform.rotation = fixedRotation;
	}
}
