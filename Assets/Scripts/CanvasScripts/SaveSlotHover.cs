using UnityEngine;
using UnityEngine.EventSystems;

public class SaveSlotHover : MonoBehaviour, IPointerEnterHandler, ISelectHandler
{
    [SerializeField] private SaveSelectUI saveSelectUI;
    [SerializeField] private int saveIndex;

    public void OnPointerEnter(PointerEventData eventData)
    {
        saveSelectUI.PreviewSave(saveIndex);
    }

    public void OnSelect(BaseEventData eventData)
    {
        saveSelectUI.PreviewSave(saveIndex);
    }
}