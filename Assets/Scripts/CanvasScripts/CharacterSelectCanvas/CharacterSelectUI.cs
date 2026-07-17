using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CharacterSelectUI : MonoBehaviour
{
    [Header("Database")]
    [SerializeField] private CharacterDatabase characterDatabase;

    [Header("Left Display")]
    [SerializeField] private TMP_Text characterNameText;
    [SerializeField] private Image characterPoseImage;
    [SerializeField] private Image elementStandImage;

    [Header("Stats")]
    [SerializeField] private TMP_Text shieldText;
    [SerializeField] private TMP_Text heartText;
    [SerializeField] private TMP_Text swordText;
    [SerializeField] private TMP_Text cloverText;

    [Header("Current Element")]
    [SerializeField] private TMP_Text currentElementText;

    [Header("Selected Party")]
    [SerializeField] private TMP_Text[] partySlotTexts;
    [SerializeField] private Image[] partySlotCharacterImages;
    [SerializeField] private Image[] partySlotElementImages;

    [Header("Menu Flow")]
    [SerializeField] private MainMenuFlow mainMenuFlow;

    private CharacterElement currentElement = CharacterElement.Fire;
    private List<CharacterData> currentUnlockedCharacters = new List<CharacterData>();
    private int currentCharacterIndex = 0;

    private List<string> selectedPartyCharacterIds = new List<string>();
    private const int MaxPartySize = 3;

    private void OnEnable()
    {
        selectedPartyCharacterIds.Clear();
        UpdatePartySlots();

        SelectElement(CharacterElement.Fire);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            SelectCurrentCharacterForParty();
        }

        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            GoBackToSaveSelect();
        }
    }

    public void NextElement()
    {
        int elementCount = System.Enum.GetValues(typeof(CharacterElement)).Length;
        int nextIndex = ((int)currentElement + 1) % elementCount;

        SelectElement((CharacterElement)nextIndex);
    }

    public void PreviousElement()
    {
        int elementCount = System.Enum.GetValues(typeof(CharacterElement)).Length;
        int previousIndex = (int)currentElement - 1;

        if (previousIndex < 0)
            previousIndex = elementCount - 1;

        SelectElement((CharacterElement)previousIndex);
    }

    public void NextCharacter()
    {
        if (currentUnlockedCharacters.Count == 0)
            return;

        currentCharacterIndex++;

        if (currentCharacterIndex >= currentUnlockedCharacters.Count)
            currentCharacterIndex = 0;

        DisplayCurrentCharacter();
    }

    public void PreviousCharacter()
    {
        if (currentUnlockedCharacters.Count == 0)
            return;

        currentCharacterIndex--;

        if (currentCharacterIndex < 0)
            currentCharacterIndex = currentUnlockedCharacters.Count - 1;

        DisplayCurrentCharacter();
    }

    private void SelectElement(CharacterElement element)
    {
        currentElement = element;
        currentCharacterIndex = 0;

        if (currentElementText != null)
            currentElementText.text = currentElement.ToString();

        BuildUnlockedCharacterListForElement();
        DisplayCurrentCharacter();
    }

    private void BuildUnlockedCharacterListForElement()
    {
        currentUnlockedCharacters.Clear();

        if (characterDatabase == null)
        {
            Debug.LogWarning("No CharacterDatabase assigned.");
            return;
        }

        if (ProfileManager.Instance == null)
        {
            Debug.LogWarning("No ProfileManager found.");
            return;
        }

        CharacterData[] charactersOfElement = characterDatabase.GetCharactersByElement(currentElement);

        for (int i = 0; i < charactersOfElement.Length; i++)
        {
            CharacterData character = charactersOfElement[i];

            if (character == null)
                continue;

            if (ProfileManager.Instance.IsCharacterUnlocked(character.characterId))
            {
                currentUnlockedCharacters.Add(character);
            }
        }
    }

    private void DisplayCurrentCharacter()
    {
        if (currentUnlockedCharacters.Count == 0)
        {
            DisplayNoCharacterFound();
            return;
        }

        CharacterData character = currentUnlockedCharacters[currentCharacterIndex];

        int level = ProfileManager.Instance.GetCharacterLevel(character.characterId);

        if (characterNameText != null)
            characterNameText.text = character.displayName + " Lv. " + level;

        if (shieldText != null)
            shieldText.text = "Shield: " + character.baseShield;

        if (heartText != null)
            heartText.text = "Heart: " + character.baseHeart;

        if (swordText != null)
            swordText.text = "Sword: " + character.baseSword;

        if (cloverText != null)
            cloverText.text = "Clover: " + character.baseClover;

        if (characterPoseImage != null)
        {
            characterPoseImage.enabled = true;

            if (character.characterPoseImage != null)
                characterPoseImage.sprite = character.characterPoseImage;
        }

        if (elementStandImage != null)
        {
            elementStandImage.enabled = true;

            if (character.elementStandImage != null)
                elementStandImage.sprite = character.elementStandImage;
        }
    }

    private void DisplayNoCharacterFound()
    {
        if (characterNameText != null)
            characterNameText.text = "No unlocked " + currentElement + " characters";

        if (shieldText != null)
            shieldText.text = "Shield: -";

        if (heartText != null)
            heartText.text = "Heart: -";

        if (swordText != null)
            swordText.text = "Sword: -";

        if (cloverText != null)
            cloverText.text = "Clover: -";

        if (characterPoseImage != null)
            characterPoseImage.enabled = false;

        if (elementStandImage != null)
            elementStandImage.enabled = false;
    }

    public void SelectCurrentCharacterForParty()
    {
        if (currentUnlockedCharacters.Count == 0)
        {
            Debug.Log("No character available to select.");
            return;
        }

        CharacterData character = currentUnlockedCharacters[currentCharacterIndex];

        if (selectedPartyCharacterIds.Contains(character.characterId))
        {
            Debug.Log("Character already selected: " + character.displayName);
            return;
        }

        if (selectedPartyCharacterIds.Count >= MaxPartySize)
        {
            Debug.Log("Party is already full.");
            return;
        }

        selectedPartyCharacterIds.Add(character.characterId);
        UpdatePartySlots();

        Debug.Log("Added to party: " + character.displayName);
    }

    private void UpdatePartySlots()
    {
        for (int i = 0; i < MaxPartySize; i++)
        {
            CharacterData character = null;

            if (i < selectedPartyCharacterIds.Count)
                character = characterDatabase.GetCharacterById(selectedPartyCharacterIds[i]);

            if (partySlotTexts != null && i < partySlotTexts.Length && partySlotTexts[i] != null)
            {
                partySlotTexts[i].text = character != null ? character.displayName : "Empty";
            }

            if (partySlotCharacterImages != null && i < partySlotCharacterImages.Length && partySlotCharacterImages[i] != null)
            {
                if (character != null && character.characterIconImage != null)
                {
                    partySlotCharacterImages[i].enabled = true;
                    partySlotCharacterImages[i].sprite = character.characterIconImage;
                }
                else
                {
                    partySlotCharacterImages[i].enabled = character != null;
                }
            }

            if (partySlotElementImages != null && i < partySlotElementImages.Length && partySlotElementImages[i] != null)
            {
                if (character != null && character.elementStandImage != null)
                {
                    partySlotElementImages[i].enabled = true;
                    partySlotElementImages[i].sprite = character.elementStandImage;
                }
                else
                {
                    partySlotElementImages[i].enabled = character != null;
                }
            }
        }
    }

    public void StartGameWithSelectedParty()
    {
        if (selectedPartyCharacterIds.Count == 0)
        {
            Debug.Log("Select at least one character before starting.");
            return;
        }

        if (ProfileManager.Instance != null)
        {
            ProfileManager.Instance.SetSelectedParty(selectedPartyCharacterIds);
        }

        LoadNextSceneInBuild();
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

    public void GoBackToSaveSelect()
    {
        if (mainMenuFlow == null)
        {
            Debug.LogWarning("No MainMenuFlow assigned.");
            return;
        }

        mainMenuFlow.ShowSaveSelect();
    }
}