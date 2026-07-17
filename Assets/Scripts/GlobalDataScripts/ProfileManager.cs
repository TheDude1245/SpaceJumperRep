using System.Collections.Generic;
using UnityEngine;

public class ProfileManager : MonoBehaviour
{
    public static ProfileManager Instance { get; private set; }

    [Header("Current Global Profile")]
    [SerializeField] private GlobalProfileData currentProfile;

    public GlobalProfileData CurrentProfile => currentProfile;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadProfile();
    }

    private void LoadProfile()
    {
        currentProfile = ProfileSystem.LoadProfile();
        Debug.Log("Global profile loaded.");
    }

    public void SaveProfile()
    {
        if (currentProfile == null)
        {
            Debug.LogWarning("No global profile to save.");
            return;
        }

        ProfileSystem.SaveProfile(currentProfile);
    }

    public bool IsCharacterUnlocked(string characterId)
    {
        if (currentProfile == null)
            return false;

        return currentProfile.unlockedCharacterIds.Contains(characterId);
    }

    public int GetCharacterLevel(string characterId)
    {
        if (currentProfile == null)
            return 0;

        for (int i = 0; i < currentProfile.characterProgress.Count; i++)
        {
            if (currentProfile.characterProgress[i].characterId == characterId)
                return currentProfile.characterProgress[i].level;
        }

        return 0;
    }

    public void UnlockCharacter(string characterId)
    {
        if (currentProfile == null)
            return;

        if (!currentProfile.unlockedCharacterIds.Contains(characterId))
            currentProfile.unlockedCharacterIds.Add(characterId);

        if (GetCharacterLevel(characterId) == 0)
        {
            currentProfile.characterProgress.Add(new CharacterProgressData
            {
                characterId = characterId,
                level = 1
            });
        }

        SaveProfile();

        Debug.Log("Unlocked character globally: " + characterId);
    }

    public void SetCharacterLevel(string characterId, int level)
    {
        if (currentProfile == null)
            return;

        level = Mathf.Max(1, level);

        for (int i = 0; i < currentProfile.characterProgress.Count; i++)
        {
            if (currentProfile.characterProgress[i].characterId == characterId)
            {
                currentProfile.characterProgress[i].level = level;
                SaveProfile();
                return;
            }
        }

        currentProfile.characterProgress.Add(new CharacterProgressData
        {
            characterId = characterId,
            level = level
        });

        SaveProfile();
    }

    public void SetSelectedParty(List<string> characterIds)
    {
        if (currentProfile == null)
            return;

        currentProfile.selectedPartyCharacterIds.Clear();

        for (int i = 0; i < characterIds.Count; i++)
        {
            if (string.IsNullOrWhiteSpace(characterIds[i]))
                continue;

            if (!IsCharacterUnlocked(characterIds[i]))
                continue;

            if (currentProfile.selectedPartyCharacterIds.Contains(characterIds[i]))
                continue;

            currentProfile.selectedPartyCharacterIds.Add(characterIds[i]);

            if (currentProfile.selectedPartyCharacterIds.Count >= 3)
                break;
        }

        SaveProfile();
    }

    public List<string> GetSelectedParty()
    {
        if (currentProfile == null)
            return new List<string>();

        return new List<string>(currentProfile.selectedPartyCharacterIds);
    }
}