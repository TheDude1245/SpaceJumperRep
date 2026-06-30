using System.IO;
using UnityEngine;
using System.Collections.Generic;

public static class SaveSystem
{
    private static string GetSavePath(int slotIndex)
    {
        return Path.Combine(Application.persistentDataPath, $"save_slot_{slotIndex}.json");
    }

    public static bool SaveExists(int slotIndex)
    {
        return File.Exists(GetSavePath(slotIndex));
    }

    public static SaveData LoadSave(int slotIndex)
    {
        string path = GetSavePath(slotIndex);

        if (!File.Exists(path))
        {
            return CreateEmptySave(slotIndex);
        }

        string json = File.ReadAllText(path);
        SaveData data = JsonUtility.FromJson<SaveData>(json);

        if (data == null)
        {
            Debug.LogWarning($"Save slot {slotIndex} could not be loaded.");
            return CreateEmptySave(slotIndex);
        }

        FixMissingData(data);

        return data;
    }

    public static void SaveGame(SaveData data)
    {
        FixMissingData(data);

        string path = GetSavePath(data.slotIndex);
        string json = JsonUtility.ToJson(data, true);

        File.WriteAllText(path, json);

        Debug.Log("Saved game to: " + path);
    }

    private static SaveData CreateEmptySave(int slotIndex)
    {
        return new SaveData
        {
            hasData = false,
            slotIndex = slotIndex,

            storyPercent = 0,

            coins = 0,

            trinketsUnlocked = 0,
            trinketsTotal = 40,

            cosmeticsUnlocked = 0,
            cosmeticsTotal = 20,

            bonusPercent = 0,

            lastSceneName = "",

            collectedTrinketIds = new List<string>()
        };
    }

    private static void FixMissingData(SaveData data)
    {
        if (data.collectedTrinketIds == null)
        {
            data.collectedTrinketIds = new List<string>();
        }
    }
}