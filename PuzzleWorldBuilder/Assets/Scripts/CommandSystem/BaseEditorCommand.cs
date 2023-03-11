using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseEditorCommand : AbstractGameEditor, ICommand
{
    [SerializeField] KeyCode baseKey;

    public bool addToUndo { get; set; }

    protected override void OnEnable()
    {
        base.OnEnable();
        addToUndo = true;
        InputCommands.AddKeyCommand(baseKey, this);
    }
    protected override void OnDisable()
    {
        base.OnDisable();
        InputCommands.RemoveKeyCommand(baseKey);
    }

    public virtual void Execute()
    {
    }

    public virtual void Undo()
    {
    }

    public virtual void Redo()
    {
    }

    public override void EditorAwake()
    {
        Debug.Log(this);
    }

    public override void EditorStart()
    {
    }

    public override void EditorUpdate()
    {
    }
}
