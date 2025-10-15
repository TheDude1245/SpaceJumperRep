using UnityEngine;

public class CameraFollow : MonoBehaviour
{
	[Header("Target")]
	public Transform target; // The player

	[Header("Follow Settings")]
	public Vector3 offset = new Vector3(0, 2f, -3f);
	public float followSpeed = 5f;
	public float rotationSmoothness = 8f;

	private void LateUpdate()
	{
		if (!target) return;

		// Desired position with offset behind player
		Vector3 desiredPosition = target.position + target.rotation * offset;
		transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);

		// Smooth rotation to match player's facing direction
		Quaternion targetRotation = Quaternion.Euler(30f, target.eulerAngles.y, 0f);
		transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSmoothness * Time.deltaTime);
	}
}
