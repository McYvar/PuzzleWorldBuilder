using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayState : BaseState
{
    public static List<GameRunner> runners = new List<GameRunner>();


    public override void OnAwake()
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
        foreach (GameRunner runner in runners)
        {
            runner.RunnerFixedUpdate();
        }
    }

    public override void OnUpdate()
    {
        foreach (GameRunner runner in runners)
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
