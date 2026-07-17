using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveSelectUI : MonoBehaviour
{
    [Header("Displayed Save Data")]
    [SerializeField] private TMP_Text storyPercentText;
    [SerializeField] private TMP_Text trinketsText;
    [SerializeField] private TMP_Text cosmeticsText;
    [SerializeField] private TMP_Text bonusPercentText;
    [SerializeField] private TMP_Text saveStatusText;

    [Header("Save Slot Highlights")]
    [SerializeField] private GameObject[] saveSlotHighlights;

    [Header("Menu Flow")]
    [SerializeField] private MainMenuFlow mainMenuFlow;

    private SaveData[] loadedSaves = new SaveData[4];
    private int selectedSaveIndex = -1;

    private void Start()
    {
        LoadAllSaveSlots();
        ClearInfoPanel();
    }

    private void LoadAllSaveSlots()
    {
        for (int i = 0; i < loadedSaves.Length; i++)
        {
            loadedSaves[i] = SaveSystem.LoadSave(i);
        }
    }

    public void PreviewSave(int saveIndex)
    {
        if (saveIndex < 0 || saveIndex >= loadedSaves.Length)
            return;

        DisplaySaveData(loadedSaves[saveIndex]);
    }

    public void RestoreSelectedSavePreview()
    {
        if (selectedSaveIndex == -1)
        {
            ClearInfoPanel();
            return;
        }

        DisplaySaveData(loadedSaves[selectedSaveIndex]);
    }

    public void SelectSave(int saveIndex)
    {
        if (saveIndex < 0 || saveIndex >= loadedSaves.Length)
            return;

        selectedSaveIndex = saveIndex;

        for (int i = 0; i < saveSlotHighlights.Length; i++)
        {
            saveSlotHighlights[i].SetActive(i == saveIndex);
        }

        DisplaySaveData(loadedSaves[saveIndex]);
    }

    private void DisplaySaveData(SaveData data)
    {
        if (data == null || !data.hasData)
        {
            ShowEmptySaveData();
            return;
        }

        saveStatusText.text = "Save Data Found";

        storyPercentText.text = data.storyPercent + "%";
        trinketsText.text = data.trinketsUnlocked + " / " + data.trinketsTotal;
        cosmeticsText.text = data.cosmeticsUnlocked + " / " + data.cosmeticsTotal;
        bonusPercentText.text = data.bonusPercent + "%";
    }

    private void ShowEmptySaveData()
    {
        saveStatusText.text = "Empty Save";

        storyPercentText.text = "0%";
        trinketsText.text = "0 / 0";
        cosmeticsText.text = "0 / 0";
        bonusPercentText.text = "0%";
    }

    private void ClearInfoPanel()
    {
        saveStatusText.text = "Select a save slot";

        storyPercentText.text = "-";
        trinketsText.text = "-";
        cosmeticsText.text = "-";
        bonusPercentText.text = "-";

        for (int i = 0; i < saveSlotHighlights.Length; i++)
        {
            saveSlotHighlights[i].SetActive(false);
        }
    }

    public void LoadSelectedSave()
    {
        if (selectedSaveIndex == -1)
        {
            Debug.Log("No save selected.");
            return;
        }

        SaveData selectedSave = loadedSaves[selectedSaveIndex];

        if (selectedSave == null || !selectedSave.hasData)
        {
            selectedSave = CreateNewSave(selectedSaveIndex);
            SaveSystem.SaveGame(selectedSave);
            loadedSaves[selectedSaveIndex] = selectedSave;
        }

        PlayerPrefs.SetInt("CurrentSaveSlot", selectedSaveIndex);

        if (mainMenuFlow == null)
        {
            Debug.LogWarning("No MainMenuFlow assigned.");
            return;
        }

        mainMenuFlow.ShowCharacterSelect();
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

    private SaveData CreateNewSave(int slotIndex)
    {
        return new SaveData
        {
            hasData = true,
            slotIndex = slotIndex,

            storyPercent = 0,

            trinketsUnlocked = 0,
            trinketsTotal = 40,

            cosmeticsUnlocked = 0,
            cosmeticsTotal = 20,

            bonusPercent = 0,

            lastSceneName = ""
        };
    }
}