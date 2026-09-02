using UnityEngine;

public class PushTarget : MonoBehaviour
{
    [Header("Visuals")]
    [SerializeField] private Renderer targetRenderer;
    [SerializeField] private Material inactiveMaterial;
    [SerializeField] private Material activeMaterial;

    public bool IsOccupied { get; private set; }

    private PushBlock occupyingBlock;

    private void Start()
    {
        UpdateVisual();
    }

    private void OnTriggerEnter(Collider other)
    {
        PushBlock block = other.GetComponentInParent<PushBlock>();

        if (block == null)
            return;

        if (IsOccupied)
            return;

        occupyingBlock = block;
        IsOccupied = true;

        UpdateVisual();
    }

    private void OnTriggerExit(Collider other)
    {
        PushBlock block = other.GetComponentInParent<PushBlock>();

        if (block == null)
            return;

        if (block != occupyingBlock)
            return;

        occupyingBlock = null;
        IsOccupied = false;

        UpdateVisual();
    }

    private void UpdateVisual()
    {
        if (targetRenderer == null)
            return;

        if (IsOccupied)
        {
            if (activeMaterial != null)
                targetRenderer.sharedMaterial = activeMaterial;
        }
        else
        {
            if (inactiveMaterial != null)
                targetRenderer.sharedMaterial = inactiveMaterial;
        }
    }
}