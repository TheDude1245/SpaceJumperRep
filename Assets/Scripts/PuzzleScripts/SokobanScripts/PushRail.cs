using UnityEngine;

public class PushRail : MonoBehaviour
{
    [Header("Rail Settings")]
    [Tooltip("Used to determine North, South, East and West for the arrows.")]
    [SerializeField] private float pointGizmoSize = 0.2f;

    public float PointGizmoSize => pointGizmoSize;

    public Vector3 Forward
    {
        get
        {
            Vector3 forward =
                Vector3.ProjectOnPlane(
                    transform.forward,
                    Vector3.up
                );

            if (forward.sqrMagnitude < 0.001f)
                return Vector3.forward;

            return forward.normalized;
        }
    }

    public Vector3 Right
    {
        get
        {
            Vector3 right =
                Vector3.ProjectOnPlane(
                    transform.right,
                    Vector3.up
                );

            if (right.sqrMagnitude < 0.001f)
                return Vector3.right;

            return right.normalized;
        }
    }
}