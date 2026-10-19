
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CharacterSelectUI : MonoBehaviour
{
    private const int MaxPartySize = 3;

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

    [Header("Party Slot Images")]
    [SerializeField] private Image[] partySlotCharacterImages;
    [SerializeField] private Image[] partySlotElementImages;

    [Header("Party Slot Interaction")]
    [SerializeField] private Button[] partySlotButtons;
    [SerializeField] private GameObject[] partySlotActiveHighlights;

    [Header("Optional Party Slot Text")]
    [SerializeField] private TMP_Text[] partySlotTexts;

    [Header("GO Button")]
    [SerializeField] private Button goButton;

    [Header("Selection Animation")]
    [Range(0f, 1f)]
    [SerializeField] private float previewOpacity = 0.4f;

    [SerializeField] private Vector2 selectionPopOffset = new Vector2(16f, 12f);
    [SerializeField] private float popOutDuration = 0.12f;
    [SerializeField] private float returnDuration = 0.23f;

    [Header("Selection Audio")]
    [SerializeField] private AudioSource selectionAudioSource;
    [SerializeField] private AudioClip selectionSoundEffect;

    [Header("Menu Flow")]
    [SerializeField] private MainMenuFlow mainMenuFlow;

    private CharacterElement currentElement = CharacterElement.Fire;
    private readonly List<CharacterData> currentCharacters = new List<CharacterData>();
    private int currentCharacterIndex;

    // Each array position corresponds to one visual party slot.
    // An empty string means the slot has no confirmed character.
    private readonly string[] selectedPartyCharacterIds = new string[MaxPartySize];

    // -1 means no slot is active (GO is focused).
    private int activeSlotIndex = 0;

    private readonly Coroutine[] slotAnimations = new Coroutine[MaxPartySize];
    private readonly Vector2[] originalImagePositions = new Vector2[MaxPartySize];

    private void Awake()
    {
        for (int i = 0; i < MaxPartySize; i++)
        {
            Image image = GetPartyCharacterImage(i);

            if (image != null)
                originalImagePositions[i] = image.rectTransform.anchoredPosition;

            // Automatically connect each slot button.
            if (partySlotButtons != null &&
                i < partySlotButtons.Length &&
                partySlotButtons[i] != null)
            {
                int slotIndex = i;

                partySlotButtons[i].onClick.AddListener(
                    () => SelectPartySlot(slotIndex)
                );
            }
        }
    }

    private void OnEnable()
    {
        for (int i = 0; i < MaxPartySize; i++)
        {
            selectedPartyCharacterIds[i] = "";

            Image image = GetPartyCharacterImage(i);

            if (image != null)
                image.rectTransform.anchoredPosition = originalImagePositions[i];
        }

        activeSlotIndex = 0;

        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);

        SelectElement(CharacterElement.Fire);
    }

    private void OnDisable()
    {
        StopAllCoroutines();

        for (int i = 0; i < MaxPartySize; i++)
        {
            slotAnimations[i] = null;

            Image image = GetPartyCharacterImage(i);

            if (image != null)
                image.rectTransform.anchoredPosition = originalImagePositions[i];
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            GoBackToSaveSelect();
            return;
        }

        if (Input.GetKeyDown(KeyCode.Return) ||
            Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            // When GO has UI focus, Unity's Button handles Enter.
            if (goButton != null &&
                EventSystem.current != null &&
                EventSystem.current.currentSelectedGameObject == goButton.gameObject)
            {
                return;
            }

            SelectCurrentCharacterForParty();
        }
    }

    // --------------------------------------------------
    // ELEMENT CAROUSEL
    // --------------------------------------------------

    public void NextElement()
    {
        int elementCount =
            System.Enum.GetValues(typeof(CharacterElement)).Length;

        int nextIndex = ((int)currentElement + 1) % elementCount;

        SelectElement((CharacterElement)nextIndex);
        RestoreBrowsingFocus();
    }

    public void PreviousElement()
    {
        int elementCount =
            System.Enum.GetValues(typeof(CharacterElement)).Length;

        int previousIndex =
            ((int)currentElement - 1 + elementCount) % elementCount;

        SelectElement((CharacterElement)previousIndex);
        RestoreBrowsingFocus();
    }

    private void SelectElement(CharacterElement element)
    {
        currentElement = element;
        currentCharacterIndex = 0;

        if (currentElementText != null)
            currentElementText.text = currentElement.ToString();

        BuildCharacterListForElement();
        DisplayCurrentCharacter();
        UpdatePartySlots();
    }

    private void BuildCharacterListForElement()
    {
        currentCharacters.Clear();

        if (characterDatabase == null)
        {
            Debug.LogWarning("No CharacterDatabase assigned.");
            return;
        }

        CharacterData[] characters =
            characterDatabase.GetCharactersByElement(currentElement);

        if (characters == null)
            return;

        foreach (CharacterData character in characters)
        {
            if (character != null)
                currentCharacters.Add(character);
        }
    }

    // --------------------------------------------------
    // CHARACTER CAROUSEL
    // --------------------------------------------------

    public void NextCharacter()
    {
        if (currentCharacters.Count == 0)
            return;

        currentCharacterIndex =
            (currentCharacterIndex + 1) % currentCharacters.Count;

        DisplayCurrentCharacter();
        UpdatePartySlots();
        RestoreBrowsingFocus();
    }

    public void PreviousCharacter()
    {
        if (currentCharacters.Count == 0)
            return;

        currentCharacterIndex =
            (currentCharacterIndex - 1 + currentCharacters.Count)
            % currentCharacters.Count;

        DisplayCurrentCharacter();
        UpdatePartySlots();
        RestoreBrowsingFocus();
    }

    private CharacterData GetCurrentCharacter()
    {
        if (currentCharacters.Count == 0)
            return null;

        if (currentCharacterIndex < 0 ||
            currentCharacterIndex >= currentCharacters.Count)
            return null;

        return currentCharacters[currentCharacterIndex];
    }

    private bool IsUnlocked(CharacterData character)
    {
        return character != null &&
               ProfileManager.Instance != null &&
               ProfileManager.Instance.IsCharacterUnlocked(character.characterId);
    }

    private void DisplayCurrentCharacter()
    {
        CharacterData character = GetCurrentCharacter();

        if (character == null)
        {
            if (characterNameText != null)
                characterNameText.text = "No characters";

            SetStats("-", "-", "-", "-");

            SetImage(characterPoseImage, null);
            SetImage(elementStandImage, null);

            return;
        }

        bool unlocked = IsUnlocked(character);

        if (unlocked)
        {
            int level =
                ProfileManager.Instance.GetCharacterLevel(character.characterId);

            if (characterNameText != null)
                characterNameText.text =
                    character.displayName + " Lv. " + level;

            SetStats(
                character.baseShield.ToString(),
                character.baseHeart.ToString(),
                character.baseSword.ToString(),
                character.baseClover.ToString()
            );

            SetImage(characterPoseImage, character.characterPoseImage);
        }
        else
        {
            if (characterNameText != null)
                characterNameText.text = "???";

            SetStats("-", "-", "-", "-");

            SetImage(characterPoseImage, character.silhouettePoseImage);
        }

        SetImage(elementStandImage, character.elementStandImage);
    }

    private void SetStats(
        string shield,
        string heart,
        string sword,
        string clover)
    {
        if (shieldText != null)
            shieldText.text = "Shield: " + shield;

        if (heartText != null)
            heartText.text = "Heart: " + heart;

        if (swordText != null)
            swordText.text = "Sword: " + sword;

        if (cloverText != null)
            cloverText.text = "Clover: " + clover;
    }

    // --------------------------------------------------
    // ACTIVE PARTY SLOT
    // --------------------------------------------------

    public void SelectPartySlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= MaxPartySize)
            return;

        activeSlotIndex = slotIndex;

        // Keep the current element and character exactly as they are.
        // Only the active party slot changes.

        UpdatePartySlots();

        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);

        Debug.Log("Editing party slot: " + (slotIndex + 1));
    }

    public void SelectCurrentCharacterForParty()
    {
        if (activeSlotIndex < 0 || activeSlotIndex >= MaxPartySize)
            return;

        CharacterData character = GetCurrentCharacter();

        if (character == null)
            return;

        if (!IsUnlocked(character))
        {
            Debug.Log("This character is locked.");
            return;
        }

        // A character cannot occupy two different slots.
        for (int i = 0; i < MaxPartySize; i++)
        {
            if (i != activeSlotIndex &&
                selectedPartyCharacterIds[i] == character.characterId)
            {
                Debug.Log(
                    "Character already selected in another slot: " +
                    character.displayName
                );
                return;
            }
        }

        int confirmedSlot = activeSlotIndex;

        selectedPartyCharacterIds[confirmedSlot] = character.characterId;

        Debug.Log(
            "Confirmed " + character.displayName +
            " in slot " + (confirmedSlot + 1)
        );

        // Find the next empty slot without changing the carousel.
        int nextSlot = FindNextEmptySlot(confirmedSlot);

        activeSlotIndex = nextSlot;

        UpdatePartySlots();

        // Animate the slot we just confirmed.
        PlayConfirmationAnimation(confirmedSlot);

        // Play this character's unique selection voice line.
        PlaySelectionAudio(character);

        // If every slot is filled, focus GO.
        if (nextSlot == -1)
            FocusGoButton();
        else if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
    }

    private int FindNextEmptySlot(int afterSlot)
    {
        for (int offset = 1; offset <= MaxPartySize; offset++)
        {
            int index = (afterSlot + offset) % MaxPartySize;

            if (string.IsNullOrEmpty(selectedPartyCharacterIds[index]))
                return index;
        }

        return -1;
    }

    // --------------------------------------------------
    // PARTY SLOT VISUALS
    // --------------------------------------------------

    private void UpdatePartySlots()
    {
        CharacterData browsingCharacter = GetCurrentCharacter();

        bool canPreview =
            browsingCharacter != null &&
            IsUnlocked(browsingCharacter);

        int confirmedCount = 0;

        for (int i = 0; i < MaxPartySize; i++)
        {
            string characterId = selectedPartyCharacterIds[i];

            CharacterData confirmedCharacter = null;

            if (!string.IsNullOrEmpty(characterId))
            {
                confirmedCount++;

                if (characterDatabase != null)
                    confirmedCharacter =
                        characterDatabase.GetCharacterById(characterId);
            }

            bool isActive = i == activeSlotIndex;

            // Active slots preview the character being browsed.
            // Other slots display their confirmed characters.
            bool showingPreview = isActive && canPreview;

            CharacterData displayedCharacter =
                showingPreview ? browsingCharacter : confirmedCharacter;

            Image characterImage = GetPartyCharacterImage(i);
            Image standImage = GetPartyElementImage(i);

            Sprite selectedSprite =
                displayedCharacter != null
                    ? displayedCharacter.selectedImage
                    : null;

            Sprite standSprite =
                displayedCharacter != null
                    ? displayedCharacter.elementStandImage
                    : null;

            SetImage(characterImage, selectedSprite);
            SetImage(standImage, standSprite);

            if (characterImage != null)
            {
                SetImageOpacity(
                    characterImage,
                    showingPreview ? previewOpacity : 1f
                );
            }

            if (standImage != null)
            {
                SetImageOpacity(
                    standImage,
                    showingPreview ? previewOpacity : 1f
                );
            }

            // Highlight whichever slot is being edited.
            if (partySlotActiveHighlights != null &&
                i < partySlotActiveHighlights.Length &&
                partySlotActiveHighlights[i] != null)
            {
                partySlotActiveHighlights[i].SetActive(isActive);
            }

            // Optional: supports your old text labels.
            if (partySlotTexts != null &&
                i < partySlotTexts.Length &&
                partySlotTexts[i] != null)
            {
                if (showingPreview)
                {
                    partySlotTexts[i].text = browsingCharacter.displayName;
                }
                else
                {
                    partySlotTexts[i].text =
                        confirmedCharacter != null
                            ? confirmedCharacter.displayName
                            : "Empty";
                }
            }
        }

        if (goButton != null)
            goButton.interactable = confirmedCount > 0;
    }

    private Image GetPartyCharacterImage(int index)
    {
        if (partySlotCharacterImages == null ||
            index >= partySlotCharacterImages.Length)
            return null;

        return partySlotCharacterImages[index];
    }

    private Image GetPartyElementImage(int index)
    {
        if (partySlotElementImages == null ||
            index >= partySlotElementImages.Length)
            return null;

        return partySlotElementImages[index];
    }

    private void SetImage(Image image, Sprite sprite)
    {
        if (image == null)
            return;

        image.sprite = sprite;
        image.enabled = sprite != null;
    }

    private void SetImageOpacity(Image image, float opacity)
    {
        if (image == null)
            return;

        Color color = image.color;
        color.a = opacity;
        image.color = color;
    }

    // --------------------------------------------------
    // SELECTION ANIMATION
    // --------------------------------------------------

    private void PlayConfirmationAnimation(int slotIndex)
    {
        Image image = GetPartyCharacterImage(slotIndex);

        if (image == null || !image.enabled)
            return;

        if (slotAnimations[slotIndex] != null)
            StopCoroutine(slotAnimations[slotIndex]);

        slotAnimations[slotIndex] =
            StartCoroutine(AnimateSelectedImage(slotIndex, image));
    }

    private IEnumerator AnimateSelectedImage(int slotIndex, Image image)
    {
        RectTransform imageTransform = image.rectTransform;

        Vector2 startPosition = originalImagePositions[slotIndex];
        Vector2 raisedPosition = startPosition + selectionPopOffset;

        imageTransform.anchoredPosition = startPosition;
        SetImageOpacity(image, 1f);

        // Phase 1: move slightly up-right.
        float timer = 0f;
        float duration = Mathf.Max(0.001f, popOutDuration);

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(timer / duration);
            t = Mathf.SmoothStep(0f, 1f, t);

            imageTransform.anchoredPosition =
                Vector2.Lerp(startPosition, raisedPosition, t);

            yield return null;
        }

        imageTransform.anchoredPosition = raisedPosition;

        // Phase 2: move back down-left into the frame.
        timer = 0f;
        duration = Mathf.Max(0.001f, returnDuration);

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(timer / duration);
            t = Mathf.SmoothStep(0f, 1f, t);

            imageTransform.anchoredPosition =
                Vector2.Lerp(raisedPosition, startPosition, t);

            yield return null;
        }

        imageTransform.anchoredPosition = startPosition;

        slotAnimations[slotIndex] = null;
    }

    // --------------------------------------------------
    // SELECTION AUDIO
    // --------------------------------------------------

    private void PlaySelectionAudio(CharacterData character)
    {
        if (selectionAudioSource == null)
            return;

        if (selectionSoundEffect != null)
            selectionAudioSource.PlayOneShot(selectionSoundEffect);

        if (character != null && character.selectionVoiceLine != null)
            selectionAudioSource.PlayOneShot(character.selectionVoiceLine);
    }

    // --------------------------------------------------
    // UI FOCUS
    // --------------------------------------------------

    private void FocusGoButton()
    {
        if (goButton == null || !goButton.interactable)
            return;

        goButton.Select();

        Debug.Log("Party is full. GO button is now selected.");
    }

    private void RestoreBrowsingFocus()
    {
        if (activeSlotIndex == -1)
        {
            FocusGoButton();
        }
        else if (EventSystem.current != null)
        {
            // Prevent an arrow button from consuming Enter
            // when Enter should confirm a character.
            EventSystem.current.SetSelectedGameObject(null);
        }
    }

    // --------------------------------------------------
    // START GAME
    // --------------------------------------------------

    public void StartGameWithSelectedParty()
    {
        List<string> party = new List<string>();

        for (int i = 0; i < MaxPartySize; i++)
        {
            if (!string.IsNullOrEmpty(selectedPartyCharacterIds[i]))
                party.Add(selectedPartyCharacterIds[i]);
        }

        if (party.Count == 0)
        {
            Debug.Log("Select at least one character before starting.");
            return;
        }

        if (ProfileManager.Instance == null)
        {
            Debug.LogError("No ProfileManager found.");
            return;
        }

        ProfileManager.Instance.SetSelectedParty(party);

        LoadNextSceneInBuild();
    }

    private void LoadNextSceneInBuild()
    {
        int nextSceneIndex =
            SceneManager.GetActiveScene().buildIndex + 1;

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
