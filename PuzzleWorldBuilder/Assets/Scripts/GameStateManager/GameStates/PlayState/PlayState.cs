using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayState : BaseState
{
    public static List<RunnerBase> runners = new List<RunnerBase>();
    public static Queue<RunnerBase> newRunnesQueue = new Queue<RunnerBase>();
    public static Queue<RunnerBase> removeRunnesQueue = new Queue<RunnerBase>();

    [SerializeField] InputCommands inputCommands;

    [SerializeField] PlayerStateManager playerStateManager;

    public override void OnEnter()
    {
        playerStateManager.SwitchState(typeof(IdleState));

        foreach (GridObject go in GridObject.gridObjects)
        {
            go.OnPlayMode();
        }
    }

    public override void OnExit()
    {
        playerStateManager.OnStopPlayMode();
    }

    public override void OnFixedUpdate()
    {
        foreach (RunnerBase runner in runners)
        {
            runner.OnFixedUpdate();
        }
    }

    public override void OnUpdate()
    {
        foreach (RunnerBase runner in runners)
        {
            runner.OnUpdate();
        }

        while (newRunnesQueue.Count > 0)
        {
            runners.Add(newRunnesQueue.Dequeue());
        }

        while (removeRunnesQueue.Count > 0)
        {
            runners.Remove(removeRunnesQueue.Dequeue());
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            stateManager.SwitchState(typeof(EditorState)); // has to switch to an ingame ui menu instead of to edit state later
            DataPersistenceManager.instance.LoadFile();
            inputCommands.transform.parent.gameObject.SetActive(true);
            inputCommands.ResetTool();
        }
    }

    public override void OnLateUpdate()
    {
        foreach (RunnerBase runner in runners)
        {
            runner.OnLateUpdate();
        }
    }

    public void SwitchToEditState()
    {
        // code for loading the current level
        stateManager.SwitchState(typeof(EditorState));
    }

}
