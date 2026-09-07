using System.Collections.Generic;
using UnityEngine;

public class PushRailPoint : MonoBehaviour
{
    [Header("Connections")]
    [Tooltip(
        "Other rail points directly connected to this point. " +
        "Only connected points can be reached from here."
    )]
    [SerializeField]
    private List<PushRailPoint> connectedPoints =
        new List<PushRailPoint>();

    public IReadOnlyList<PushRailPoint> ConnectedPoints
        => connectedPoints;

    public PushBlock OccupyingBlock { get; private set; }

    public bool CanEnter(PushBlock block)
    {
        return OccupyingBlock == null ||
               OccupyingBlock == block;
    }

    public bool TryOccupy(PushBlock block)
    {
        if (!CanEnter(block))
            return false;

        OccupyingBlock = block;

        return true;
    }

    public void Leave(PushBlock block)
    {
        if (OccupyingBlock == block)
            OccupyingBlock = null;
    }

    private void OnDrawGizmos()
    {
        PushRail rail =
            GetComponentInParent<PushRail>();

        float size =
            rail != null
                ? rail.PointGizmoSize
                : 0.2f;

        Gizmos.DrawWireSphere(
            transform.position,
            size
        );

        if (connectedPoints == null)
            return;

        foreach (PushRailPoint point in connectedPoints)
        {
            if (point == null)
                continue;

            Gizmos.DrawLine(
                transform.position,
                point.transform.position
            );
        }
    }
}