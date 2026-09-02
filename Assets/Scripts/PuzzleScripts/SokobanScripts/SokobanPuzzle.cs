using UnityEngine;
using UnityEngine.Events;

public class SokobanPuzzle : MonoBehaviour
{
    [Header("Targets")]
    [SerializeField] private PushTarget[] targets;

    [Header("Completion")]
    [SerializeField] private UnityEvent onPuzzleCompleted;

    private bool completed;

    private void Update()
    {
        if (completed)
            return;

        if (AreAllTargetsOccupied())
        {
            CompletePuzzle();
        }
    }

    private bool AreAllTargetsOccupied()
    {
        if (targets == null || targets.Length == 0)
            return false;

        foreach (PushTarget target in targets)
        {
            if (target == null || !target.IsOccupied)
                return false;
        }

        return true;
    }

    private void CompletePuzzle()
    {
        completed = true;

        Debug.Log("Sokoban Puzzle Completed!");

        onPuzzleCompleted?.Invoke();
    }
}