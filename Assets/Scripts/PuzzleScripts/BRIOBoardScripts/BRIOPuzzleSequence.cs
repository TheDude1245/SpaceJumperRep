using System.Collections;
using UnityEngine;

public class BRIOPuzzleSequence : MonoBehaviour
{
    [Header("Cameras")]
    [SerializeField] private Camera mainCamera;

    [Tooltip("The actual fixed top-down camera used during the BRIO puzzle.")]
    [SerializeField] private Camera puzzleCamera;

    [Tooltip("Where the Main Camera moves before transitioning into the puzzle.")]
    [SerializeField] private Transform worldCloseupCameraPoint;

    [Header("BRIO Puzzle")]
    [SerializeField] private BRIOPuzzle brioPuzzle;
    [SerializeField] private BRIOBoardController boardController;
    [SerializeField] private BRIOBall ball;

    [Header("Normal Camera System")]
    [Tooltip("Scripts that normally control the gameplay camera. Leave empty if none are needed.")]
    [SerializeField] private Behaviour[] normalCameraBehaviours;

    [Header("Fade")]
    [SerializeField] private CanvasGroup fadeCanvas;

    [Header("Transition Settings")]
    [SerializeField] private float zoomInDuration = 0.8f;
    [SerializeField] private float zoomOutDuration = 0.8f;
    [SerializeField] private float fadeDuration = 0.25f;

    // Spawned player
    private GameObject activePlayer;

    private PlayerMovement playerMovement;
    private PlayerAttack playerAttack;
    private PlayerInteractor playerInteractor;
    private UnityEngine.InputSystem.PlayerInput playerInput;
    private Rigidbody playerRigidbody;

    // Remember original player states
    private bool movementWasEnabled;
    private bool attackWasEnabled;
    private bool interactorWasEnabled;
    private bool playerInputWasEnabled;
    private bool rigidbodyWasKinematic;

    // Remember camera behaviour states
    private bool[] cameraBehaviourStates;

    // Remember normal gameplay camera position
    private Vector3 savedCameraPosition;
    private Quaternion savedCameraRotation;

    private bool transitioning;
    private bool inPuzzle;

    private void Awake()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    private void Start()
    {
        // Normal gameplay camera starts active.
        if (mainCamera != null)
            mainCamera.enabled = true;

        // BRIO camera starts inactive.
        if (puzzleCamera != null)
            puzzleCamera.enabled = false;

        // Puzzle shouldn't be controllable yet.
        if (boardController != null)
            boardController.enabled = false;

        if (ball != null)
            ball.Freeze(true);

        if (fadeCanvas != null)
        {
            fadeCanvas.alpha = 0f;
            fadeCanvas.blocksRaycasts = false;
            fadeCanvas.interactable = false;
        }
    }

    // =========================================================
    // BEGIN PUZZLE
    // =========================================================

    public void BeginPuzzle(Transform interactor)
    {
        if (transitioning || inPuzzle)
            return;

        if (brioPuzzle != null && brioPuzzle.IsCompleted)
            return;

        if (interactor == null)
        {
            Debug.LogWarning(
                "BRIO Puzzle could not start because no player interacted with it."
            );

            return;
        }

        GetPlayerReferences(interactor);

        if (activePlayer == null)
            return;

        StartCoroutine(EnterPuzzleRoutine());
    }

    // =========================================================
    // GET SPAWNED PLAYER
    // =========================================================

    private void GetPlayerReferences(Transform interactor)
    {
        activePlayer = interactor.gameObject;

        playerMovement =
            activePlayer.GetComponent<PlayerMovement>();

        playerAttack =
            activePlayer.GetComponent<PlayerAttack>();

        playerInteractor =
            activePlayer.GetComponent<PlayerInteractor>();

        playerInput =
            activePlayer.GetComponent<UnityEngine.InputSystem.PlayerInput>();

        playerRigidbody =
            activePlayer.GetComponent<Rigidbody>();
    }

    // =========================================================
    // ENTER BRIO
    // =========================================================

    private IEnumerator EnterPuzzleRoutine()
    {
        transitioning = true;

        // Save current gameplay camera location.
        savedCameraPosition = mainCamera.transform.position;
        savedCameraRotation = mainCamera.transform.rotation;

        DisablePlayerControls();
        DisableNormalCameraControls();

        // -----------------------------------------------------
        // 1. Main Camera zooms toward BRIO object
        // -----------------------------------------------------

        if (worldCloseupCameraPoint != null)
        {
            yield return MoveCamera(
                mainCamera,
                worldCloseupCameraPoint.position,
                worldCloseupCameraPoint.rotation,
                zoomInDuration
            );
        }

        // -----------------------------------------------------
        // 2. Fade to black
        // -----------------------------------------------------

        yield return FadeTo(1f);

        // -----------------------------------------------------
        // 3. CAMERA SWITCH
        // -----------------------------------------------------

        if (mainCamera != null)
            mainCamera.enabled = false;

        if (puzzleCamera != null)
            puzzleCamera.enabled = true;

        // Reset BRIO.
        if (ball != null)
            ball.ResetImmediately();

        if (boardController != null)
            boardController.enabled = true;

        // -----------------------------------------------------
        // 4. Reveal puzzle
        // -----------------------------------------------------

        yield return FadeTo(0f);

        inPuzzle = true;
        transitioning = false;
    }

    // =========================================================
    // EXIT BRIO
    // =========================================================

    public void ExitPuzzle()
    {
        if (transitioning || !inPuzzle)
            return;

        StartCoroutine(ExitPuzzleRoutine());
    }

    private IEnumerator ExitPuzzleRoutine()
    {
        transitioning = true;

        // Stop puzzle controls.
        if (boardController != null)
            boardController.enabled = false;

        if (ball != null)
            ball.Freeze(true);

        // -----------------------------------------------------
        // 1. Fade puzzle to black
        // -----------------------------------------------------

        yield return FadeTo(1f);

        // -----------------------------------------------------
        // 2. Switch back to gameplay camera
        // -----------------------------------------------------

        if (puzzleCamera != null)
            puzzleCamera.enabled = false;

        if (mainCamera != null)
        {
            // Put Main Camera back at the BRIO close-up.
            if (worldCloseupCameraPoint != null)
            {
                mainCamera.transform.position =
                    worldCloseupCameraPoint.position;

                mainCamera.transform.rotation =
                    worldCloseupCameraPoint.rotation;
            }

            mainCamera.enabled = true;
        }

        // -----------------------------------------------------
        // 3. Reveal BRIO object again
        // -----------------------------------------------------

        yield return FadeTo(0f);

        // -----------------------------------------------------
        // 4. Reverse zoom back to gameplay position
        // -----------------------------------------------------

        yield return MoveCamera(
            mainCamera,
            savedCameraPosition,
            savedCameraRotation,
            zoomOutDuration
        );

        // -----------------------------------------------------
        // 5. Restore normal gameplay
        // -----------------------------------------------------

        EnableNormalCameraControls();
        EnablePlayerControls();

        inPuzzle = false;
        transitioning = false;

        activePlayer = null;
    }

    // =========================================================
    // PLAYER CONTROLS
    // =========================================================

    private void DisablePlayerControls()
    {
        if (playerMovement != null)
        {
            movementWasEnabled = playerMovement.enabled;
            playerMovement.enabled = false;
        }

        if (playerAttack != null)
        {
            attackWasEnabled = playerAttack.enabled;
            playerAttack.enabled = false;
        }

        if (playerInteractor != null)
        {
            interactorWasEnabled = playerInteractor.enabled;
            playerInteractor.enabled = false;
        }

        if (playerInput != null)
        {
            playerInputWasEnabled = playerInput.enabled;
            playerInput.enabled = false;
        }

        if (playerRigidbody != null)
        {
            rigidbodyWasKinematic =
                playerRigidbody.isKinematic;

            playerRigidbody.linearVelocity =
                Vector3.zero;

            playerRigidbody.angularVelocity =
                Vector3.zero;

            playerRigidbody.isKinematic = true;
        }
    }

    private void EnablePlayerControls()
    {
        if (playerRigidbody != null)
        {
            playerRigidbody.isKinematic =
                rigidbodyWasKinematic;

            if (!playerRigidbody.isKinematic)
            {
                playerRigidbody.linearVelocity =
                    Vector3.zero;

                playerRigidbody.angularVelocity =
                    Vector3.zero;
            }
        }

        if (playerMovement != null)
            playerMovement.enabled = movementWasEnabled;

        if (playerAttack != null)
            playerAttack.enabled = attackWasEnabled;

        if (playerInteractor != null)
            playerInteractor.enabled = interactorWasEnabled;

        if (playerInput != null)
            playerInput.enabled = playerInputWasEnabled;
    }

    // =========================================================
    // NORMAL CAMERA CONTROL SCRIPTS
    // =========================================================

    private void DisableNormalCameraControls()
    {
        if (normalCameraBehaviours == null)
            return;

        cameraBehaviourStates =
            new bool[normalCameraBehaviours.Length];

        for (int i = 0; i < normalCameraBehaviours.Length; i++)
        {
            Behaviour behaviour =
                normalCameraBehaviours[i];

            if (behaviour == null)
                continue;

            cameraBehaviourStates[i] =
                behaviour.enabled;

            behaviour.enabled = false;
        }
    }

    private void EnableNormalCameraControls()
    {
        if (normalCameraBehaviours == null ||
            cameraBehaviourStates == null)
            return;

        for (int i = 0; i < normalCameraBehaviours.Length; i++)
        {
            Behaviour behaviour =
                normalCameraBehaviours[i];

            if (behaviour == null)
                continue;

            if (i >= cameraBehaviourStates.Length)
                continue;

            behaviour.enabled =
                cameraBehaviourStates[i];
        }
    }

    // =========================================================
    // CAMERA MOVEMENT
    // =========================================================

    private IEnumerator MoveCamera(
        Camera cameraToMove,
        Vector3 targetPosition,
        Quaternion targetRotation,
        float duration)
    {
        if (cameraToMove == null)
            yield break;

        Vector3 startPosition =
            cameraToMove.transform.position;

        Quaternion startRotation =
            cameraToMove.transform.rotation;

        if (duration <= 0f)
        {
            cameraToMove.transform.position =
                targetPosition;

            cameraToMove.transform.rotation =
                targetRotation;

            yield break;
        }

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t =
                Mathf.Clamp01(elapsed / duration);

            t = Mathf.SmoothStep(0f, 1f, t);

            cameraToMove.transform.position =
                Vector3.Lerp(
                    startPosition,
                    targetPosition,
                    t
                );

            cameraToMove.transform.rotation =
                Quaternion.Slerp(
                    startRotation,
                    targetRotation,
                    t
                );

            yield return null;
        }

        cameraToMove.transform.position =
            targetPosition;

        cameraToMove.transform.rotation =
            targetRotation;
    }

    // =========================================================
    // FADE
    // =========================================================

    private IEnumerator FadeTo(float targetAlpha)
    {
        if (fadeCanvas == null)
            yield break;

        fadeCanvas.blocksRaycasts = true;

        float startAlpha =
            fadeCanvas.alpha;

        if (fadeDuration <= 0f)
        {
            fadeCanvas.alpha = targetAlpha;
            fadeCanvas.blocksRaycasts = targetAlpha > 0f;

            yield break;
        }

        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    elapsed / fadeDuration
                );

            fadeCanvas.alpha =
                Mathf.Lerp(
                    startAlpha,
                    targetAlpha,
                    t
                );

            yield return null;
        }

        fadeCanvas.alpha = targetAlpha;

        if (targetAlpha <= 0f)
            fadeCanvas.blocksRaycasts = false;
    }
}