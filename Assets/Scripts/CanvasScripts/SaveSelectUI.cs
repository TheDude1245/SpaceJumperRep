using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveSelectUI : MonoBehaviour
{
    [Header("Info")]
    [SerializeField] private TMP_Text saveInfoText;

    [Header("Save Slot Highlights")]
    [SerializeField] private GameObject[] saveSlotHighlights;

    public void SelectSave(int saveIndex)
    {
        for (int i = 0; i < saveSlotHighlights.Length; i++)
        {
            saveSlotHighlights[i].SetActive(i == saveIndex);
        }

        if (saveIndex == 0)
            saveInfoText.text = "Goblin Save";
        else if (saveIndex == 1)
            saveInfoText.text = "Sigma Save";
        else if (saveIndex == 2)
            saveInfoText.text = "Troll Save";
        else if (saveIndex == 3)
            saveInfoText.text = "Zogma Save";
    }

    public void LoadSelectedSave()
    {
        SceneManager.LoadScene("Map_1_Scene");
    }
}