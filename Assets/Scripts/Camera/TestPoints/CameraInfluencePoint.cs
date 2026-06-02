using UnityEngine;

public class CameraInfluencePoint : MonoBehaviour
{
    [Header("Influence")]
    public float radius = 12f;
    public float priority = 1f;

    [Header("Camera Style")]
    public float distance = 8f;
    public float height = 5f;
    public float angle = 0f;
    public float fov = 60f;
    public float lookDownOffset = -2f;

    [Header("Debug")]
    public Color gizmoColor = Color.red;

    public float GetWeight(Vector3 playerPosition)
    {
        float distanceToPlayer = Vector3.Distance(playerPosition, transform.position);

        if (distanceToPlayer > radius)
            return 0f;

        float t = 1f - (distanceToPlayer / radius);

        t = t * t * (3f - 2f * t);

        return t * priority;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}