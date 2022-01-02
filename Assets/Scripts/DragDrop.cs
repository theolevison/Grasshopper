using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DragDrop : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Transform originalParent;
    private Canvas canvas;
    public bool slotChange = false;

    private void Awake() {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
    }
    //called every frame during drag
    public void OnDrag(PointerEventData eventData){
        //make object follow cursor
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnBeginDrag(PointerEventData eventData){
        originalParent = rectTransform.parent;
        rectTransform.SetParent(canvas.transform);
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.6f;
    }

    public void OnEndDrag(PointerEventData eventData){
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;
        if (!slotChange){
            rectTransform.SetParent(originalParent);
        }
        slotChange = false;
    }

    public void OnPointerDown(PointerEventData eventData){
        
    }
}
