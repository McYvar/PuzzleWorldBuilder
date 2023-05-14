using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseEditorCommand : EditorBase, ICommand
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

    public virtual void ClearFirstUndo()
    {
    }

    public virtual void ClearRedo()
    {
    }
}
