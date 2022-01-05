using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BonusDiceController : UIController
{
    public void UpdatePrefab(JSONReader.BonusDice bonusDice){
        generic = bonusDice;
        TextMeshProUGUI[] texts = transform.GetComponentsInChildren<TextMeshProUGUI>();
        
        //update UI text
        texts[0].text = bonusDice.name;

        AddDiceSlots(bonusDice.dice);
        //defaults to not allow rolls
    }
}