using UnityEngine;
using UnityEngine.Events;

public class BRIOPuzzle : MonoBehaviour
{
    [Header("Puzzle References")]
    [SerializeField] private BRIOBoardController boardController;
    [SerializeField] private Rigidbody ballRigidbody;

    [Header("Completion")]
    [SerializeField] private UnityEvent onPuzzleCompleted;

    public bool IsCompleted { get; private set; }

    public void CompletePuzzle()
    {
        if (IsCompleted)
            return;

        IsCompleted = true;

        // Stop player from tilting the board.
        if (boardController != null)
            boardController.enabled = false;

        // Stop the ball.
        if (ballRigidbody != null)
        {
            ballRigidbody.linearVelocity = Vector3.zero;
            ballRigidbody.angularVelocity = Vector3.zero;
            ballRigidbody.isKinematic = true;
        }

        Debug.Log("BRIO Puzzle Completed!");

        onPuzzleCompleted?.Invoke();
    }
}