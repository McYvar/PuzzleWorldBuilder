using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayState : BaseState
{
    public static List<AbstractGameRunner> runners = new List<AbstractGameRunner>();


    public override void OnAwake()
    {
    }

    public override void OnStart()
    {
    }

    public override void OnEnter()
    {
    }

    public override void OnExit()
    {
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
        foreach (AbstractGameRunner runner in runners)
        {
            runner.RunnerUpdate();
        }
    }

    public void SwitchToEditState()
    {
        // code for loading the current level
        stateManager.SwitchState(typeof(EditorState));
    }
}
