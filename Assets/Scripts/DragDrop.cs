using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DragDrop : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    private AudioSource audioSource;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Transform tempParent;
    private Canvas canvas;
    public bool slotChange = false;
    private DieIconProperties dip;
    private Controller controller;
    private void Awake() {
        controller = GameObject.Find("Controller").GetComponent<Controller>();
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        canvas = GameObject.Find("UICanvas").GetComponent<Canvas>();
        audioSource = GameObject.Find("UICanvas").GetComponent<AudioSource>();
        dip = GetComponent<DieIconProperties>();
        tempParent = rectTransform.parent;
    }
    //called every frame during drag
    public void OnDrag(PointerEventData eventData){
        if (dip.canDrag && !dip.dialoguePause && !controller.sleeping) {
            //make object follow cursor
            rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
        }
    }

    public void OnBeginDrag(PointerEventData eventData){
        if (dip.canDrag && !dip.dialoguePause && !controller.sleeping) {
            audioSource.clip = (AudioClip) Resources.Load("takeDice");
            audioSource.Play();
            tempParent = rectTransform.parent;
            rectTransform.SetParent(canvas.transform);
            canvasGroup.blocksRaycasts = false;
            canvasGroup.alpha = 0.6f;
        }
    }

    public void OnEndDrag(PointerEventData eventData){
        if (dip.canDrag && !dip.dialoguePause && !controller.sleeping) {
            audioSource.clip = (AudioClip) Resources.Load("dropDice");
            audioSource.Play();
            canvasGroup.blocksRaycasts = true;
            canvasGroup.alpha = 1f;
            if (!slotChange){
                rectTransform.SetParent(tempParent);
            }
            slotChange = false;
        }
    }

    public void OnPointerDown(PointerEventData eventData){
        
    }

    public void reset(){
        rectTransform.SetParent(dip.originalParent);
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;
    }
}
