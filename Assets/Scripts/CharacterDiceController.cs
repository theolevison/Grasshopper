using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CharacterDiceController : MonoBehaviour
{
    [SerializeField] GameObject tenDiePrefab;
    [SerializeReference] protected RectTransform diceSlot;
    public void UpdatePrefab(JSONReader.Character character){
        TextMeshProUGUI[] texts = transform.GetComponentsInChildren<TextMeshProUGUI>();
        
        //update UI text
        texts[0].text = character.name;
        texts[1].text = character.description;
        character.diceSlots = character.dice.Length;

        //add the dice slots to the UI
        foreach (var item in character.dice)
        {
            var diceSlotInstance = Instantiate(diceSlot, transform.Find("DiceSlots"));
            Instantiate(tenDiePrefab, diceSlotInstance); //TODO: get reference to dice from json instead of unity
        }
    }
}
