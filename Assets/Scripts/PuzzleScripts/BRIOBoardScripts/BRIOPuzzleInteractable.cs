using UnityEngine;

public class BRIOPuzzleInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private BRIOPuzzleSequence puzzleSequence;

    public void Interact(Transform interactor)
    {
        if (puzzleSequence == null)
            return;

        puzzleSequence.BeginPuzzle(interactor);
    }
}