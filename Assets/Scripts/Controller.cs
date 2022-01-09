using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;
using Yarn.Unity;

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
    [SerializeField] DialogueRunner dialogueRunner;
    //[SerializeField] List<YarnProject> projects = new List<YarnProject>(); 
    [SerializeField] TextMeshProUGUI clockText;
    [SerializeField] TextMeshProUGUI characterHeader;
    [SerializeField] TextMeshProUGUI taskHeader;
    private float rawTime = 720f;
    private float clockHR = 0.0f;
    private float clockMN = 0.0f;
    private string clockAMPM = "AM";
    private int ClockSpeedMultiplier = 500;
    private string oldClockText;
    [SerializeField] public List<GameObject> dicePrefabLibrary = new List<GameObject>();
    private Dictionary<string, GameObject> completedTasks = new Dictionary<string, GameObject>();
    private Dictionary<string, GameObject> taskDictionary = new Dictionary<string, GameObject>();
    private List<GameObject> tasksWithNoStartTime = new List<GameObject>();
    private List<GameObject> activeTasks = new List<GameObject>();
    private List<GameObject> queuedTasks = new List<GameObject>();
    private Dictionary<string, int> stats = new Dictionary<string, int>(){
        {"hygiene", 10},
        {"academic", 10},
        {"parties", 10},
        {"sport", 10},
        {"sleep", 10}
    };
    [SerializeField] public List<GameObject> partyList = new List<GameObject>();
    [SerializeField] private GameObject directionalLight;
    [SerializeField] private GameObject lightPivot;
    [SerializeField] private Canvas victoryCanvas;
    [SerializeField] private int daysToGraduation;
    private int daysPassed = 0;
    private int socialProgression = 0;
    private int academicProgression = 0;
    
    // Start is called before the first frame update
    void Start()
    {
        foreach (GameObject item in partyList)
        {
            item.SetActive(false);
        }
        completedTasks.Add("Sleep", null);

        victoryCanvas.gameObject.SetActive(false);

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
        rawTime += Time.deltaTime * ClockSpeedMultiplier;
        clockHR = (int)rawTime / 60;
        clockMN = (int)rawTime - (int)clockHR * 60;
    
        if (rawTime >= 1440) {
            rawTime = 0;
            daysPassed += 1;
            Debug.Log("Days passed: " + daysPassed);
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
            lightPivot.transform.Rotate(new Vector3(0,0,-0.5f));
        }

        //update numbers in headers
        taskHeader.text = "OPPORTUNITIES " + GameObject.Find("Special Tasks Frame").transform.childCount + "/3";
    
        if (daysPassed >= daysToGraduation){
            endGame();
        }
    }

    private void endGame(){
        if (socialProgression + academicProgression >= 6) {
            //become chancellor
            dialogue("Chancellor");
        }else if (academicProgression + socialProgression <= 2) {
            //drop out
            dialogue("DropOut");
        } else if (socialProgression > academicProgression){
            //social ending
            dialogue("SocialEnding");
        } else if (academicProgression > socialProgression){
            //academic ending
            dialogue("AcademicEnding");
        } else {
            //has broken game, get's recruited by GCHQ
            dialogue("GCHQ");
        }

        victoryCanvas.gameObject.SetActive(true);
        TextMeshProUGUI[] texts = victoryCanvas.GetComponentsInChildren<TextMeshProUGUI>();

        //ignore texts[0]
        texts[1].text = "average sleep: ";
        //TODO: add the rest of the overall stats
    }

    private void checkRepeatedTasks(string text){
        try
        {
            bool activate = true;

            //taskDictionary.Where(p => p.Key == "00:00 AM").ToDictionary(p => p.Key, p => p.Value)
            //if tasks without a set time have met requirements activate them too
            foreach (GameObject item in tasksWithNoStartTime)
            {
                //check it's not already active or completed
                if (!activeTasks.Contains(item) && !completedTasks.ContainsValue(item)){
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
                //check it's not already active or completed
                if (!activeTasks.Contains(item) && !completedTasks.ContainsValue(item)){
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
                //check it's not already active or completed
                if (!activeTasks.Contains(taskInstance) && !completedTasks.ContainsValue(taskInstance)){
                    JSONReader.RepeatedTask task = ((JSONReader.RepeatedTask)taskInstance.GetComponent<RepeatedTaskController>().generic);
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

        if (taskObject.TryGetComponent(out RepeatedTaskController controller)){
            JSONReader.RepeatedTask task = (JSONReader.RepeatedTask)controller.generic;

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

            //effects
            foreach (string effect in task.effects)
            {
                stats[effect.Split()[0]] += int.Parse(effect.Split()[1]);
            }

            dialogue(task.name);            

            // if we want to switch projects
            // dialogueRunner.SetProject(projects.Find(k => k.name == task.name));

        } else if (taskObject.TryGetComponent(out SpecialTaskController controller2)){
            //not a repeated task, so process as a special task
            JSONReader.SpecialTask task = (JSONReader.SpecialTask)controller2.generic;

            //effects
            foreach (string effect in task.effects)
            {
                stats[effect.Split()[0]] += int.Parse(effect.Split()[1]);
            }

            dialogue(task.name);

        } else {
            throw new System.Exception("taskobject doesn't have any UI controller " + taskObject);
        }
        foreach (int value in stats.Values)
        {
            Debug.Log(value);
        }
        //TODO: change stats behind the scene depending on the task
    }

    private void dialogue(string name){
        try
        {
            //disable all dice and start dialogue
            foreach (GameObject dieObject in GameObject.FindGameObjectsWithTag("Dice"))
            {
                dieObject.GetComponent<DieIconProperties>().dialoguePause = true;
            }

            dialogueRunner.StartDialogue(name);
            
        }
        catch (Yarn.DialogueException e)
        {
            Debug.Log("No node matches task name, this could be intentional or not \n" + e);
        }
    }

    [YarnCommand("unpauseDice")]
    public void unpauseDice(){
        //disable all dice and start dialogue
        foreach (GameObject dieObject in GameObject.FindGameObjectsWithTag("Dice"))
        {
            dieObject.GetComponent<DieIconProperties>().dialoguePause = false;
        }
    }

    [YarnCommand("party")]
    public void party(){
        foreach (GameObject item in partyList)
        {
            item.SetActive(true);
        }
        directionalLight.SetActive(false);
    }


}