using TMPro;
using UnityEngine;

public class CharacterCollectionPageUI : MonoBehaviour
{
    [System.Serializable]
    public class CharacterPreviewData
    {
        public string characterName;
        public int shield;
        public int heart;
        public int sword;
        public int clover;
        [TextArea] public string description;
    }

    [Header("Left Page Display")]
    [SerializeField] private TMP_Text shieldText;
    [SerializeField] private TMP_Text heartText;
    [SerializeField] private TMP_Text swordText;
    [SerializeField] private TMP_Text cloverText;
    [SerializeField] private TMP_Text descriptionText;

    [Header("Character Slot Labels")]
    [SerializeField] private TMP_Text[] characterSlotTexts;

    [Header("Temporary Character Data")]
    [SerializeField] private CharacterPreviewData[] fireCharacters;
    [SerializeField] private CharacterPreviewData[] waterCharacters;
    [SerializeField] private CharacterPreviewData[] airCharacters;
    [SerializeField] private CharacterPreviewData[] earthCharacters;
    [SerializeField] private CharacterPreviewData[] lifeCharacters;

    private CharacterPreviewData[] currentElementCharacters;

    private void OnEnable()
    {
        SelectElement(0);
    }

    public void SelectElement(int elementIndex)
    {
        switch (elementIndex)
        {
            case 0:
                currentElementCharacters = fireCharacters;
                break;

            case 1:
                currentElementCharacters = waterCharacters;
                break;

            case 2:
                currentElementCharacters = airCharacters;
                break;

            case 3:
                currentElementCharacters = earthCharacters;
                break;

            case 4:
                currentElementCharacters = lifeCharacters;
                break;

            default:
                currentElementCharacters = fireCharacters;
                break;
        }

        UpdateCharacterSlots();

        if (currentElementCharacters != null && currentElementCharacters.Length > 0)
        {
            SelectCharacter(0);
        }
        else
        {
            ClearLeftPage();
        }
    }

    public void SelectCharacter(int characterIndex)
    {
        if (currentElementCharacters == null)
            return;

        if (characterIndex < 0 || characterIndex >= currentElementCharacters.Length)
            return;

        CharacterPreviewData character = currentElementCharacters[characterIndex];

        shieldText.text = "Shield: " + character.shield;
        heartText.text = "Heart: " + character.heart;
        swordText.text = "Sword: " + character.sword;
        cloverText.text = "Clover: " + character.clover;

        descriptionText.text = character.description;
    }

    private void UpdateCharacterSlots()
    {
        for (int i = 0; i < characterSlotTexts.Length; i++)
        {
            if (currentElementCharacters != null && i < currentElementCharacters.Length)
            {
                characterSlotTexts[i].text = currentElementCharacters[i].characterName;
                characterSlotTexts[i].transform.parent.gameObject.SetActive(true);
            }
            else
            {
                characterSlotTexts[i].text = "";
                characterSlotTexts[i].transform.parent.gameObject.SetActive(false);
            }
        }
    }

    private void ClearLeftPage()
    {
        shieldText.text = "Shield: -";
        heartText.text = "Heart: -";
        swordText.text = "Sword: -";
        cloverText.text = "Clover: -";
        descriptionText.text = "No character selected.";
    }
}