using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EditorState : BaseState
{
    public static Queue<EditorBase> editorsAddQueue = new Queue<EditorBase>();
    public static Queue<EditorBase> editorsRemoveQueue = new Queue<EditorBase>();
    public static List<EditorBase> editors = new List<EditorBase>();

    [SerializeField] InputCommands inputCommands;

    public override void OnAwake()
    {
    }

    public override void OnStart()
    {
    }

    public override void OnEnter()
    {
        foreach (GridObject go in GridObject.gridObjects)
        {
            go.OnEditMode();
        }
    }

    public override void OnExit()
    {
    }

    public override void OnFixedUpdate()
    {
        foreach (EditorBase editor in editors)
        {
            editor.OnFixedUpdate();
        }
    }

    public override void OnUpdate()
    {
        Debug.Log(editors.Count);
        foreach (EditorBase editor in editors)
        {
            editor.OnUpdate();
        }

        while (editorsAddQueue.Count > 0)
        {
            editors.Add(editorsAddQueue.Dequeue());
        }

        while (editorsRemoveQueue.Count > 0)
        {
            editors.Remove(editorsRemoveQueue.Dequeue());
        }
    }

    public override void OnLateUpdate()
    {
        foreach (EditorBase editor in editors)
        {
            editor.OnLateUpdate();
        }
    }

    public void SwitchToPlayState()
    {
        // code for saving the current level and then play it!
        stateManager.SwitchState(typeof(PlayState));
    }
}
