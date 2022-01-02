using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


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

    private Dictionary<string, JSONReader.RepeatedTask> repeatedTasks = new Dictionary<string, JSONReader.RepeatedTask>();
    private Dictionary<string, bool> taskChecklist = new Dictionary<string, bool>();

    // Start is called before the first frame update
    void Start()
    {
        //parse tasks from JSON
        jsonReader.LoadJSON();
       
       //load regular tasks to be loaded by time triggers
        foreach (var task in jsonReader.myTaskList.repeatedTask)
        {
            try
            {
                repeatedTasks.Add(task.timeTrigger, task);
                taskChecklist.Add(task.name, false);
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
            checkRepeatedTasks();
        }
        oldClockText = clockText.text;
        
    }

    private void checkRepeatedTasks(){
        try
        {
            //if current time matches required time instantiate the task
            JSONReader.RepeatedTask task = repeatedTasks[clockText.text];
            if (!taskChecklist[task.name]){
                //check all requirements have been fulfilled
                foreach (string requirement in task.requirements)
                {
                    if (!taskChecklist[requirement]){
                        throw new System.Exception("not all required tasks have been completed, can't add " + task.name);
                    }
                }
                var taskInstance = Instantiate(repeatedTaskPrefab, repeatedTasksUI);
                //add the ui to the task object so we can reference it easily later
                task.uiElement = taskInstance;
                taskInstance.GetComponent<RepeatedTaskController>().UpdatePrefab(task);
                taskChecklist[task.name] = true;
            }
        }
        catch (KeyNotFoundException)
        {
            //ignore key not found
        }
        catch (System.Exception e)
        {
            Debug.Log(e);
        }
    }

    public void notifyOfTaskCompletion(JSONReader.Generic task, int result){
        Debug.Log(task.name + " " + result);
    }
}