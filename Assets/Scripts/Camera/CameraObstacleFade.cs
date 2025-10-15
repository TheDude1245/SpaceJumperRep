using UnityEngine;
using System.Collections.Generic;

public class CameraObstacleFade : MonoBehaviour
{
	public Transform player;
	public float fadeSpeed = 10f;
	public float targetAlpha = 0.2f;
	private List<Renderer> fadedRenderers = new List<Renderer>();
	private LayerMask obstacleMask;

	private void Start()
	{
		// Adjust this to include layers you want to fade (e.g., Default)
		obstacleMask = LayerMask.GetMask("Default");
	}

	private void LateUpdate()
	{
		if (!player) return;

		// Step 1: Restore previous faded objects
		foreach (Renderer r in fadedRenderers)
		{
			if (r)
				SetAlpha(r, 1f);
		}
		fadedRenderers.Clear();

		// Step 2: Raycast from camera to player
		Vector3 direction = player.position - transform.position;
		float distance = Vector3.Distance(transform.position, player.position);

		Ray ray = new Ray(transform.position, direction);
		RaycastHit[] hits = Physics.RaycastAll(ray, distance, obstacleMask);

		// Step 3: Fade out any obstacles
		foreach (RaycastHit hit in hits)
		{
			Renderer r = hit.collider.GetComponent<Renderer>();
			if (r != null && r.gameObject != player.gameObject)
			{
				SetAlpha(r, targetAlpha);
				fadedRenderers.Add(r);
			}
		}
	}

	void SetAlpha(Renderer r, float alpha)
	{
		foreach (Material m in r.materials)
		{
			if (m.HasProperty("_Color"))
			{
				Color c = m.color;
				c.a = Mathf.Lerp(c.a, alpha, Time.deltaTime * fadeSpeed);
				m.color = c;

				// Ensure the material supports transparency
				if (alpha < 1f)
				{
					m.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
					m.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
					m.SetInt("_ZWrite", 0);
					m.DisableKeyword("_ALPHATEST_ON");
					m.EnableKeyword("_ALPHABLEND_ON");
					m.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
				}
				else
				{
					m.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
					m.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
					m.SetInt("_ZWrite", 1);
					m.DisableKeyword("_ALPHABLEND_ON");
					m.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Geometry;
				}
			}
		}
	}
}
