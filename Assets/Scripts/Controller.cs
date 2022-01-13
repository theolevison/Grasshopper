using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;
using Yarn.Unity;
using System;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class Controller : GenericSingletonClass<Controller>
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
    [SerializeField] TextMeshProUGUI clockText;
    [SerializeField] TextMeshProUGUI characterHeader;
    [SerializeField] TextMeshProUGUI taskHeader;
    [SerializeField] Image tiredImage;
    [SerializeField] GameObject normalRoom;
    private float rawTime = 720f;
    private float clockHR = 0.0f;
    private float clockMN = 0.0f;
    private string clockAMPM = "AM";
    private int ClockSpeedMultiplier = 50;
    private string oldClockText;
    public bool sleeping = false;
    [SerializeField] public List<GameObject> dicePrefabLibrary = new List<GameObject>();
    private Dictionary<string, GameObject> completedTasks = new Dictionary<string, GameObject>();
    private List<GameObject> completedSpecialTasks = new List<GameObject>();
    private Dictionary<string, GameObject> taskDictionary = new Dictionary<string, GameObject>();
    private List<GameObject> tasksWithNoStartTime = new List<GameObject>();
    private List<GameObject> activeTasks = new List<GameObject>();
    private List<GameObject> queuedTasks = new List<GameObject>();
    private List<GameObject> characters = new List<GameObject>();
    private List<string> activeCharacters = new List<string>();
    private List<string> usedDialogue = new List<string>();
    private List<GameObject> specialTasks = new List<GameObject>();
    private Queue<(string, bool)> queuedDialogue = new Queue<(string, bool)>();
    [SerializeField] List<GameObject> sounds = new List<GameObject>();
    public Dictionary<string, int> stats = new Dictionary<string, int>(){
        {"badHygiene", 0},
        {"academic", 0},
        {"parties", 0},
        {"sleep", 0},
        {"socialProgression", 0},
        {"academicProgression", 0}
    };
    [SerializeField] public List<GameObject> partyList = new List<GameObject>();
    [SerializeField] public List<GameObject> sleepList = new List<GameObject>();
    [SerializeField] public List<GameObject> workList = new List<GameObject>();
    [SerializeField] private GameObject directionalLight;
    [SerializeField] private GameObject lightPivot;
    [SerializeField] private Canvas victoryCanvas;
    [SerializeField] private int daysToGraduation;
    [SerializeField] private TextMeshProUGUI daysToGraduationText;
    public int daysPassed = 0;
    private bool tutorialCompleted = false;
    [SerializeField] private float smoothSpeed;
    [SerializeField] private float maxSpeed;
    private Vector3 speed = Vector2.zero;
    private float slope = 0.75f;//(5 - 20) / (-20);
    
    // Start is called before the first frame update
    void Start()
    {
        daysToGraduationText.text = "DAYS TO GRADUATION      " + daysToGraduation.ToString();

        dialogueRunner.onNodeComplete.AddListener(runQueuedDialogue);

        resetBedroom();
        sounds.First(k => k.name == "RelaxingAndy").SetActive(true);
        directionalLight.SetActive(true);

        completedTasks.Add("Wakeup", null);

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
            if (task.name.Equals("ECSJumpstart"))
            {
                var taskObject = Instantiate(specialTaskPrefab, specialTasksUI);
                taskObject.GetComponent<SpecialTaskController>().UpdatePrefab(task);
                activeTasks.Add(taskObject);
            } else {
                var taskObject = Instantiate(specialTaskPrefab, specialTasksUI);
                taskObject.GetComponent<SpecialTaskController>().UpdatePrefab(task);
                taskObject.SetActive(false);
                specialTasks.Add(taskObject);
            }
        }

        //load characters and bonus dice depending on story variables stored in the JSON
        foreach (var character in jsonReader.myDiceList.character)
        {
            var characterInstance = Instantiate(characterDicePrefab, characterDiceUI);
            characterInstance.GetComponent<CharacterDiceController>().UpdatePrefab(character);
            characterInstance.SetActive(false);
            characters.Add(characterInstance);
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
        float output = 20 + (slope * stats["sleep"]);
        
        tiredImage.transform.localScale = new Vector3(output, output, 1);
        //tired effect

        Vector2 desiredPosition = Mouse.current.position.ReadValue();
        Vector2 smoothedPosition = Vector3.SmoothDamp(tiredImage.transform.position, desiredPosition, ref speed, smoothSpeed * Time.deltaTime, maxSpeed);
        if (tutorialCompleted){
            tiredImage.transform.position = smoothedPosition;
        }

        rawTime += Time.deltaTime * ClockSpeedMultiplier;
        clockHR = (int)rawTime / 60;
        clockMN = (int)rawTime - (int)clockHR * 60;

        if (rawTime >= 1440)
        {
            rawTime = 0;
            if (tutorialCompleted){
                daysPassed += 1;

                //should not lose sleep or hygiene whilst sleeping
                if (!completedTasks.Keys.Contains("Sleep")){
                    //don't get tired below this
                    if (stats["sleep"] > -20){
                        stats["sleep"] -= 5;
                    }
                    if (stats["sleep"] <= -10){
                        dialogue("Tired", true);
                    }
                    if (stats["badHygiene"] < 30){
                        stats["badHygiene"] += 5;
                    }
                }
                if (stats["sleep"] > 20){
                        stats["sleep"] = 20;
                    }
                
                daysToGraduationText.text =  "DAYS TO GRADUATION      " + (daysToGraduation - daysPassed).ToString();
            }
        }

        if (rawTime >= 720)
        {
            clockAMPM = "PM";
            clockHR -= 12;
        }
        else
        {
            clockAMPM = "AM";
        }

        if (clockHR < 1)
        {
            clockHR = 12;

        }

        clockText.text = clockHR.ToString("00") + ":" + clockMN.ToString("00") + " " + clockAMPM;

        if (clockText.text != oldClockText)
        {
            checkRepeatedTasks(clockText.text);
            oldClockText = clockText.text;
            lightPivot.transform.Rotate(new Vector3(0, 0, -0.5f));
        }

        //update numbers in headers
        int count = 0;
        foreach (Transform child in GameObject.Find("Special Tasks Frame").transform)
        {
            if (child.gameObject.activeSelf){
                count++;
            }
        }
        taskHeader.text = "OPPORTUNITIES " + count + "/3";

        //update character numbers in headers
        count = 0;
        foreach (Transform child in GameObject.Find("Characters Dice Frame").transform)
        {
            if (child.gameObject.activeSelf){
                count++;
            }
        }
        characterHeader.text = "CHARACTERS " + count + "/3";

        if (daysPassed >= daysToGraduation)
        {
            endGame();
            daysPassed = 0;
        }
    }

    [YarnCommand("endGame")]
    private void endGame(){
        //make sure the end game dialogue is displayed
        dialogueRunner.Stop();

        if (stats["academicProgression"] + stats["socialProgression"] >= 6) {
            //become chancellor
            dialogue("Chancellor", true);
        }else if (stats["academicProgression"] + stats["socialProgression"] <= 2) {
            //drop out
            dialogue("DropOut", true);
        } else if (stats["socialProgression"] > stats["academicProgression"]){
            //social ending
            dialogue("SocialEnding", true);
        } else if (stats["academicProgression"] > stats["socialProgression"]){
            //academic ending
            dialogue("AcademicEnding", true);
        } else {
            //has broken game, get's recruited by GCHQ
            dialogue("GCHQ", true);
        }

        victoryCanvas.gameObject.SetActive(true);
        TextMeshProUGUI[] texts = victoryCanvas.GetComponentsInChildren<TextMeshProUGUI>();

        //ignore texts[0]
        texts[1].text = stats.Keys.ToList()[0] + ": " + stats.Values.ToList()[0];
        texts[2].text = stats.Keys.ToList()[1] + ": " + stats.Values.ToList()[1];
        texts[3].text = stats.Keys.ToList()[2] + ": " + stats.Values.ToList()[2];
        texts[4].text = stats.Keys.ToList()[3] + ": " + stats.Values.ToList()[3];
        texts[5].text = stats.Keys.ToList()[4] + ": " + stats.Values.ToList()[4];
    }

    private void checkRepeatedTasks(string text){
        try
        {
            bool activate = true;

            //taskDictionary.Where(p => p.Key == "00:00 AM").ToDictionary(p => p.Key, p => p.Value)
            //if tasks without a set time have met requirements activate them too
            foreach (GameObject taskObject in tasksWithNoStartTime)
            {
                //check it's not already active or completed
                if (!activeTasks.Contains(taskObject) && !completedTasks.ContainsValue(taskObject)){
                    JSONReader.RepeatedTask task = ((JSONReader.RepeatedTask)taskObject.GetComponent<RepeatedTaskController>().generic);
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
                        taskObject.SetActive(true);
                        activeTasks.Add(taskObject);
                    }
                }
            }

            List<GameObject> tasksToRemove = new List<GameObject>();

            //check requirements for tasks with start times that weren't able to start
            foreach (GameObject taskObject in queuedTasks)
            {
                //check it's not already active or completed
                if (!activeTasks.Contains(taskObject) && !completedTasks.ContainsValue(taskObject)){
                    JSONReader.RepeatedTask task = ((JSONReader.RepeatedTask)taskObject.GetComponent<RepeatedTaskController>().generic);
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
                        taskObject.SetActive(true);
                        activeTasks.Add(taskObject);
                        tasksToRemove.Add(taskObject);
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
                GameObject taskObject = taskDictionary[text];
                //check it's not already active or completed
                if (!activeTasks.Contains(taskObject) && !completedTasks.ContainsValue(taskObject)){
                    JSONReader.RepeatedTask task = ((JSONReader.RepeatedTask)taskObject.GetComponent<RepeatedTaskController>().generic);

                    //check all requirements have been fulfilled
                    foreach (string requirement in task.requirements)
                    {
                        //if not exit the function
                        if (!completedTasks.ContainsKey(requirement)){
                            queuedTasks.Add(taskObject);
                            return;
                        }
                    }
                    taskObject.SetActive(true);
                    activeTasks.Add(taskObject);
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

                foreach (var item in activeTasks)
                {
                    item.SetActive(false);
                }

                resetBedroom();
                normalRoom.SetActive(true);
                sleepList.ForEach(k => k.SetActive(true));
                sounds.First(k => k.name == "RelaxingAndy").SetActive(true);
                directionalLight.SetActive(true);

                activeTasks.Clear();

                completedTasks.Remove("Sleep");
            } else if (task.name == "Wakeup"){
                if (!tutorialCompleted){
                    tutorialCompleted = true;
                }
                if (tutorialCompleted){
                    
                    //only update special tasks once a day
                    checkSpecialTasks();
                }

                resetBedroom();
                normalRoom.SetActive(true);
                sounds.First(k => k.name == "RelaxingAndy").SetActive(true);
                directionalLight.SetActive(true);

                //so we can sleep again
                completedTasks.Remove("Sleep");
            }
            
            //task has been completed
            completedTasks.Add(task.name, taskObject);


            //effects
            foreach (string effect in task.effects)
            {
                stats[effect.Split()[0]] += int.Parse(effect.Split()[1]);
            }

            dialogue(task.name.Replace(" ", ""), true);            

            // if we want to switch projects
            // dialogueRunner.SetProject(projects.Find(k => k.name == task.name));

        } else if (taskObject.TryGetComponent(out SpecialTaskController controller2)){
            //not a repeated task, so process as a special task
            JSONReader.SpecialTask task = (JSONReader.SpecialTask)controller2.generic;

            //add to special tasks completed list
            completedSpecialTasks.Add(taskObject);

            //remove task from active tasks
            activeTasks.Remove(taskObject);

            //special tasks that invoke progress
            //TODO: update social progression with methods from yarn
            // if (task.name.Equals("FlatParty3")) stats["socialProgression"] = 1;
            // if (task.name.Equals("Stags")) stats["socialProgression"] = 2;
            // if (task.name.Equals("Jesters")) stats["socialProgression"] = 3;

            //effects
            foreach (string effect in task.effects)
            {
                stats[effect.Split()[0]] += int.Parse(effect.Split()[1]);
            }

            dialogue(task.name.Replace(" ", ""), true);
        } else {
            throw new System.Exception("taskobject doesn't have any UI controller " + taskObject);
        }

        if (tutorialCompleted){
            updateCharacters();
        }
    }

    public void dialogue(string name, bool allowReuse){
        try
        {
            //check dialogue has not played before
            if (!usedDialogue.Contains(name)) {
                if (!dialogueRunner.IsDialogueRunning){
                    //disable all dice and start dialogue
                    foreach (GameObject dieObject in GameObject.FindGameObjectsWithTag("Dice"))
                    {
                        dieObject.GetComponent<DragDrop>().reset();
                        dieObject.GetComponent<DieIconProperties>().dialoguePause = true;
                    }

                    dialogueRunner.StartDialogue(name);

                    if (!allowReuse){
                        usedDialogue.Add(name);
                    }
                } else {
                    //queue dialogue, don't allow repeat queueing
                    if (!queuedDialogue.Contains((name, allowReuse))) {
                        Debug.Log("Queued dialogue: " + name);
                        queuedDialogue.Enqueue((name, allowReuse));
                    }
                }
            }
        }
        catch (Yarn.DialogueException e)
        {
            Debug.Log("No node matches task name, this could be intentional or not \n" + e);
        }
    }

    private void runQueuedDialogue(string nodeName){
        StartCoroutine(rqd(nodeName));
    }

    IEnumerator rqd(string nodeName){
        yield return new WaitForSeconds(1f);
        
        if (queuedDialogue.Count > 0){
            var item = queuedDialogue.Dequeue();
            Debug.Log(item.Item1 + " was dequeued after " + nodeName);
            dialogue(item.Item1, item.Item2);
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
    public void party()
    {
        resetBedroom();
        normalRoom.SetActive(true);
        partyList.ForEach(k => k.SetActive(true));
        sounds.First(k => k.name == "PartySpeaker").SetActive(true);
        directionalLight.SetActive(false);
    }

    [YarnCommand("library")]
    public void library()
    {
        resetBedroom();
        normalRoom.SetActive(false);
        workList.ForEach(k => k.SetActive(true));
        sounds.First(k => k.name == "LibrarySpeaker").SetActive(true);
        directionalLight.SetActive(true);
    }

    [YarnCommand("sickAndy")]
    public void sickAndy()
    {
        sounds.ForEach(k => k.SetActive(false));
        sounds.First(k => k.name == "SickAndy").SetActive(true);
    }

    [YarnCommand("changeStat")]
    public void changeStat(string statChange)
    {
        stats[statChange.Split()[0]] += int.Parse(statChange.Split()[1]);
    }

    //loads a special task by name
    private void runSpecialTask(GameObject task)
    {
        if (specialTasks.Contains(task) && !activeTasks.Contains(task) && !completedSpecialTasks.Contains(task))
        {
            task.SetActive(true);
            activeTasks.Add(task);
        } else {
            Debug.Log("couldn't run task"); //TODO: either get rid of old completed tasks, or check if they can be run, or allow repeated normi tasks if there's no other options
        }
    }
    
    //checks if requirements of special tasks have been met and activates a selection of them
    private void checkSpecialTasks(){
        //deactivate all tasks that still exist
        specialTasks.ForEach(k => k.SetActive(false));

        List<(int, GameObject)> potentialList = new List<(int, GameObject)>();
        foreach (GameObject taskObject in specialTasks.Except(completedSpecialTasks))
        {
            JSONReader.SpecialTask task = taskObject.GetComponent<SpecialTaskController>().task;
            bool check = true;
            foreach (string requirement in task.requirements)
            {
                //check if the requirement is a character or a stat
                string[] reqs = requirement.Split();
                if (int.TryParse(reqs[1], out _))
                {
                    if (requirement.Split()[0].Equals("academicProgression") || requirement.Split()[0].Equals("socialProgression"))
                    {
                        if (int.Parse(requirement.Split()[1]) == stats[requirement.Split()[0]])
                        {
                            //stat requirement met, do nothing
                        }
                        else
                        {
                            //character requirement not met
                            check = false;
                            break;
                        }
                    }
                    else
                    {
                        if (int.Parse(requirement.Split()[1]) <= stats[requirement.Split()[0]])
                        {
                            //stat requirement met, do nothing
                        }
                        else
                        {
                            //character requirement not met
                            check = false;
                            break;
                        }
                    }

                } else if (activeCharacters.Contains(requirement)){
                    //character requirement met, do nothing
                } else {
                    //character requirement not met
                    check = false;
                    break;
                }                
            }
            if (check){
                //add task to potential list
                potentialList.Add((task.priority, taskObject));
            }
        }
        
        Debug.Log(string.Join(Environment.NewLine ,potentialList.OrderBy(k => k.Item1).Select(kvp => $"{kvp.Item1} : {kvp.Item2.GetComponent<SpecialTaskController>().generic.name}")));

        //select top two
        var list = potentialList.OrderBy(k => k.Item1).GroupBy(k => k.Item1).First().ToList();
        int i = UnityEngine.Random.Range(0, list.Count);
        if (list.Count > 0){
            runSpecialTask(list[i].Item2);
            list.Remove(list[i]);
            if (list.Count > 0){
                i = UnityEngine.Random.Range(0, list.Count);
                runSpecialTask(list[i].Item2);
                list.Remove(list[i]);
            }
        }
        
        //then select a random bottom tier
        list = potentialList.OrderBy(k => k.Item1).GroupBy(k => k.Item1).Last().ToList();
        if (list.Count > 0){
            runSpecialTask(list[UnityEngine.Random.Range(0, list.Count)].Item2);
        }
        
        Debug.Log(string.Join(Environment.NewLine ,stats.Select(kvp => $"{kvp.Key} : {kvp.Value}")));
    }

    private void updateCharacters(){
        foreach (GameObject characterObject in characters)
        {
            
            JSONReader.Character character = characterObject.GetComponent<CharacterDiceController>().character;
            //Debug.Log(character.name + ": " + characterObject.activeInHierarchy);
            int totalLikes = 0;
            int totalDislikes = 0;
            foreach (string like in character.likes)
            {
                if (like != ""){
                    totalLikes += stats[like];
                }
            }
            foreach (string dislike in character.dislikes)
            {
                if (dislike != ""){
                    totalDislikes += stats[dislike];
                }
            }
            int opinion = totalLikes - totalDislikes;
            
            if (opinion >= character.gainFriendThresholds[0]){
                //gain friend
                if (!characterObject.activeSelf){
                    Debug.Log(character.name + " is " + characterObject.activeSelf + " " + characterObject.activeInHierarchy);
                    characterObject.SetActive(true);
                    activeCharacters.Add(character.name);
                    dialogue("Gain"+character.name.Replace(" ", ""), false);
                }
            }else if (opinion >= character.gainFriendThresholds[1]){
                //meet friend
                dialogue("Meet"+character.name.Replace(" ", ""), false);
            } else if (opinion <= character.loseFriendThresholds[0]){
                //lose friend
                if (characterObject.activeSelf){
                    characterObject.SetActive(false);
                    activeCharacters.Remove(character.name);
                    dialogue("Lose"+character.name.Replace(" ", ""), true);
                    //reset dialogue so you can be friends again
                    usedDialogue.Except(new[]{"Warning"+character.name.Replace(" ", ""), "Gain"+character.name.Replace(" ", "")});
                }
            } else if (opinion <= character.loseFriendThresholds[1]){
                //warning
                if (characterObject.activeSelf){
                    dialogue("Warning"+character.name.Replace(" ", ""), false);
                }
            } else {
                //it's broken oops
            }
        }
    }

    private void resetBedroom(){
        workList.ForEach(k => k.SetActive(false));
        sleepList.ForEach(k => k.SetActive(false));
        partyList.ForEach(k => k.SetActive(false));
        sounds.ForEach(k => k.SetActive(false));
    }

    // [MenuItem("My Game/Cheats/Change Stat")]
    // public static void ChangeStats()
    // {
    //     if (Application.isPlaying)
    //     {
    //         //TODO: unlock code here...
    //     } else {
    //         Debug.LogError("Not in play mode.");
    //     }
    // }

    //debug method
    public void ReloadTasks(){
        Debug.Log("reloading tasks");
        tutorialCompleted = true;

        completedTasks.Clear();

        foreach (var item in activeTasks)
        {
            item.SetActive(false);
        }

        foreach (GameObject item in partyList)
        {
            item.SetActive(false);
        }
        sounds.ForEach(k => k.SetActive(false));
        sounds.First(k => k.name == "RelaxingAndy").SetActive(true);
        directionalLight.SetActive(true);

        activeTasks.Clear();

        completedTasks.Add("Wakeup", null);

        updateCharacters();
        checkSpecialTasks();
    } 
}