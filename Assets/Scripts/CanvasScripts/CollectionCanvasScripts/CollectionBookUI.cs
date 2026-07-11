using UnityEngine;
using UnityEngine.UI;

public class CollectionBookUI : MonoBehaviour
{
    [Header("Pages")]
    [SerializeField] private GameObject[] pages;

    [Header("Navigation Buttons")]
    [SerializeField] private Button leftArrowButton;
    [SerializeField] private Button rightArrowButton;

    [Header("Starting Page")]
    [SerializeField] private int startingPageIndex = 0;

    [Header("Mouse Wheel")]
    [SerializeField] private float scrollCooldown = 0.15f;

    private int currentPageIndex;
    private float nextScrollTime;

    private void OnEnable()
    {
        currentPageIndex = Mathf.Clamp(startingPageIndex, 0, pages.Length - 1);
        ShowPage(currentPageIndex);
    }

    private void Update()
    {
        HandleMouseWheel();
    }

    private void HandleMouseWheel()
    {
        if (Time.unscaledTime < nextScrollTime)
            return;

        float scroll = Input.mouseScrollDelta.y;

        if (scroll > 0.1f)
        {
            NextPage();
            nextScrollTime = Time.unscaledTime + scrollCooldown;
        }
        else if (scroll < -0.1f)
        {
            PreviousPage();
            nextScrollTime = Time.unscaledTime + scrollCooldown;
        }
    }

    public void NextPage()
    {
        if (currentPageIndex >= pages.Length - 1)
            return;

        currentPageIndex++;
        ShowPage(currentPageIndex);
    }

    public void PreviousPage()
    {
        if (currentPageIndex <= 0)
            return;

        currentPageIndex--;
        ShowPage(currentPageIndex);
    }

    private void ShowPage(int pageIndex)
    {
        for (int i = 0; i < pages.Length; i++)
        {
            if (pages[i] != null)
                pages[i].SetActive(i == pageIndex);
        }

        UpdateArrowButtons();
    }

    private void UpdateArrowButtons()
    {
        if (leftArrowButton != null)
            leftArrowButton.interactable = currentPageIndex > 0;

        if (rightArrowButton != null)
            rightArrowButton.interactable = currentPageIndex < pages.Length - 1;
    }
}