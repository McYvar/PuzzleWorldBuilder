using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditorState : BaseState
{
    public static List<AbstractGameEditor> editors = new List<AbstractGameEditor>();
    public static Queue<AbstractGameEditor> newEditorsQueue = new Queue<AbstractGameEditor>();
    public static Queue<AbstractGameEditor> removeEditorsQueue = new Queue<AbstractGameEditor>();

    List<AbstractGameEditor> addedEditors;

    public override void OnAwake()
    {
        Editor();
        EditorsOnAwake();
    }

    public override void OnStart()
    {
        EditorsOnStart();
        addedEditors.Clear();
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
        Editor();
        EditorsOnAwake();
        EditorsOnStart();
        addedEditors.Clear();
        foreach (AbstractGameEditor editor in editors)
        {
            editor.EditorUpdate();
        }
    }

    void Editor()
    {
        addedEditors = new List<AbstractGameEditor>();
        while (newEditorsQueue.Count > 0)
        {
            AbstractGameEditor editor = newEditorsQueue.Dequeue();
            addedEditors.Add(editor);
            editors.Add(editor);
        }

        while (removeEditorsQueue.Count > 0)
        {
            AbstractGameEditor editor = removeEditorsQueue.Dequeue();
            if (editor != null)
                if (editors.Contains(editor))
                    editors.Remove(editor);
        }

    }

    public void EditorsOnAwake()
    {
        foreach (AbstractGameEditor editor in addedEditors)
        {
            editor.EditorAwake();
        }
    }

    public void EditorsOnStart()
    {
        foreach (AbstractGameEditor editor in addedEditors)
        {
            editor.EditorStart();
        }
    }

    public void SwitchToPlayState()
    {
        // code for saving the current level and then play it!
        stateManager.SwitchState(typeof(PlayState));
    }
}
