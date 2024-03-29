using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JSONReader : MonoBehaviour
{
    [SerializeField] TextAsset charactersJSON;

    [System.Serializable]
    public class Generic{
        public string name;
        public string description;
    }


    //Dice
    [System.Serializable]
    public class Character : Generic
    {
        public string[] dice;
    }

    [System.Serializable]
    public class BonusDice : Generic
    {
        public string[] dice;
    }

    [System.Serializable]
    public class DiceList
    {
        //these array names must be the same as the top json names
        public Character[] character;
        public BonusDice[] bonusDice;
    }

    public DiceList myDiceList = new DiceList();


    //Tasks
    [SerializeField] TextAsset tasksJSON;

    [System.Serializable]
    public class Task : Generic 
    {
        public int diceSlots;
        public int diceScoreRequirement;
        public int taskLength;
    }

    [System.Serializable]
    public class RepeatedTask : Task
    {
        public string timeTrigger;
        public int timeLimit;
        public string[] requirements;
        public GameObject uiElement;
    }

    [System.Serializable]
    public class SpecialTask : Task
    {

    }

    [System.Serializable]
    public class TaskList
    {
        //these array names must be the same as the top json names
        public RepeatedTask[] repeatedTask;
        public SpecialTask[] specialTask;
    }

    public TaskList myTaskList = new TaskList();


    public void LoadJSON(){
        myDiceList = JsonUtility.FromJson<DiceList>(charactersJSON.text);
        myTaskList = JsonUtility.FromJson<TaskList>(tasksJSON.text);
    }
}
