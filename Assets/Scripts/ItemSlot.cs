using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ItemSlot : MonoBehaviour, IDropHandler
{
    private bool enableSlot = true;
    public bool taskSlot = true; //if slot should actually accept dice, only tasks should
    public void OnDrop(PointerEventData eventData){
        //make dropped item snap into position
        if (eventData.pointerDrag != null && enableSlot && !eventData.pointerDrag.GetComponent<DieIconProperties>().dialoguePause && eventData.pointerDrag.GetComponent<DieIconProperties>().canDrag){
            //check to make sure only one dice can be put in each slot
            if (this.transform.childCount == 0)
            {
                //set parent of dice to a dice slot
                eventData.pointerDrag.GetComponent<RectTransform>().SetParent(GetComponent<RectTransform>());
                eventData.pointerDrag.GetComponent<DragDrop>().slotChange = true;
            }
        }
    }

    public void resetSlot(){
        enableSlot = taskSlot;
    }

    public void disableSlot(){
        enableSlot = false;
    }
}
