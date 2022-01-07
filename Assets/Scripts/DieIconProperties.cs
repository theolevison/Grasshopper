using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DieIconProperties : MonoBehaviour
{
    public GameObject dieModel;
    private float diceRespawnMultiplier = 1;
    public RectTransform originalParent;
    private Image cooldownImage;
    private int taskLength;
    public bool canDrag = true;
    private void Start() {
        cooldownImage = transform.GetChild(0).GetComponent<Image>();
    }

    public void RespawnDice(int taskLength){
        canDrag = false;
        this.taskLength = taskLength;
        GetComponent<RectTransform>().SetParent(originalParent);
        cooldownImage.fillAmount = 1;
    }

    private void FixedUpdate() {
        //start cooldown
        if (cooldownImage.fillAmount > 0){
            cooldownImage.fillAmount -= 1 / (diceRespawnMultiplier * taskLength * Time.deltaTime);
            //TODO: make this less stupid but I'm tired and idk how
            if ((cooldownImage.fillAmount -= 1 / (diceRespawnMultiplier * taskLength * Time.deltaTime)) <= 0){
                canDrag = true;
                cooldownImage.fillAmount = 0;
            }
            
        }
    }
}
