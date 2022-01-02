using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BonusDiceController : MonoBehaviour
{
    [SerializeReference] protected RectTransform diceSlot;
    public void UpdatePrefab(JSONReader.BonusDice bonusDice){
        TextMeshProUGUI[] texts = transform.GetComponentsInChildren<TextMeshProUGUI>();
        
        //update UI text
        texts[0].text = bonusDice.name;
        bonusDice.diceSlots = bonusDice.dice.Length;

        //add the dice slots to the UI
        foreach (var item in bonusDice.dice)
        {
            Instantiate(diceSlot, transform.Find("DiceSlots"));
        }
    }
}