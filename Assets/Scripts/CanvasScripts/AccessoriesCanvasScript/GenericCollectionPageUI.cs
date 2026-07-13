using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GenericCollectionPageUI : MonoBehaviour
{
    [System.Serializable]
    public class CollectionItemPreviewData
    {
        public string itemName;

        [TextArea]
        public string description;

        public Sprite itemSprite;
    }

    [Header("Page Info")]
    [SerializeField] private TMP_Text pageTitleText;
    [SerializeField] private string pageTitle = "TRINKETS";

    [Header("Selected Item Display")]
    [SerializeField] private Image selectedItemImage;
    [SerializeField] private TMP_Text selectedItemDescriptionText;

    [Header("Item Slot Labels")]
    [SerializeField] private TMP_Text[] itemSlotTexts;

    [Header("Temporary Item Data")]
    [SerializeField] private CollectionItemPreviewData[] items;

    private int selectedItemIndex = -1;

    private void OnEnable()
    {
        SetupPage();
    }

    private void SetupPage()
    {
        if (pageTitleText != null)
            pageTitleText.text = pageTitle;

        UpdateItemSlots();

        if (items != null && items.Length > 0)
        {
            SelectItem(0);
        }
        else
        {
            ClearSelectedItem();
        }
    }

    public void SelectItem(int itemIndex)
    {
        if (items == null)
            return;

        if (itemIndex < 0 || itemIndex >= items.Length)
            return;

        selectedItemIndex = itemIndex;

        CollectionItemPreviewData item = items[itemIndex];

        if (selectedItemDescriptionText != null)
            selectedItemDescriptionText.text = item.description;

        if (selectedItemImage != null)
        {
            if (item.itemSprite != null)
            {
                selectedItemImage.enabled = true;
                selectedItemImage.sprite = item.itemSprite;
            }
            else
            {
                selectedItemImage.enabled = true;
            }
        }
    }

    private void UpdateItemSlots()
    {
        for (int i = 0; i < itemSlotTexts.Length; i++)
        {
            if (items != null && i < items.Length)
            {
                itemSlotTexts[i].text = items[i].itemName;
                itemSlotTexts[i].transform.parent.gameObject.SetActive(true);
            }
            else
            {
                itemSlotTexts[i].text = "";
                itemSlotTexts[i].transform.parent.gameObject.SetActive(false);
            }
        }
    }

    private void ClearSelectedItem()
    {
        selectedItemIndex = -1;

        if (selectedItemDescriptionText != null)
            selectedItemDescriptionText.text = "No item selected.";

        if (selectedItemImage != null)
            selectedItemImage.enabled = false;
    }
}