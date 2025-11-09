using UnityEngine;
using UnityEngine.EventSystems;

public class ForgeDragItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Canvas canvas;
    private RectTransform rt;
    private CanvasGroup cg;
    private Vector3 startPos;

    void Awake(){ rt=GetComponent<RectTransform>(); cg=gameObject.AddComponent<CanvasGroup>(); }
    public void OnBeginDrag(PointerEventData e){ startPos=rt.position; cg.blocksRaycasts=false; }
    public void OnDrag(PointerEventData e){ rt.anchoredPosition += e.delta / canvas.scaleFactor; }
    public void OnEndDrag(PointerEventData e){ rt.position=startPos; cg.blocksRaycasts=true; }
}
