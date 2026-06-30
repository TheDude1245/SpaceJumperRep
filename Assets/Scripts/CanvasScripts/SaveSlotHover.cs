using UnityEngine;
using UnityEngine.EventSystems;

public class SaveSlotHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler
{
    [SerializeField] private SaveSelectUI saveSelectUI;
    [SerializeField] private int saveIndex;

    public void OnPointerEnter(PointerEventData eventData)
    {
        saveSelectUI.PreviewSave(saveIndex);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        saveSelectUI.RestoreSelectedSavePreview();
    }

    public void OnSelect(BaseEventData eventData)
    {
        saveSelectUI.PreviewSave(saveIndex);
    }
}