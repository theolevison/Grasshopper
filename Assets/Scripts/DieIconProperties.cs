using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DieIconProperties : MonoBehaviour
{
    public GameObject dieModel;
    public float diceRespawnMultiplier = 1;
    public RectTransform originalParent;
    private GameObject diceGraveyard;

    private void Start() {
        diceGraveyard = GameObject.Find("DiceGraveyard");
    }

    public void RespawnDice(int taskLength){
        StartCoroutine(RespawnDiceEnumerator(taskLength));
    }
    public IEnumerator RespawnDiceEnumerator(int taskLength){
        GetComponent<RectTransform>().SetParent(diceGraveyard.GetComponent<RectTransform>());
        yield return new WaitForSeconds(diceRespawnMultiplier * taskLength);
        GetComponent<RectTransform>().SetParent(originalParent);
    }
}
