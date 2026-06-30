using System.Collections.Generic;
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

                lastSceneName = SceneManager.GetActiveScene().name,

                collectedTrinketIds = new List<string>()
            };

            SaveSystem.SaveGame(currentSaveData);
        }

        FixMissingData();

        Debug.Log("Loaded save slot: " + currentSaveSlot);
    }

    private void FixMissingData()
    {
        if (currentSaveData.collectedTrinketIds == null)
        {
            currentSaveData.collectedTrinketIds = new List<string>();
        }
    }

    public void SaveCurrentGame()
    {
        if (currentSaveData == null)
        {
            Debug.LogWarning("No current save data to save.");
            return;
        }

        FixMissingData();

        currentSaveData.lastSceneName = SceneManager.GetActiveScene().name;
        SaveSystem.SaveGame(currentSaveData);

        Debug.Log("Current save saved.");
    }

    public bool IsTrinketCollected(string trinketId)
    {
        if (currentSaveData == null)
            return false;

        FixMissingData();

        return currentSaveData.collectedTrinketIds.Contains(trinketId);
    }

    public bool TryCollectTrinket(string trinketId)
    {
        if (string.IsNullOrWhiteSpace(trinketId))
        {
            Debug.LogWarning("Tried to collect trinket with no ID.");
            return false;
        }

        if (currentSaveData == null)
        {
            Debug.LogWarning("No current save data found.");
            return false;
        }

        FixMissingData();

        if (currentSaveData.collectedTrinketIds.Contains(trinketId))
        {
            Debug.Log("Trinket already collected: " + trinketId);
            return false;
        }

        currentSaveData.collectedTrinketIds.Add(trinketId);

        currentSaveData.trinketsUnlocked = currentSaveData.collectedTrinketIds.Count;

        if (currentSaveData.trinketsUnlocked > currentSaveData.trinketsTotal)
        {
            currentSaveData.trinketsUnlocked = currentSaveData.trinketsTotal;
        }

        SaveCurrentGame();

        Debug.Log("Collected trinket: " + trinketId);

        return true;
    }

    public void AddCoins(int amount)
    {
        if (amount <= 0)
            return;

        if (currentSaveData == null)
        {
            Debug.LogWarning("No current save data found. Could not add coins.");
            return;
        }

        currentSaveData.coins += amount;

        SaveCurrentGame();

        Debug.Log("Added coins: " + amount + ". Total coins: " + currentSaveData.coins);
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
        {
            currentSaveData.trinketsUnlocked = currentSaveData.trinketsTotal;
        }

        SaveCurrentGame();
    }

    public void UnlockCosmetic()
    {
        currentSaveData.cosmeticsUnlocked++;

        if (currentSaveData.cosmeticsUnlocked > currentSaveData.cosmeticsTotal)
        {
            currentSaveData.cosmeticsUnlocked = currentSaveData.cosmeticsTotal;
        }

        SaveCurrentGame();
    }
}