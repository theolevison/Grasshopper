using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIController : MonoBehaviour
{
    [SerializeReference] protected RectTransform diceSlot;
    protected const int DICEROLLSPEED = 2000;
    protected List<RectTransform> slots = new List<RectTransform>();
    protected List<Transform> diceIcons = new List<Transform>();
    protected JSONReader.Generic generic;
    protected bool diceSpawn = true;
    protected void Update() {
        //check if dice slots have been filled, if so roll the dice and report task result to controller
        foreach (var slot in slots)
        {
            if (slot.childCount>0){
                diceIcons.Add(slot.GetChild(0));
            }
        }
        if (diceIcons.Count == generic.diceSlots && diceSpawn){
            //roll dice
            foreach (var dieIcon in diceIcons)
            {       
                //Instantiate the die and let it roll before reporting the result
                GameObject dice = Instantiate(dieIcon.GetComponent<DieIconProperties>().dieModel);
                //Destroy Icon so it can't be re-used
                Destroy(dieIcon.gameObject);
                dice.GetComponent<Rigidbody>().AddForce(Random.onUnitSphere * DICEROLLSPEED);
                dice.GetComponent<Transform>().rotation = Random.rotation;
                StartCoroutine(Roll(generic, dice));
                //TODO: make die go away for a bit then come back according to task? Or just have each char respawn them automatically after a time period
            }
            diceSpawn = false;
        }
        diceIcons.Clear();
    }

    IEnumerator Roll(JSONReader.Generic task, GameObject dice){
        yield return new WaitForSeconds(5f);//TODO: change from time limit to task length once that's implemented
        //send result to controller
        GameObject.Find("Controller").GetComponent<Controller>().notifyOfTaskCompletion(task, dice.GetComponent<DiceStat>().side);
        Destroy(dice);
    }
}