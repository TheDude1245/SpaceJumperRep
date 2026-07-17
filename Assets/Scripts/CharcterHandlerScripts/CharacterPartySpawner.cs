using System.Collections.Generic;
using UnityEngine;

public class CharacterPartySpawner : MonoBehaviour
{
    [Header("Database")]
    [SerializeField] private CharacterDatabase characterDatabase;

    [Header("Spawn")]
    [SerializeField] private Transform spawnPoint;

    [Header("Camera")]
    [SerializeField] private DynamicSkyCamera dynamicSkyCamera;

    [Header("Debug")]
    [SerializeField] private GameObject spawnedPlayer;

    private void Start()
    {
        SpawnSelectedCharacter();
    }

    private void SpawnSelectedCharacter()
    {
        if (characterDatabase == null)
        {
            Debug.LogWarning("No CharacterDatabase assigned to CharacterPartySpawner.");
            return;
        }

        if (ProfileManager.Instance == null)
        {
            Debug.LogWarning("No ProfileManager found. Start the game from MainMenuScene so the global profile loads first.");
            return;
        }

        List<string> selectedParty = ProfileManager.Instance.GetSelectedParty();

        if (selectedParty == null || selectedParty.Count == 0)
        {
            Debug.LogWarning("No selected party found. Select at least one character before entering the map.");
            return;
        }

        string activeCharacterId = selectedParty[0];

        CharacterData characterData = characterDatabase.GetCharacterById(activeCharacterId);

        if (characterData == null)
        {
            Debug.LogWarning("No CharacterData found for ID: " + activeCharacterId);
            return;
        }

        if (characterData.characterPrefab == null)
        {
            Debug.LogWarning("Character has no playable prefab assigned: " + characterData.displayName);
            return;
        }

        Vector3 spawnPosition = spawnPoint != null ? spawnPoint.position : transform.position;
        Quaternion spawnRotation = spawnPoint != null ? spawnPoint.rotation : transform.rotation;

        spawnedPlayer = Instantiate(characterData.characterPrefab, spawnPosition, spawnRotation);
        spawnedPlayer.name = characterData.displayName + "_Player";
        spawnedPlayer.tag = "Player";

        AssignCameraTarget(spawnedPlayer.transform);

        Debug.Log("Spawned selected character: " + characterData.displayName);
        Debug.Log("Selected party: " + string.Join(", ", selectedParty));
    }

    private void AssignCameraTarget(Transform playerTransform)
    {
        if (playerTransform == null)
        {
            Debug.LogWarning("Tried to assign camera target, but playerTransform was null.");
            return;
        }

        if (dynamicSkyCamera == null)
        {
            dynamicSkyCamera = FindObjectOfType<DynamicSkyCamera>();
        }

        if (dynamicSkyCamera == null)
        {
            Debug.LogWarning("No DynamicSkyCamera found in the scene.");
            return;
        }

        dynamicSkyCamera.SetPlayerTarget(playerTransform);

        Debug.Log("Assigned spawned player to DynamicSkyCamera: " + playerTransform.name);
    }
}