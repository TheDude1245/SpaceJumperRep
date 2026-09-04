using UnityEngine;

public class BRIOGoalHole : MonoBehaviour
{
    [SerializeField] private BRIOPuzzle puzzle;

    private void OnTriggerEnter(Collider other)
    {
        BRIOBall ball = other.GetComponent<BRIOBall>();

        if (ball == null)
            return;

        if (puzzle == null)
            return;

        puzzle.CompletePuzzle();
    }
}