using TMPro;
using UnityEngine;

public class SettingsCarouselUI : MonoBehaviour
{
    public enum CategoryAction
    {
        NormalSettings,
        Collection,
        Placeholder1,
        Placeholder2,
        Placeholder3
    }

    [System.Serializable]
    public class SettingsCategory
    {
        public string categoryName;
        public RectTransform rectTransform;
        public CanvasGroup canvasGroup;
        public CategoryAction action;
    }

    [Header("Categories")]
    [SerializeField] private SettingsCategory[] categories;
    [SerializeField] private int selectedIndex = 1;
    [SerializeField] private bool loopCarousel = true;

    [Header("Layout")]
    [SerializeField] private float spacing = 320f;
    [SerializeField] private float yPosition = 0f;

    [Header("Scale")]
    [SerializeField] private float selectedScale = 1f;
    [SerializeField] private float sideScale = 0.75f;
    [SerializeField] private float farScale = 0.5f;

    [Header("Fade")]
    [SerializeField] private float selectedAlpha = 1f;
    [SerializeField] private float sideAlpha = 0.55f;
    [SerializeField] private float farAlpha = 0.2f;

    [Header("Movement")]
    [SerializeField] private float transitionSpeed = 12f;
    [SerializeField] private float scrollCooldown = 0.15f;

    [Header("UI Roots")]
    [SerializeField] private GameObject settingsCarouselRoot;
    [SerializeField] private GameObject normalSettingsPanel;

    [Header("Menu Flow")]
    [SerializeField] private MainMenuFlow mainMenuFlow;

    [Header("Optional UI")]
    [SerializeField] private TMP_Text selectedCategoryTitleText;

    private float nextScrollTime;

    private void OnEnable()
    {
        if (settingsCarouselRoot != null)
            settingsCarouselRoot.SetActive(true);

        if (normalSettingsPanel != null)
            normalSettingsPanel.SetActive(false);

        if (categories == null || categories.Length == 0)
            return;

        selectedIndex = Mathf.Clamp(selectedIndex, 0, categories.Length - 1);

        UpdateSelectedCategoryTitle();
        ApplyVisuals(true);
    }

    private void Update()
    {
        if (settingsCarouselRoot != null && !settingsCarouselRoot.activeSelf)
            return;

        HandleMouseScroll();
        ApplyVisuals(false);
    }

    private void HandleMouseScroll()
    {
        if (Time.unscaledTime < nextScrollTime)
            return;

        float scroll = Input.mouseScrollDelta.y;

        if (scroll > 0.1f)
        {
            SelectPrevious();
            nextScrollTime = Time.unscaledTime + scrollCooldown;
        }
        else if (scroll < -0.1f)
        {
            SelectNext();
            nextScrollTime = Time.unscaledTime + scrollCooldown;
        }
    }

    public void ClickCategory(int categoryIndex)
    {
        if (!IsValidIndex(categoryIndex))
            return;

        if (categoryIndex == selectedIndex)
        {
            OpenSelectedCategory();
            return;
        }

        selectedIndex = categoryIndex;
        UpdateSelectedCategoryTitle();
        ApplyVisuals(false);
    }

    public void SelectNext()
    {
        selectedIndex++;

        if (selectedIndex >= categories.Length)
            selectedIndex = loopCarousel ? 0 : categories.Length - 1;

        UpdateSelectedCategoryTitle();
    }

    public void SelectPrevious()
    {
        selectedIndex--;

        if (selectedIndex < 0)
            selectedIndex = loopCarousel ? categories.Length - 1 : 0;

        UpdateSelectedCategoryTitle();
    }

    private void OpenSelectedCategory()
    {
        SettingsCategory selectedCategory = categories[selectedIndex];

        switch (selectedCategory.action)
        {
            case CategoryAction.NormalSettings:
                OpenNormalSettings();
                break;

            case CategoryAction.Collection:
                OpenCollection();
                break;

            case CategoryAction.Placeholder1:
            case CategoryAction.Placeholder2:
            case CategoryAction.Placeholder3:
                Debug.Log("Placeholder category selected: " + selectedCategory.categoryName);
                break;
        }
    }

    private void OpenNormalSettings()
    {
        if (settingsCarouselRoot != null)
            settingsCarouselRoot.SetActive(false);

        if (normalSettingsPanel != null)
        {
            normalSettingsPanel.SetActive(true);
            normalSettingsPanel.transform.SetAsLastSibling();
        }
    }

    private void OpenCollection()
    {
        if (mainMenuFlow == null)
        {
            Debug.LogWarning("No MainMenuFlow assigned on SettingsCarouselUI.");
            return;
        }

        mainMenuFlow.ShowCollection();
    }

    public void BackToCategories()
    {
        if (normalSettingsPanel != null)
            normalSettingsPanel.SetActive(false);

        if (settingsCarouselRoot != null)
            settingsCarouselRoot.SetActive(true);

        ApplyVisuals(true);
    }

    private void ApplyVisuals(bool instant)
    {
        if (categories == null || categories.Length == 0)
            return;

        for (int i = 0; i < categories.Length; i++)
        {
            SettingsCategory category = categories[i];

            if (category.rectTransform == null)
                continue;

            int offset = GetCarouselOffset(i);
            int distance = Mathf.Abs(offset);

            float targetX = offset * spacing;
            float targetScale = GetScaleForDistance(distance);
            float targetAlpha = GetAlphaForDistance(distance);

            Vector2 targetPosition = new Vector2(targetX, yPosition);
            Vector3 targetScaleVector = new Vector3(targetScale, targetScale, targetScale);

            if (instant)
            {
                category.rectTransform.anchoredPosition = targetPosition;
                category.rectTransform.localScale = targetScaleVector;

                if (category.canvasGroup != null)
                    category.canvasGroup.alpha = targetAlpha;
            }
            else
            {
                category.rectTransform.anchoredPosition = Vector2.Lerp(
                    category.rectTransform.anchoredPosition,
                    targetPosition,
                    Time.unscaledDeltaTime * transitionSpeed
                );

                category.rectTransform.localScale = Vector3.Lerp(
                    category.rectTransform.localScale,
                    targetScaleVector,
                    Time.unscaledDeltaTime * transitionSpeed
                );

                if (category.canvasGroup != null)
                {
                    category.canvasGroup.alpha = Mathf.Lerp(
                        category.canvasGroup.alpha,
                        targetAlpha,
                        Time.unscaledDeltaTime * transitionSpeed
                    );
                }
            }

            if (category.canvasGroup != null)
            {
                category.canvasGroup.interactable = distance <= 2;
                category.canvasGroup.blocksRaycasts = distance <= 2;
            }

            if (distance == 0)
                category.rectTransform.SetAsLastSibling();
        }
    }

    private int GetCarouselOffset(int categoryIndex)
    {
        int offset = categoryIndex - selectedIndex;

        if (!loopCarousel)
            return offset;

        int count = categories.Length;
        int half = count / 2;

        if (offset > half)
            offset -= count;
        else if (offset < -half)
            offset += count;

        return offset;
    }

    private float GetScaleForDistance(int distance)
    {
        if (distance == 0)
            return selectedScale;

        if (distance == 1)
            return sideScale;

        return farScale;
    }

    private float GetAlphaForDistance(int distance)
    {
        if (distance == 0)
            return selectedAlpha;

        if (distance == 1)
            return sideAlpha;

        return farAlpha;
    }

    private void UpdateSelectedCategoryTitle()
    {
        if (selectedCategoryTitleText == null)
            return;

        if (!IsValidIndex(selectedIndex))
            return;

        selectedCategoryTitleText.text = categories[selectedIndex].categoryName;
    }

    private bool IsValidIndex(int index)
    {
        return index >= 0 && index < categories.Length;
    }
}