using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SpecialTaskController : UIController
{
    public void UpdatePrefab(JSONReader.SpecialTask task){
        generic = task;
        TextMeshProUGUI[] texts = transform.GetComponentsInChildren<TextMeshProUGUI>();
        
        //update UI text
        texts[0].text = task.name;
        texts[1].text = task.description;

        AddDiceSlots();
        specialTask = true;
    }
}
