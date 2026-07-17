using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class ProfileSystem
{
    private static string GetProfilePath()
    {
        return Path.Combine(Application.persistentDataPath, "global_profile.json");
    }

    public static GlobalProfileData LoadProfile()
    {
        string path = GetProfilePath();

        if (!File.Exists(path))
        {
            GlobalProfileData newProfile = CreateNewProfile();
            SaveProfile(newProfile);
            return newProfile;
        }

        string json = File.ReadAllText(path);
        GlobalProfileData profile = JsonUtility.FromJson<GlobalProfileData>(json);

        if (profile == null)
        {
            Debug.LogWarning("Global profile could not be loaded. Creating new profile.");
            profile = CreateNewProfile();
            SaveProfile(profile);
            return profile;
        }

        FixMissingData(profile);
        return profile;
    }

    public static void SaveProfile(GlobalProfileData profile)
    {
        FixMissingData(profile);

        string path = GetProfilePath();
        string json = JsonUtility.ToJson(profile, true);

        File.WriteAllText(path, json);

        Debug.Log("Saved global profile to: " + path);
    }

    private static GlobalProfileData CreateNewProfile()
    {
        GlobalProfileData profile = new GlobalProfileData
        {
            hasProfile = true,
            unlockedCharacterIds = new List<string>(),
            characterProgress = new List<CharacterProgressData>(),
            selectedPartyCharacterIds = new List<string>()
        };

        // Starting unlocked characters for testing.
        AddStartingCharacter(profile, "zogma", 1);
        AddStartingCharacter(profile, "goblin", 1);
        AddStartingCharacter(profile, "troll", 1);

        return profile;
    }

    private static void AddStartingCharacter(GlobalProfileData profile, string characterId, int level)
    {
        if (!profile.unlockedCharacterIds.Contains(characterId))
            profile.unlockedCharacterIds.Add(characterId);

        profile.characterProgress.Add(new CharacterProgressData
        {
            characterId = characterId,
            level = level
        });
    }

    private static void FixMissingData(GlobalProfileData profile)
    {
        if (profile.unlockedCharacterIds == null)
            profile.unlockedCharacterIds = new List<string>();

        if (profile.characterProgress == null)
            profile.characterProgress = new List<CharacterProgressData>();

        if (profile.selectedPartyCharacterIds == null)
            profile.selectedPartyCharacterIds = new List<string>();
    }
}