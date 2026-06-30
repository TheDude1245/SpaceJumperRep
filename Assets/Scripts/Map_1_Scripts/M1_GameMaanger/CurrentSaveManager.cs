using UnityEngine;
using UnityEngine.SceneManagement;

public class CurrentSaveManager : MonoBehaviour
{
    public static CurrentSaveManager Instance { get; private set; }

    [Header("Current Save")]
    [SerializeField] private int currentSaveSlot = -1;
    [SerializeField] private SaveData currentSaveData;

    public int CurrentSaveSlot => currentSaveSlot;
    public SaveData CurrentSaveData => currentSaveData;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadCurrentSave();
    }

    private void LoadCurrentSave()
    {
        currentSaveSlot = PlayerPrefs.GetInt("CurrentSaveSlot", -1);

        if (currentSaveSlot == -1)
        {
            Debug.LogWarning("No current save slot was selected before loading the map.");
            return;
        }

        currentSaveData = SaveSystem.LoadSave(currentSaveSlot);

        if (currentSaveData == null || !currentSaveData.hasData)
        {
            Debug.LogWarning("Selected save slot had no data. Creating new save data.");

            currentSaveData = new SaveData
            {
                hasData = true,
                slotIndex = currentSaveSlot,

                storyPercent = 0,

                trinketsUnlocked = 0,
                trinketsTotal = 40,

                cosmeticsUnlocked = 0,
                cosmeticsTotal = 20,

                bonusPercent = 0,

                lastSceneName = SceneManager.GetActiveScene().name
            };

            SaveSystem.SaveGame(currentSaveData);
        }

        Debug.Log("Loaded save slot: " + currentSaveSlot);
    }

    public void SaveCurrentGame()
    {
        if (currentSaveData == null)
        {
            Debug.LogWarning("No current save data to save.");
            return;
        }

        currentSaveData.lastSceneName = SceneManager.GetActiveScene().name;
        SaveSystem.SaveGame(currentSaveData);

        Debug.Log("Current save saved.");
    }

    public void SetStoryProgress(int percent)
    {
        currentSaveData.storyPercent = Mathf.Clamp(percent, 0, 100);
        SaveCurrentGame();
    }

    public void SetBonusProgress(int percent)
    {
        currentSaveData.bonusPercent = Mathf.Clamp(percent, 0, 100);
        SaveCurrentGame();
    }

    public void UnlockTrinket()
    {
        currentSaveData.trinketsUnlocked++;

        if (currentSaveData.trinketsUnlocked > currentSaveData.trinketsTotal)
            currentSaveData.trinketsUnlocked = currentSaveData.trinketsTotal;

        SaveCurrentGame();
    }

    public void UnlockCosmetic()
    {
        currentSaveData.cosmeticsUnlocked++;

        if (currentSaveData.cosmeticsUnlocked > currentSaveData.cosmeticsTotal)
            currentSaveData.cosmeticsUnlocked = currentSaveData.cosmeticsTotal;

        SaveCurrentGame();
    }
}