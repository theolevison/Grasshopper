using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RepeatedTaskController : UIController
{  
    public void UpdatePrefab(JSONReader.RepeatedTask task){
        generic = task;
        TextMeshProUGUI[] texts = transform.GetComponentsInChildren<TextMeshProUGUI>();
        
        //update UI text
        texts[0].text = task.name;
        texts[1].text = task.description;

        //add the dice slots to the UI
        for (int i = 0; i < task.diceSlots; i++)
        {
            RectTransform slot = Instantiate(diceSlot, transform.Find("DiceSlots"));
            slots.Add(slot);
        }

        //TODO: start task expiry timer
    }
}
