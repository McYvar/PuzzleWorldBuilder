using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditorState : BaseState
{
    public static List<GameEditor> editors = new List<GameEditor>();

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
    }

    public override void OnUpdate()
    {
        foreach (GameEditor editor in editors)
        {
            editor.EditorUpdate();
        }
    }

    public void SwitchToPlayState()
    {
        // code for saving the current level and then play it!
        stateManager.SwitchState(typeof(PlayState));
    }
}
