using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditorState : BaseState
{
    public static List<AbstractGameEditor> editors = new List<AbstractGameEditor>();
    public static Queue<AbstractGameEditor> newEditorsQueue = new Queue<AbstractGameEditor>();
    public static Queue<AbstractGameEditor> removeEditorsQueue = new Queue<AbstractGameEditor>();

    public override void OnAwake()
    {
        Editor();
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
        foreach (AbstractGameEditor editor in editors)
        {
            editor.EditorUpdate();
        }
    }

    void Editor()
    {
        List<AbstractGameEditor> addedEditor = new List<AbstractGameEditor>();
        while (newEditorsQueue.Count > 0)
        {
            AbstractGameEditor editor = newEditorsQueue.Dequeue();
            addedEditor.Add(editor);
            editors.Add(editor);
        }

        while (removeEditorsQueue.Count > 0)
        {
            AbstractGameEditor editor = removeEditorsQueue.Dequeue();
            if (editor != null)
                if (editors.Contains(editor))
                    editors.Remove(editor);
        }

        foreach (AbstractGameEditor editor in addedEditor)
        {
            editor.EditorAwake();
        }
        foreach (AbstractGameEditor editor in addedEditor)
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
