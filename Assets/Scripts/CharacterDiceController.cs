using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CharacterDiceController : UIController
{
    public void UpdatePrefab(JSONReader.Character character){
        generic = character;
        TextMeshProUGUI[] texts = transform.GetComponentsInChildren<TextMeshProUGUI>();
        
        //update UI text
        texts[0].text = character.name;
        texts[1].text = character.description;

        AddDiceSlots(character.dice);
        //defaults to not allow rolls
    }
}
