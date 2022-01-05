using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;

public class Controller : MonoBehaviour
{
    [SerializeField] JSONReader jsonReader;
    [SerializeField] GameObject repeatedTaskPrefab;
    [SerializeField] RectTransform repeatedTasksUI;
    [SerializeField] GameObject specialTaskPrefab;
    [SerializeField] RectTransform specialTasksUI;
    [SerializeField] GameObject characterDicePrefab;
    [SerializeField] RectTransform characterDiceUI;
    [SerializeField] GameObject bonusDicePrefab;
    [SerializeField] RectTransform bonusDiceUI;

    [SerializeField] TextMeshProUGUI clockText;
    private float rawTime = 720f;
    private float clockHR = 0.0f;
    private float clockMN = 0.0f;
    private string clockAMPM = "AM";
    private int ClockSpeedMultiplier = 50;
    private string oldClockText;
    [SerializeField] public List<GameObject> dicePrefabLibrary = new List<GameObject>();
    private Dictionary<string, GameObject> completedTasks = new Dictionary<string, GameObject>();
    private Dictionary<string, GameObject> taskDictionary = new Dictionary<string, GameObject>();
    private List<GameObject> tasksWithNoStartTime = new List<GameObject>();
    private List<GameObject> activeTasks = new List<GameObject>();
    private List<GameObject> queuedTasks = new List<GameObject>();

    
    // Start is called before the first frame update
    void Start()
    {
        completedTasks.Add("Sleep", null);

        //parse tasks from JSON
        jsonReader.LoadJSON();
       
       //load regular tasks as inactive, to be enabled by time triggers
        foreach (var task in jsonReader.myTaskList.repeatedTask)
        {
            try
            {
                var taskInstance = Instantiate(repeatedTaskPrefab, repeatedTasksUI);
                taskInstance.GetComponent<RepeatedTaskController>().UpdatePrefab(task);
                taskInstance.SetActive(false);

                if (task.timeTrigger == "00:00 AM"){
                    tasksWithNoStartTime.Add(taskInstance);
                } else {
                    taskDictionary.Add(task.timeTrigger, taskInstance);
                }
                
            }
            catch (System.ArgumentException e)
            {
                Debug.Log(e + " all tasks need a unique start time");
            }
        }

        //load tasks depending on story variables stored in the JSON
        foreach (var task in jsonReader.myTaskList.specialTask)
        {
            var taskInstance = Instantiate(specialTaskPrefab, specialTasksUI);
            taskInstance.GetComponent<SpecialTaskController>().UpdatePrefab(task);
        }

        //load characters and bonus dice depending on story variables stored in the JSON
        foreach (var character in jsonReader.myDiceList.character)
        {
            var characterInstance = Instantiate(characterDicePrefab, characterDiceUI);
            characterInstance.GetComponent<CharacterDiceController>().UpdatePrefab(character);
        }

        foreach (var bonus in jsonReader.myDiceList.bonusDice)
        {
            var bonusInstance = Instantiate(bonusDicePrefab, bonusDiceUI);
            bonusInstance.GetComponent<BonusDiceController>().UpdatePrefab(bonus);
        }
    }

    // Update is called once per frame
    void Update()
    {
        //TODO: implement world clock and trigger regular tasks as specific times
        rawTime += Time.deltaTime * ClockSpeedMultiplier;
        clockHR = (int)rawTime / 60;
        clockMN = (int)rawTime - (int)clockHR * 60;
    
        if (rawTime >= 1440) {
            rawTime = 0;
        }
 
        if (rawTime >= 720) {
            clockAMPM = "PM";
            clockHR -= 12;
        } else {
            clockAMPM = "AM";
        }
 
        if (clockHR < 1 ) {
            clockHR = 12;
        }
        
        clockText.text = clockHR.ToString("00")     + ":" + clockMN.ToString("00") + " " + clockAMPM;
              
        if (clockText.text != oldClockText){
            checkRepeatedTasks(clockText.text);
            oldClockText = clockText.text;
        }

        // if (clockText.text == "09:59 PM"){
        //     Debug.Log("reset");
        //     Debug.Log(currentTasks.Count);
        //     //reset at 10pm
        //     activeTasks = activeTasks.ToDictionary(k => k.Key, k => false);
        //     queuedTasks.Clear();
        //     foreach (GameObject item in currentTasks)
        //     {
        //         item.SetActive(false);
        //     }
        //     currentTasks.Clear();
        // }
    }

    private void checkRepeatedTasks(string text){
        try
        {
            bool activate = true;

            //taskDictionary.Where(p => p.Key == "00:00 AM").ToDictionary(p => p.Key, p => p.Value)
            //if tasks without a set time have met requirements activate them too
            foreach (GameObject item in tasksWithNoStartTime)
            {
                //check it's not already active
                if (!activeTasks.Contains(item)){
                    JSONReader.RepeatedTask task = ((JSONReader.RepeatedTask)item.GetComponent<RepeatedTaskController>().generic);
                    //check all requirements have been fulfilled
                    foreach (string requirement in task.requirements)
                    {
                        //if not break the loop and check the rest
                        if (!completedTasks.ContainsKey(requirement)){
                            activate = false;
                            break;
                        }
                    }
                    if (activate){
                        item.SetActive(true);
                        activeTasks.Add(item);
                    }
                }
            }

            List<GameObject> tasksToRemove = new List<GameObject>();

            //check requirements for tasks with start times that weren't able to start
            foreach (GameObject item in queuedTasks)
            {
                //check it's not already active
                if (!activeTasks.Contains(item)){
                    JSONReader.RepeatedTask task = ((JSONReader.RepeatedTask)item.GetComponent<RepeatedTaskController>().generic);
                    //check all requirements have been fulfilled
                    foreach (string requirement in task.requirements)
                    {
                        //if not break the loop and check the rest
                        if (!completedTasks.ContainsKey(requirement)){
                            activate = false;
                            break;
                        }
                    }
                    if (activate){
                        item.SetActive(true);
                        activeTasks.Add(item);
                        tasksToRemove.Add(item);
                    }
                }
            }

            //must remove queued tasks from outside the previous foreach to avoid modifying collection whilst in use
            foreach (GameObject item in tasksToRemove)
            {
                queuedTasks.Remove(item);
            }
            

            //if current time matches required time instantiate the task
            if (taskDictionary.ContainsKey(text)){
                GameObject taskInstance = taskDictionary[text];
                //check it's not already active
                if (!activeTasks.Contains(taskInstance)){
                    JSONReader.RepeatedTask task = ((JSONReader.RepeatedTask)taskInstance.GetComponent<RepeatedTaskController>().generic);//TODO: please make this better by having the task accessible in UIController or something???
                    //check all requirements have been fulfilled
                    foreach (string requirement in task.requirements)
                    {
                        //if not exit the function
                        if (!completedTasks.ContainsKey(requirement)){
                            queuedTasks.Add(taskInstance);
                            return;
                        }
                    }
                    taskInstance.SetActive(true);
                    activeTasks.Add(taskInstance);
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.Log(e);
        }
    }

    public void notifyOfTaskCompletion(GameObject taskObject, bool success){
        try
        {
            JSONReader.Generic task = taskObject.GetComponent<RepeatedTaskController>().generic;
            activeTasks.Remove(taskObject);

            //reset everything after sleeping
            if (task.name == "Sleep"){
                completedTasks.Clear();

                foreach (GameObject item in activeTasks)
                {
                    item.SetActive(false);
                }
                activeTasks.Clear();
            }
            
            //task has been completed
            completedTasks.Add(task.name, taskObject);
        }
        catch (System.Exception e)
        {
            //not a repeated task, so process as a special task
            Debug.Log(e);
        }
        //TODO: change stats behind the scene depending on the task
    }
}