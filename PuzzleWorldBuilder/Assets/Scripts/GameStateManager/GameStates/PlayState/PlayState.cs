using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayState : BaseState
{
    public static List<AbstractGameRunner> runners = new List<AbstractGameRunner>();
    public static Queue<AbstractGameRunner> newRunnesQueue = new Queue<AbstractGameRunner>();
    public static Queue<AbstractGameRunner> removeRunnesQueue = new Queue<AbstractGameRunner>();

    List<AbstractGameRunner> addedRunners;
    [SerializeField] InputCommands inputCommands;

    [SerializeField] PlayerStateManager playerStateManager;

    public override void OnAwake()
    {
        Runners();
        RunnersOnAwake();
    }

    public override void OnStart()
    {
        RunnersOnStart();
        addedRunners.Clear();
    }

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
        foreach (AbstractGameRunner runner in runners)
        {
            runner.RunnerFixedUpdate();
        }
    }

    public override void OnUpdate()
    {
        Runners();
        RunnersOnAwake();
        RunnersOnStart();
        addedRunners.Clear();
        foreach (AbstractGameRunner runner in runners)
        {
            runner.RunnerUpdate();
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
        foreach (AbstractGameRunner runner in runners)
        {
            runner.RunnerLateUpdate();
        }
    }

    void Runners()
    {
        addedRunners = new List<AbstractGameRunner>();
        while (newRunnesQueue.Count > 0)
        {
            AbstractGameRunner runner = newRunnesQueue.Dequeue();
            addedRunners.Add(runner);
            runners.Add(runner);
        }

        while (removeRunnesQueue.Count > 0)
        {
            AbstractGameRunner runner = removeRunnesQueue.Dequeue();
            if (runner != null) 
                if (runners.Contains(runner))
                    runners.Remove(runner);
        }
    }

    void RunnersOnAwake()
    {
        foreach (AbstractGameRunner runner in addedRunners)
        {
            runner.RunnerAwake();
        }
    }
    
    void RunnersOnStart()
    {
        foreach (AbstractGameRunner runner in addedRunners)
        {
            runner.RunnerStart();
        }
    }

    public void SwitchToEditState()
    {
        // code for loading the current level
        stateManager.SwitchState(typeof(EditorState));
    }

}
