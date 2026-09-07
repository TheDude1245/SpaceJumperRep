using UnityEngine;

public class CrystalEnergyReceiver : MonoBehaviour
{
    [Header("Puzzle")]
    [SerializeField] private CrystalEnergyPuzzle puzzle;

    [Header("Receiver State")]
    [SerializeField] private GameObject inactiveVisual;
    [SerializeField] private GameObject activeVisual;

    public bool IsReceivingEnergy { get; private set; }

    private void Awake()
    {
        SetReceivingEnergy(false);
    }

    public void ReceiveEnergy()
    {
        if (IsReceivingEnergy)
            return;

        IsReceivingEnergy = true;

        UpdateVisuals();

        if (puzzle != null)
        {
            puzzle.CompletePuzzle();
        }
    }

    public void StopReceivingEnergy()
    {
        if (!IsReceivingEnergy)
            return;

        IsReceivingEnergy = false;

        UpdateVisuals();
    }

    private void SetReceivingEnergy(bool receiving)
    {
        IsReceivingEnergy = receiving;

        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if (inactiveVisual != null)
        {
            inactiveVisual.SetActive(
                !IsReceivingEnergy
            );
        }

        if (activeVisual != null)
        {
            activeVisual.SetActive(
                IsReceivingEnergy
            );
        }
    }
}