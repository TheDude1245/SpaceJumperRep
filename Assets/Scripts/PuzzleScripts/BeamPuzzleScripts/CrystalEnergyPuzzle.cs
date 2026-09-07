using UnityEngine;
using UnityEngine.Events;

public class CrystalEnergyPuzzle : MonoBehaviour
{
    [Header("Puzzle")]
    [SerializeField] private CrystalEnergySource energySource;

    [Header("Completion Beam Behaviour")]
    [Tooltip(
        "If enabled, the completed laser fades away. " +
        "If disabled, the completed laser remains permanently visible."
    )]
    [SerializeField] private bool disableBeamOnCompletion = true;

    [Tooltip(
        "How long the completed laser remains fully visible before fading."
    )]
    [SerializeField] private float beamHoldTimeAfterCompletion = 0.5f;

    [Tooltip(
        "How long the entire laser takes to fade from fully visible to invisible."
    )]
    [SerializeField] private float beamFadeDuration = 0.5f;

    [Header("Completion")]
    [SerializeField] private UnityEvent onPuzzleCompleted;

    public bool IsCompleted { get; private set; }

    public void CompletePuzzle()
    {
        if (IsCompleted)
            return;

        IsCompleted = true;

        Debug.Log(name + " completed!");

        if (disableBeamOnCompletion &&
            energySource != null)
        {
            energySource.BeginCompletionFade(
                beamHoldTimeAfterCompletion,
                beamFadeDuration
            );
        }

        onPuzzleCompleted?.Invoke();
    }
}