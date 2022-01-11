using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SpecialTaskController : UIController
{
    public JSONReader.SpecialTask task;
    public void UpdatePrefab(JSONReader.SpecialTask task){
        this.task = task;
        generic = task;
        TextMeshProUGUI[] texts = transform.GetComponentsInChildren<TextMeshProUGUI>();
        
        //update UI text
        texts[0].text = task.name;
        texts[1].text = task.description;
        texts[2].text = ">= " + task.diceScoreRequirement.ToString();

        AddDiceSlots();
        specialTask = true;
    }
}
