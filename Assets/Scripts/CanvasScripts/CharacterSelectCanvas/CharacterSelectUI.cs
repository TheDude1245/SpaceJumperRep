
using System;
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
    private const int ElementCount = 5;

    private readonly CharacterElement[] elementOrder =
    {
        CharacterElement.Fire,
        CharacterElement.Earth,
        CharacterElement.Water,
        CharacterElement.Life,
        CharacterElement.Air
    };

    [Header("Database")]
    [SerializeField] private CharacterDatabase characterDatabase;

    [Header("Focused Character")]
    [SerializeField] private TMP_Text characterNameText;

    [Header("Element Carousel")]
    [SerializeField] private Transform elementCarouselRoot;
    [SerializeField] private Button[] elementIconButtons = new Button[5];

    [Header("Element Carousel Layout")]
    [SerializeField] private Vector2 elementCenterOffset = Vector2.zero;
    [SerializeField] private float elementIconSpacing = 85f;
    [SerializeField] private float elementOuterRise = 12f;

    [Header("Element Carousel Appearance")]
    [SerializeField] private float focusedElementScale = 1.25f;
    [SerializeField] private float outerElementScale = 0.7f;

    [Range(0f, 1f)]
    [SerializeField] private float outerElementOpacity = 0.35f;

    [Header("Element Carousel Animation")]
    [Min(0.01f)]
    [SerializeField] private float elementSmoothTime = 0.2f;

    [Header("Character Wheel - 5 Buttons")]
    [SerializeField] private Button[] wheelCharacterButtons = new Button[5];

    [Header("Character Wheel Layout")]
    [SerializeField] private Vector2 wheelCenterOffset = new Vector2(0f, -60f);
    [SerializeField] private float wheelRadiusX = 260f;
    [SerializeField] private float wheelBackRise = 110f;

    [Header("Character Wheel Size")]
    [SerializeField] private float focusedScale = 1.15f;
    [SerializeField] private float backgroundScale = 0.5f;

    [Header("Character Wheel Opacity")]
    [Range(0f, 1f)]
    [SerializeField] private float backgroundOpacity = 0.45f;

    [Range(0f, 1f)]
    [SerializeField] private float lockedOpacityMultiplier = 0.85f;

    [Header("Character Wheel Animation")]
    [Min(0.01f)]
    [SerializeField] private float rotationSmoothTime = 0.25f;

    [Header("Banner Sprites")]
    [SerializeField] private Sprite neutralBanner;
    [SerializeField] private Sprite fireBanner;
    [SerializeField] private Sprite waterBanner;
    [SerializeField] private Sprite airBanner;
    [SerializeField] private Sprite earthBanner;
    [SerializeField] private Sprite lifeBanner;

    [Header("Party Banner Images - Size 3")]
    [SerializeField] private Image[] partySlotBannerImages;
    [SerializeField] private Image[] partySlotCharacterImages;

    [Header("Party Banner Interaction - Size 3")]
    [SerializeField] private Button[] partySlotButtons;
    [SerializeField] private GameObject[] partySlotActiveHighlights;

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

    // Character browsing
    private readonly List<CharacterData> currentCharacters =
        new List<CharacterData>();

    private int currentCharacterIndex;

    // Element carousel
    private int currentElementIndex;
    private float visualElementPosition;
    private float targetElementPosition;
    private float elementVelocity;

    // IMPORTANT:
    // This is calculated from the icons' existing UI positions.
    private Vector2 originalElementCenter;

    // Character wheel
    private float visualWheelPosition;
    private float targetWheelPosition;
    private float wheelVelocity;

    // Party selection
    private readonly string[] selectedPartyCharacterIds =
        new string[MaxPartySize];

    private int activeSlotIndex;

    private readonly Coroutine[] slotAnimations =
        new Coroutine[MaxPartySize];

    private readonly Vector2[] originalImagePositions =
        new Vector2[MaxPartySize];

    // ============================================
    // INITIALIZATION
    // ============================================

    private void Awake()
    {
        InitializeElementButtons();
        InitializeCharacterButtons();
        InitializePartyButtons();
    }

    private void InitializeElementButtons()
    {
        Vector2 combinedPosition = Vector2.zero;
        int validButtons = 0;

        for (int i = 0; i < ElementCount; i++)
        {
            Button button = GetElementButton(i);

            if (button == null)
                continue;

            // Prevent Unity Button transition colors
            // from interfering with our icon opacity.
            button.transition = Selectable.Transition.None;

            RectTransform rect = button.GetComponent<RectTransform>();

            if (rect != null)
            {
                combinedPosition += rect.anchoredPosition;
                validButtons++;
            }

            Image image = button.GetComponent<Image>();

            if (image != null)
            {
                image.enabled = image.sprite != null;
                image.color = Color.white;
            }

            int index = i;

            button.onClick.AddListener(
                () => OnElementClicked(index)
            );
        }

        // Use the actual original UI layout as our center.
        originalElementCenter = validButtons > 0
            ? combinedPosition / validButtons
            : Vector2.zero;
    }

    private void InitializeCharacterButtons()
    {
        if (wheelCharacterButtons == null)
            return;

        for (int i = 0; i < wheelCharacterButtons.Length; i++)
        {
            Button button = wheelCharacterButtons[i];

            if (button == null)
                continue;

            int index = i;

            button.onClick.AddListener(
                () => OnWheelCharacterClicked(index)
            );
        }
    }

    private void InitializePartyButtons()
    {
        for (int i = 0; i < MaxPartySize; i++)
        {
            Image image = GetPartyCharacterImage(i);

            if (image != null)
            {
                originalImagePositions[i] =
                    image.rectTransform.anchoredPosition;
            }

            if (partySlotButtons != null &&
                i < partySlotButtons.Length &&
                partySlotButtons[i] != null)
            {
                int index = i;

                partySlotButtons[i].onClick.AddListener(
                    () => SelectPartySlot(index)
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
            {
                image.rectTransform.anchoredPosition =
                    originalImagePositions[i];
            }
        }

        activeSlotIndex = 0;

        currentElementIndex = 0;
        visualElementPosition = 0f;
        targetElementPosition = 0f;
        elementVelocity = 0f;

        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);

        SelectElement(elementOrder[currentElementIndex]);

        UpdateElementIconVisuals();
    }

    private void OnDisable()
    {
        StopAllCoroutines();

        for (int i = 0; i < MaxPartySize; i++)
        {
            slotAnimations[i] = null;

            Image image = GetPartyCharacterImage(i);

            if (image != null)
            {
                image.rectTransform.anchoredPosition =
                    originalImagePositions[i];
            }
        }
    }

    private void Update()
    {
        HandleKeyboardInput();
        HandleMouseScroll();

        AnimateElementCarousel();
        AnimateCharacterWheel();
    }

    // ============================================
    // INPUT
    // ============================================

    private void HandleKeyboardInput()
    {
        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            GoBackToSaveSelect();
            return;
        }

        if (Input.GetKeyDown(KeyCode.Return) ||
            Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if (goButton != null &&
                EventSystem.current != null &&
                EventSystem.current.currentSelectedGameObject ==
                goButton.gameObject)
            {
                return;
            }

            SelectCurrentCharacterForParty();
        }
    }

    private void HandleMouseScroll()
    {
        float scroll = Input.mouseScrollDelta.y;

        if (Mathf.Abs(scroll) < 0.01f)
            return;

        if (MouseIsOverElementCarousel())
        {
            if (scroll > 0f)
                PreviousElement();
            else
                NextElement();
        }
        else
        {
            if (scroll > 0f)
                PreviousCharacter();
            else
                NextCharacter();
        }
    }

    private bool MouseIsOverElementCarousel()
    {
        if (elementCarouselRoot == null)
            return false;

        RectTransform[] rects =
            elementCarouselRoot.GetComponentsInChildren<RectTransform>(false);

        Vector2 min = new Vector2(
            float.PositiveInfinity,
            float.PositiveInfinity
        );

        Vector2 max = new Vector2(
            float.NegativeInfinity,
            float.NegativeInfinity
        );

        Vector3[] corners = new Vector3[4];

        bool foundUI = false;

        foreach (RectTransform rect in rects)
        {
            if (rect == null || !rect.gameObject.activeInHierarchy)
                continue;

            Canvas canvas = rect.GetComponentInParent<Canvas>();
            Camera uiCamera = null;

            if (canvas != null &&
                canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                uiCamera = canvas.worldCamera;
            }

            rect.GetWorldCorners(corners);

            for (int i = 0; i < 4; i++)
            {
                Vector2 point =
                    RectTransformUtility.WorldToScreenPoint(
                        uiCamera,
                        corners[i]
                    );

                min = Vector2.Min(min, point);
                max = Vector2.Max(max, point);
            }

            foundUI = true;
        }

        if (!foundUI)
            return false;

        Vector2 mouse = Input.mousePosition;

        return mouse.x >= min.x &&
               mouse.x <= max.x &&
               mouse.y >= min.y &&
               mouse.y <= max.y;
    }

    private void RestoreBrowsingFocus()
    {
        if (activeSlotIndex == -1)
        {
            FocusGoButton();
        }
        else if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }

    // ============================================
    // ELEMENT NAVIGATION
    // ============================================

    public void NextElement()
    {
        FocusElement(
            (currentElementIndex + 1) % ElementCount
        );
    }

    public void PreviousElement()
    {
        FocusElement(
            (currentElementIndex - 1 + ElementCount) % ElementCount
        );
    }

    public void OnElementClicked(int index)
    {
        FocusElement(index);
    }

    private void FocusElement(int index)
    {
        if (index < 0 || index >= ElementCount)
            return;

        if (index == currentElementIndex)
            return;

        int difference = index - currentElementIndex;

        if (difference > ElementCount / 2)
            difference -= ElementCount;

        if (difference < -ElementCount / 2)
            difference += ElementCount;

        targetElementPosition += difference;
        currentElementIndex = index;

        SelectElement(elementOrder[currentElementIndex]);

        RestoreBrowsingFocus();
    }

    private void SelectElement(CharacterElement element)
    {
        currentCharacters.Clear();
        currentCharacterIndex = 0;

        // Only reset the character wheel.
        visualWheelPosition = 0f;
        targetWheelPosition = 0f;
        wheelVelocity = 0f;

        if (characterDatabase != null)
        {
            CharacterData[] characters =
                characterDatabase.GetCharactersByElement(element);

            if (characters != null)
            {
                foreach (CharacterData character in characters)
                {
                    if (character != null)
                        currentCharacters.Add(character);
                }
            }
        }

        DisplayCurrentCharacter();
        UpdateWheelVisuals();
        UpdatePartySlots();
    }

    // ============================================
    // ELEMENT CAROUSEL ANIMATION
    // ============================================

    private void AnimateElementCarousel()
    {
        visualElementPosition = Mathf.SmoothDamp(
            visualElementPosition,
            targetElementPosition,
            ref elementVelocity,
            elementSmoothTime,
            Mathf.Infinity,
            Time.unscaledDeltaTime
        );

        if (Mathf.Abs(
            visualElementPosition - targetElementPosition
        ) < 0.001f)
        {
            visualElementPosition = targetElementPosition;
            elementVelocity = 0f;
        }

        UpdateElementIconVisuals();
    }

    private Button GetElementButton(int index)
    {
        if (elementIconButtons == null ||
            index < 0 ||
            index >= elementIconButtons.Length)
        {
            return null;
        }

        return elementIconButtons[index];
    }

    private void UpdateElementIconVisuals()
    {
        for (int i = 0; i < ElementCount; i++)
        {
            Button button = GetElementButton(i);

            if (button == null)
                continue;

            RectTransform rect =
                button.GetComponent<RectTransform>();

            Image image = button.GetComponent<Image>();

            // Wrap the five icons around the focused position.
            float offset = Mathf.Repeat(
                i - visualElementPosition + ElementCount * 0.5f,
                ElementCount
            ) - ElementCount * 0.5f;

            float distance = Mathf.Abs(offset);

            float t = Mathf.Clamp01(distance / 2f);

            if (rect != null)
            {
                // FIX:
                // Position relative to the center of the
                // icons' original UI layout.
                Vector2 position =
                    originalElementCenter +
                    elementCenterOffset +
                    new Vector2(
                        offset * elementIconSpacing,
                        t * elementOuterRise
                    );

                rect.anchoredPosition = position;

                float scale = Mathf.Lerp(
                    focusedElementScale,
                    outerElementScale,
                    t
                );

                rect.localScale = Vector3.one * scale;
            }

            if (image != null)
            {
                // FIX:
                // No extra edge fading.
                // All five icons remain visible.
                float opacity = Mathf.Lerp(
                    1f,
                    Mathf.Max(0.1f, outerElementOpacity),
                    t
                );

                image.enabled = image.sprite != null;

                image.color = new Color(
                    1f, 1f, 1f, opacity
                );
            }

            // Keep all five icons clickable.
            button.interactable = true;
        }
    }

    // ============================================
    // CHARACTER NAVIGATION
    // ============================================

    public void NextCharacter()
    {
        if (currentCharacters.Count == 0)
            return;

        FocusCharacter(
            (currentCharacterIndex + 1) % currentCharacters.Count
        );
    }

    public void PreviousCharacter()
    {
        if (currentCharacters.Count == 0)
            return;

        FocusCharacter(
            (currentCharacterIndex - 1 + currentCharacters.Count)
            % currentCharacters.Count
        );
    }

    public void OnWheelCharacterClicked(int index)
    {
        if (index < 0 || index >= currentCharacters.Count)
            return;

        if (index == currentCharacterIndex)
        {
            SelectCurrentCharacterForParty();
            return;
        }

        FocusCharacter(index);
    }

    private void FocusCharacter(int index)
    {
        if (index < 0 || index >= currentCharacters.Count)
            return;

        if (index == currentCharacterIndex)
            return;

        int count = currentCharacters.Count;
        int difference = index - currentCharacterIndex;

        if (difference > count / 2)
            difference -= count;

        if (difference < -count / 2)
            difference += count;

        targetWheelPosition += difference;
        currentCharacterIndex = index;

        DisplayCurrentCharacter();
        UpdatePartySlots();
        RestoreBrowsingFocus();
    }

    // ============================================
    // CHARACTER WHEEL ANIMATION
    // ============================================

    private void AnimateCharacterWheel()
    {
        visualWheelPosition = Mathf.SmoothDamp(
            visualWheelPosition,
            targetWheelPosition,
            ref wheelVelocity,
            rotationSmoothTime,
            Mathf.Infinity,
            Time.unscaledDeltaTime
        );

        if (Mathf.Abs(
            visualWheelPosition - targetWheelPosition
        ) < 0.001f)
        {
            visualWheelPosition = targetWheelPosition;
            wheelVelocity = 0f;
        }

        UpdateWheelVisuals();
    }

    private void UpdateWheelVisuals()
    {
        if (wheelCharacterButtons == null)
            return;

        int count = currentCharacters.Count;

        int visibleCount = Mathf.Min(
            count,
            wheelCharacterButtons.Length
        );

        List<int> drawingOrder = new List<int>();
        float[] depths = new float[visibleCount];

        for (int i = 0; i < wheelCharacterButtons.Length; i++)
        {
            Button button = wheelCharacterButtons[i];

            if (button == null)
                continue;

            if (i >= visibleCount)
            {
                button.gameObject.SetActive(false);
                continue;
            }

            if (!button.gameObject.activeSelf)
                button.gameObject.SetActive(true);

            CharacterData character = currentCharacters[i];
            bool unlocked = IsUnlocked(character);

            Image image = button.GetComponent<Image>();

            Sprite sprite = unlocked
                ? character.characterPoseImage
                : character.silhouettePoseImage;

            if (image != null)
            {
                image.sprite = sprite;
                image.enabled = sprite != null;
            }

            button.interactable = sprite != null;

            float angle =
                (i - visualWheelPosition) *
                (Mathf.PI * 2f / count);

            float x = Mathf.Sin(angle);
            float depth = (Mathf.Cos(angle) + 1f) * 0.5f;

            depths[i] = depth;

            RectTransform rect =
                button.GetComponent<RectTransform>();

            if (rect != null)
            {
                rect.anchoredPosition =
                    wheelCenterOffset +
                    new Vector2(
                        x * wheelRadiusX,
                        (1f - depth) * wheelBackRise
                    );

                float scale = Mathf.Lerp(
                    backgroundScale,
                    focusedScale,
                    depth
                );

                rect.localScale = Vector3.one * scale;
            }

            if (image != null)
            {
                float opacity = Mathf.Lerp(
                    backgroundOpacity,
                    1f,
                    depth
                );

                if (!unlocked)
                    opacity *= lockedOpacityMultiplier;

                image.color = new Color(
                    1f, 1f, 1f, opacity
                );
            }

            drawingOrder.Add(i);
        }

        drawingOrder.Sort(
            (a, b) => depths[a].CompareTo(depths[b])
        );

        foreach (int index in drawingOrder)
        {
            Button button = wheelCharacterButtons[index];

            if (button != null)
                button.transform.SetAsLastSibling();
        }
    }

    // ============================================
    // CHARACTER INFORMATION
    // ============================================

    private CharacterData GetCurrentCharacter()
    {
        if (currentCharacters.Count == 0)
            return null;

        return currentCharacters[currentCharacterIndex];
    }

    private bool IsUnlocked(CharacterData character)
    {
        return character != null &&
               ProfileManager.Instance != null &&
               ProfileManager.Instance.IsCharacterUnlocked(
                   character.characterId
               );
    }

    private void DisplayCurrentCharacter()
    {
        if (characterNameText == null)
            return;

        CharacterData character = GetCurrentCharacter();

        if (character == null)
        {
            characterNameText.text = "No characters";
            return;
        }

        if (!IsUnlocked(character))
        {
            characterNameText.text = "???";
            return;
        }

        int level =
            ProfileManager.Instance.GetCharacterLevel(character.characterId);

        characterNameText.text =
            character.displayName + " Lv. " + level;
    }

    // ============================================
    // BANNER ARTWORK
    // ============================================

    private Sprite GetBannerForElement(CharacterElement element)
    {
        switch (element)
        {
            case CharacterElement.Fire:
                return fireBanner != null ? fireBanner : neutralBanner;

            case CharacterElement.Water:
                return waterBanner != null ? waterBanner : neutralBanner;

            case CharacterElement.Air:
                return airBanner != null ? airBanner : neutralBanner;

            case CharacterElement.Earth:
                return earthBanner != null ? earthBanner : neutralBanner;

            case CharacterElement.Life:
                return lifeBanner != null ? lifeBanner : neutralBanner;

            default:
                return neutralBanner;
        }
    }

    // ============================================
    // PARTY SLOT SELECTION
    // ============================================

    public void SelectPartySlot(int index)
    {
        if (index < 0 || index >= MaxPartySize)
            return;

        CancelSlotAnimation(index);

        activeSlotIndex = index;
        UpdatePartySlots();

        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
    }

    public void SelectCurrentCharacterForParty()
    {
        if (activeSlotIndex < 0 || activeSlotIndex >= MaxPartySize)
            return;

        CharacterData character = GetCurrentCharacter();

        if (character == null || !IsUnlocked(character))
        {
            Debug.Log("Character is locked or unavailable.");
            return;
        }

        for (int i = 0; i < MaxPartySize; i++)
        {
            if (i != activeSlotIndex &&
                selectedPartyCharacterIds[i] == character.characterId)
            {
                Debug.Log("Character already selected.");
                return;
            }
        }

        int confirmedSlot = activeSlotIndex;

        selectedPartyCharacterIds[confirmedSlot] =
            character.characterId;

        activeSlotIndex = FindNextEmptySlot(confirmedSlot);

        UpdatePartySlots();

        PlayConfirmationAnimation(confirmedSlot);
        PlaySelectionAudio(character);

        if (activeSlotIndex == -1)
        {
            FocusGoButton();
        }
        else if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
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

    // ============================================
    // PARTY BANNER VISUALS
    // ============================================

    private void UpdatePartySlots()
    {
        CharacterData browsingCharacter = GetCurrentCharacter();

        bool canPreview =
            browsingCharacter != null &&
            IsUnlocked(browsingCharacter);

        int confirmedCount = 0;

        for (int i = 0; i < MaxPartySize; i++)
        {
            CharacterData confirmedCharacter = null;

            if (!string.IsNullOrEmpty(selectedPartyCharacterIds[i]))
            {
                confirmedCount++;

                if (characterDatabase != null)
                {
                    confirmedCharacter =
                        characterDatabase.GetCharacterById(
                            selectedPartyCharacterIds[i]
                        );
                }
            }

            bool isActive = i == activeSlotIndex;
            bool isPreview = isActive && canPreview;

            CharacterData displayedCharacter =
                isPreview ? browsingCharacter : confirmedCharacter;

            Sprite bannerSprite = neutralBanner;

            if (displayedCharacter != null)
            {
                bannerSprite =
                    GetBannerForElement(displayedCharacter.element);
            }

            SetImage(GetPartyBannerImage(i), bannerSprite);

            Image portraitImage = GetPartyCharacterImage(i);

            Sprite portraitSprite =
                displayedCharacter != null
                    ? displayedCharacter.selectedImage
                    : null;

            SetImage(portraitImage, portraitSprite);

            if (portraitImage != null)
            {
                SetImageOpacity(
                    portraitImage,
                    isPreview ? previewOpacity : 1f
                );
            }

            if (partySlotActiveHighlights != null &&
                i < partySlotActiveHighlights.Length &&
                partySlotActiveHighlights[i] != null)
            {
                partySlotActiveHighlights[i].SetActive(isActive);
            }
        }

        if (goButton != null)
            goButton.interactable = confirmedCount > 0;
    }

    private Image GetPartyBannerImage(int index)
    {
        if (partySlotBannerImages == null ||
            index >= partySlotBannerImages.Length)
            return null;

        return partySlotBannerImages[index];
    }

    private Image GetPartyCharacterImage(int index)
    {
        if (partySlotCharacterImages == null ||
            index >= partySlotCharacterImages.Length)
            return null;

        return partySlotCharacterImages[index];
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

    // ============================================
    // SELECTION ANIMATION
    // ============================================

    private void CancelSlotAnimation(int index)
    {
        if (slotAnimations[index] != null)
        {
            StopCoroutine(slotAnimations[index]);
            slotAnimations[index] = null;
        }

        Image image = GetPartyCharacterImage(index);

        if (image != null)
        {
            image.rectTransform.anchoredPosition =
                originalImagePositions[index];
        }
    }

    private void PlayConfirmationAnimation(int index)
    {
        Image image = GetPartyCharacterImage(index);

        if (image == null || !image.enabled)
            return;

        CancelSlotAnimation(index);

        slotAnimations[index] =
            StartCoroutine(AnimateSelectedImage(index, image));
    }

    private IEnumerator AnimateSelectedImage(int index, Image image)
    {
        RectTransform rect = image.rectTransform;

        Vector2 start = originalImagePositions[index];
        Vector2 raised = start + selectionPopOffset;

        rect.anchoredPosition = start;
        SetImageOpacity(image, 1f);

        float timer = 0f;
        float duration = Mathf.Max(0.001f, popOutDuration);

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;

            float t = Mathf.SmoothStep(
                0f, 1f,
                Mathf.Clamp01(timer / duration)
            );

            rect.anchoredPosition =
                Vector2.Lerp(start, raised, t);

            yield return null;
        }

        rect.anchoredPosition = raised;

        timer = 0f;
        duration = Mathf.Max(0.001f, returnDuration);

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;

            float t = Mathf.SmoothStep(
                0f, 1f,
                Mathf.Clamp01(timer / duration)
            );

            rect.anchoredPosition =
                Vector2.Lerp(raised, start, t);

            yield return null;
        }

        rect.anchoredPosition = start;
        slotAnimations[index] = null;
    }

    // ============================================
    // AUDIO
    // ============================================

    private void PlaySelectionAudio(CharacterData character)
    {
        if (selectionAudioSource == null)
            return;

        if (selectionSoundEffect != null)
            selectionAudioSource.PlayOneShot(selectionSoundEffect);

        if (character != null &&
            character.selectionVoiceLine != null)
        {
            selectionAudioSource.PlayOneShot(
                character.selectionVoiceLine
            );
        }
    }

    // ============================================
    // START GAME
    // ============================================

    private void FocusGoButton()
    {
        if (goButton != null && goButton.interactable)
            goButton.Select();
    }

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
            Debug.Log("Select at least one character.");
            return;
        }

        if (ProfileManager.Instance == null)
        {
            Debug.LogError("No ProfileManager found.");
            return;
        }

        ProfileManager.Instance.SetSelectedParty(party);

        int nextSceneIndex =
            SceneManager.GetActiveScene().buildIndex + 1;

        if (nextSceneIndex >= SceneManager.sceneCountInBuildSettings)
        {
            Debug.LogError("No next scene in Build Settings.");
            return;
        }

        SceneManager.LoadScene(nextSceneIndex);
    }

    public void GoBackToSaveSelect()
    {
        if (mainMenuFlow != null)
            mainMenuFlow.ShowSaveSelect();
    }
}
