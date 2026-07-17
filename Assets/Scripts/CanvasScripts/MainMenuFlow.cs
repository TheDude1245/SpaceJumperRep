using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuFlow : MonoBehaviour
{
    [Header("Canvases")]
    [SerializeField] private GameObject titleCanvas;
    [SerializeField] private GameObject saveSelectCanvas;
    [SerializeField] private GameObject characterSelectCanvas;
    [SerializeField] private GameObject settingsCanvas;
    [SerializeField] private GameObject collectionCanvas;
    [SerializeField] private GameObject loadingCanvas;

    private GameObject currentCanvas;

    private void Start()
    {
        ShowTitle();
    }

    public void ShowTitle()
    {
        ShowCanvas(titleCanvas);
    }

    public void ShowSaveSelect()
    {
        ShowCanvas(saveSelectCanvas);
    }

    public void ShowCharacterSelect()
    {
        ShowCanvas(characterSelectCanvas);
    }

    public void ShowSettings()
    {
        ShowCanvas(settingsCanvas);
    }

    public void ShowCollection()
    {
        ShowCanvas(collectionCanvas);
    }

    public void ShowLoading()
    {
        ShowCanvas(loadingCanvas);
    }

    public void StartSelectedSave()
    {
        ShowLoading();
        LoadNextSceneInBuild();
    }

    public void QuitGame()
    {
        Debug.Log("Quit Game");
        Application.Quit();
    }

    private void ShowCanvas(GameObject canvasToShow)
    {
        SetCanvasActive(titleCanvas, false);
        SetCanvasActive(saveSelectCanvas, false);
        SetCanvasActive(characterSelectCanvas, false);
        SetCanvasActive(settingsCanvas, false);
        SetCanvasActive(collectionCanvas, false);
        SetCanvasActive(loadingCanvas, false);

        if (canvasToShow == null)
        {
            Debug.LogWarning("Tried to show a canvas, but the canvas reference is missing on MainMenuFlow.");
            return;
        }

        canvasToShow.SetActive(true);
        currentCanvas = canvasToShow;
    }

    private void SetCanvasActive(GameObject canvas, bool isActive)
    {
        if (canvas != null)
        {
            canvas.SetActive(isActive);
        }
    }

    private void LoadNextSceneInBuild()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int nextSceneIndex = currentSceneIndex + 1;

        if (nextSceneIndex >= SceneManager.sceneCountInBuildSettings)
        {
            Debug.LogError("No next scene exists in Build Settings.");
            return;
        }

        SceneManager.LoadScene(nextSceneIndex);
    }
}