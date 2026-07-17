using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuFlow : MonoBehaviour
{
    [Header("Canvases")]
    [SerializeField] private GameObject titleCanvas;
    [SerializeField] private GameObject saveSelectCanvas;
    [SerializeField] private GameObject settingsCanvas;
    [SerializeField] private GameObject collectionCanvas;
    [SerializeField] private GameObject loadingCanvas;
    [SerializeField] private GameObject characterSelectCanvas;

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

    public void ShowSettings()
    {
        ShowCanvas(settingsCanvas);
    }

    public void ShowCollection()
    {
        ShowCanvas(collectionCanvas);
    }

    public void ShowCharacterSelect()
    {
        ShowCanvas(characterSelectCanvas);
    }

    public void StartSelectedSave()
    {
        ShowCanvas(loadingCanvas);

        // Temporary for now.
        // Later this will check if the save is new or existing.
        SceneManager.LoadScene("GameScene");
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quit Game");
    }

    private void ShowCanvas(GameObject canvasToShow)
    {
        titleCanvas.SetActive(false);
        saveSelectCanvas.SetActive(false);
        settingsCanvas.SetActive(false);
        collectionCanvas.SetActive(false);
        loadingCanvas.SetActive(false);
        characterSelectCanvas.SetActive(false);

        canvasToShow.SetActive(true);
        currentCanvas = canvasToShow;
    }
}