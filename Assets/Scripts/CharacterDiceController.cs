using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CharacterDiceController : UIController
{
    public JSONReader.Character character;
    public void UpdatePrefab(JSONReader.Character character){
        this.character = character;
        generic = character;
        TextMeshProUGUI[] texts = transform.GetComponentsInChildren<TextMeshProUGUI>();
        
        //update UI text
        texts[0].text = character.name;
        texts[1].text = character.description;
        
        texts[2].text = character.likes[0];
        texts[3].text = character.likes[1];

        texts[4].text = character.dislikes[0];
        texts[5].text = character.dislikes[1];

        AddDiceSlots(character.dice);
        //defaults to not allow rolls
    }
}
