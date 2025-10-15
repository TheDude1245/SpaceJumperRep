using UnityEngine;

public class StencilMaskController : MonoBehaviour
{
	[Header("References")]
	public GameObject cameraObject;   // Your main camera
	public GameObject player;         // The player object
	public GameObject sphereMask;     // The sphere mask
	public LayerMask obstructionMask; // Layer of obstacles (walls, etc.)

	[Header("Mask Settings")]
	public float visibleScale = 10f;  // Size when visible
	public float hiddenScale = 0f;    // Size when hidden
	public float lerpSpeed = 5f;      // How fast to transition

	private Vector3 targetScale;

	private void Start()
	{
		// Start with visible size
		targetScale = Vector3.one * visibleScale;
		sphereMask.transform.localScale = targetScale;
	}

	void Update()
	{
		if (!player || !cameraObject || !sphereMask) return;

		RaycastHit hit;
		Vector3 direction = (player.transform.position - cameraObject.transform.position).normalized;

		// Raycast from camera toward player
		if (Physics.Raycast(cameraObject.transform.position, direction, out hit, Mathf.Infinity, obstructionMask))
		{
			// If ray hits the sphere mask first → hide it
			if (hit.collider.gameObject.CompareTag("SphereMask"))
			{
				targetScale = Vector3.one * hiddenScale;
			}
			else
			{
				targetScale = Vector3.one * visibleScale;
			}
		}
		else
		{
			// No hit = make it visible
			targetScale = Vector3.one * visibleScale;
		}

		// Smoothly interpolate scale
		sphereMask.transform.localScale = Vector3.Lerp(
			sphereMask.transform.localScale,
			targetScale,
			Time.deltaTime * lerpSpeed
		);
	}
}
