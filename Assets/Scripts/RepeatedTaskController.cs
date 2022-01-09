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
        texts[2].text = ">= " + task.diceScoreRequirement.ToString();

        AddDiceSlots();
        repeatedTask = true;
        //TODO: start task expiry timer
    }
}
