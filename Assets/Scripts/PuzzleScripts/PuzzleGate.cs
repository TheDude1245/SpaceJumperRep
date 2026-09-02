using System.Collections;
using UnityEngine;

public class PuzzleGate : MonoBehaviour
{
    [Header("Gate Movement")]
    [SerializeField] private Vector3 openOffset = new Vector3(0f, 5f, 0f);
    [SerializeField] private float openDuration = 1.5f;

    private Vector3 closedPosition;
    private Vector3 openPosition;

    private bool isOpen;
    private bool isMoving;

    private void Awake()
    {
        closedPosition = transform.position;
        openPosition = closedPosition + openOffset;
    }

    public void Open()
    {
        if (isOpen || isMoving)
            return;

        StartCoroutine(OpenGate());
    }

    private IEnumerator OpenGate()
    {
        isMoving = true;

        Vector3 startPosition = transform.position;
        float elapsed = 0f;

        while (elapsed < openDuration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / openDuration);

            // Smooth acceleration/deceleration
            t = Mathf.SmoothStep(0f, 1f, t);

            transform.position = Vector3.Lerp(
                startPosition,
                openPosition,
                t
            );

            yield return null;
        }

        transform.position = openPosition;

        isOpen = true;
        isMoving = false;
    }
}