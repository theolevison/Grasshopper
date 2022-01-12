using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class DebugController : MonoBehaviour
{
    private bool showConsole;
    private string input;
    public List<object> commandList;
    public static DebugCommand RELOAD_TASKS;
    public static DebugCommand<string> SET_STAT; 

    public void OnToggleDebug(InputValue value){
        showConsole = !showConsole;
        
    }

    public void OnReturn(InputValue value){
        if ( showConsole){
            HandleInput();
            input = "";
        }
    }

    private void Awake() {
        RELOAD_TASKS = new DebugCommand("reload_tasks", "reloads all tasks", "reload_tasks", () => {
            Controller.Instance.ReloadTasks();
        });

        SET_STAT = new DebugCommand<string>("set_stat", "sets a stat to the provided value, stat=value", "set_stat", (x) => {
            string stat = x.Split('=')[0];
            int value = int.Parse(x.Split('=')[1]);
            Controller.Instance.stats[stat] = value;
            Debug.Log("changed " + stat + " to " + value);
        });

        commandList = new List<object>{
            RELOAD_TASKS,
            SET_STAT
        };
    }

    private void OnGUI(){
        if (!showConsole){
            return;
        }

        float y = 0f;

        GUI.Box(new Rect(0, y, Screen.width, 30), "");
        GUI.backgroundColor = new Color(0,0,0,0);
        input = GUI.TextField(new Rect(10f, y + 5f, Screen.width - 20f, 20f), input);
    }

    public void HandleInput(){
        string[] properties = input.Split(' ');

        for (int i = 0; i < commandList.Count; i++)
        {
            DebugCommandBase commandBase = commandList[i] as DebugCommandBase;

            if (input.Contains(commandBase.commandId)){
                if (commandList[i] as DebugCommand != null){
                    (commandList[i] as DebugCommand).Invoke();
                } else if (commandList[i] as DebugCommand<string> != null){
                    (commandList[i] as DebugCommand<string>).Invoke(properties[1]);
                }
            }
        }
    }
}
