using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [SerializeReference] protected RectTransform diceSlot;
    protected const int DICEROLLSPEED = 2000;
    protected List<RectTransform> slots = new List<RectTransform>();
    protected List<Transform> diceIcons = new List<Transform>();
    public JSONReader.Generic generic;
    protected bool repeatedTask = false;
    protected bool specialTask = false;
    protected Controller controller;
    private int cumalativeDiceScore = 0;
    private const float DICEROLLLENGTH = 5f;
    protected void Update() {
        //check if dice slots have been filled, if so roll the dice and report task result to controller
        if (slots.Count > 0){
            //get a list of diceIcons
            foreach (var slot in slots)
            {
                if (slot.childCount>0){
                    diceIcons.Add(slot.GetChild(0));
                }
            }

            //if there are dice in every slot roll them
            if (diceIcons.Count == slots.Count && (repeatedTask || specialTask)){
                //don't let slots be used while dice are being rolled
                foreach (var slot in slots)
                {
                    slot.GetComponent<ItemSlot>().enableSlot = false;
                }

                //roll dice
                foreach (var dieIcon in diceIcons)
                {       
                    //Instantiate the die and let it roll before reporting the result
                    GameObject dice = Instantiate(dieIcon.GetComponent<DieIconProperties>().dieModel);
                    //start the coroutine to respawn dice
                    dieIcon.GetComponent<DieIconProperties>().RespawnDice(((JSONReader.Task)generic).taskLength);
                    
                    dice.GetComponent<Rigidbody>().AddForce(Random.onUnitSphere * DICEROLLSPEED);
                    dice.GetComponent<Transform>().rotation = Random.rotation;
                    StartCoroutine(Roll(dice));
                    
                    //TODO: make die go away for a bit then come back according to task? Or just have each char respawn them automatically after a time period
                }

                Image image = GetComponent<Image>();
                image.color = Color.blue;
                //wait for dice to be rolled
                StartCoroutine(ProcessResults(image));                
            }
            //reset dice count until next update
            diceIcons.Clear();
        }
    }

    public void AddDiceSlots(){
        //add the dice slots to the UI
        controller = GameObject.Find("Controller").GetComponent<Controller>();
        for (int i = 0; i < ((JSONReader.Task)generic).diceSlots; i++)
        {
            RectTransform slot = Instantiate(diceSlot, transform.Find("DiceSlots"));
            slots.Add(slot);
        }
    }

    public void AddDiceSlots(string[] diceNames){
        //add the dice slots to the UI
        controller = GameObject.Find("Controller").GetComponent<Controller>();
        foreach (string item in diceNames)
        {
            RectTransform slot = Instantiate(diceSlot, transform.Find("DiceSlots"));
            slots.Add(slot);
            //put dice into slots
            try
            {
                GameObject die = Instantiate(controller.dicePrefabLibrary.Find(x => x.name == item), slot);
                die.GetComponent<DieIconProperties>().originalParent = slot;
            }
            catch (System.Exception)
            {
                Debug.LogError("Die not instantiated, make sure the dice that you have written in the json has a ui prefab and a die model");
            }
        }
    }

    

    IEnumerator Roll(GameObject dice){
        yield return new WaitForSeconds(DICEROLLLENGTH);
        //send result to controller
        cumalativeDiceScore += dice.GetComponent<DiceStat>().side;
        Destroy(dice);
    }

    IEnumerator ProcessResults(Image image){
        yield return new WaitForSeconds(DICEROLLLENGTH+0.1f);
        
        if (cumalativeDiceScore >= ((JSONReader.Task)generic).diceScoreRequirement){
            //task completed successfully, highlight ui in green
            image.color = Color.green;

            controller.notifyOfTaskCompletion(gameObject, true);
            //hide and reset if it's a repeated task, otherwise destroy because we won't use it again
            if (repeatedTask){
                StartCoroutine(ResetUI(false, 1f, image));
            } else {
                yield return new WaitForSeconds(1f);
                Destroy(gameObject);
            }
        } else {
            //task failed, highlight in red and allow other attempts
            image.color = Color.red;
            StartCoroutine(ResetUI(true, 1f, image));
        }
        cumalativeDiceScore = 0;
    }

    IEnumerator ResetUI(bool bol, float delay, Image uiImage){
        yield return new WaitForSeconds(delay);
        //reset colour of UI
        uiImage.color = Color.white;
        //renable slots
        foreach (var slot in slots){
            slot.GetComponent<ItemSlot>().enableSlot = true;
        }
        gameObject.SetActive(bol);
    }
}