using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_ButtonDragPass : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public UI_SubItem parent;
    private void Awake()
    {
        parent = Utils.FindAncestor<UI_SubItem>(gameObject);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (parent != null)
            parent.OnBeginDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (parent != null)
            parent.OnDrag(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (parent != null)
            parent.OnEndDrag(eventData);
    }
}
