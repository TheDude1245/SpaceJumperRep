using System;
using UnityEngine;

[Serializable]
public class PushBlockConnection
{
    [Tooltip("The node the block will move to.")]
    public PushBlockNode targetNode;

    [Tooltip("How the block reaches the target.")]
    public PushMoveType moveType = PushMoveType.Slide;

    [Tooltip(
        "Only used for Fall movement. " +
        "Place this at the edge the block should slide to before falling."
    )]
    public Transform fallEdgePoint;
}

public class PushBlockNode : MonoBehaviour
{
    [Header("Allowed Directions")]
    [SerializeField] private PushBlockConnection north;
    [SerializeField] private PushBlockConnection south;
    [SerializeField] private PushBlockConnection east;
    [SerializeField] private PushBlockConnection west;

    public PushBlock OccupyingBlock { get; private set; }

    public PushBlockConnection GetConnection(PushDirection direction)
    {
        switch (direction)
        {
            case PushDirection.North:
                return north;

            case PushDirection.South:
                return south;

            case PushDirection.East:
                return east;

            case PushDirection.West:
                return west;
        }

        return null;
    }

    public bool HasDirection(PushDirection direction)
    {
        PushBlockConnection connection = GetConnection(direction);

        return connection != null &&
               connection.targetNode != null;
    }

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
        Gizmos.DrawWireSphere(transform.position, 0.2f);

        DrawConnection(north);
        DrawConnection(south);
        DrawConnection(east);
        DrawConnection(west);
    }

    private void DrawConnection(PushBlockConnection connection)
    {
        if (connection == null ||
            connection.targetNode == null)
        {
            return;
        }

        Gizmos.DrawLine(
            transform.position,
            connection.targetNode.transform.position
        );

        if (connection.moveType == PushMoveType.Fall &&
            connection.fallEdgePoint != null)
        {
            Gizmos.DrawLine(
                transform.position,
                connection.fallEdgePoint.position
            );

            Gizmos.DrawLine(
                connection.fallEdgePoint.position,
                connection.targetNode.transform.position
            );
        }
    }
}